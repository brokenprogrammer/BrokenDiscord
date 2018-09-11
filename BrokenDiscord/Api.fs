module BrokenDiscord.RESTful

open System
open FSharpPlus

open BrokenDiscord.Types
open BrokenDiscord.Json.Json
open Newtonsoft.Json

open HttpFs.Client

open Hopac
open HttpFs.Client
open Chessie.Hopac.JobTrial
open Chessie.ErrorHandling
open Hopac.Infixes

let private userAgent =
    sprintf "DiscordBot (%s, %s)"
    <| "https://github.com/brokenprogrammer/BrokenDiscord"
    <| "6"

let private setHeaders token req =
    req
    |> Request.setHeader (Authorization (sprintf "Bot %s" token))
    |> Request.setHeader (UserAgent userAgent)

let private basePath = sprintf "https://discordapp.com/api/v6/%s"

module Ratelimiting =
    open Hopac
    type Target = Global | Route of string
    
    let routeStr (u : Uri) =
        let path =
            u.Segments 
            |> Seq.map (String.filter ((<>) '/'))
            |> Seq.filter ((<>) "")
            |> String.concat "/"
        sprintf "%s/%s" u.Host path      
    
    let routeFrom u = Route <| routeStr u
        
    type Cessation = 
        { target : Target; release : DateTime }
        with
        member x.Timeout =
            let span = x.release - DateTime.Now
            if span < TimeSpan(0L) then Alt.zero () else timeOut span
            
    let private inbox = new Ch<Cessation> ()
    let mutable private cache = Map.empty
    let private bulletin = new Event<Cessation> ()
    let cessationNotifier =
        job {
            while true do
                let! cessation = Ch.take inbox
                let { target=target; release=release } = cessation
                cache <- Map.add target release cache
                bulletin.Trigger cessation
                do! cessation.Timeout
                cache <- Map.remove target cache
        }
    let ceased = cache
    let cessations = bulletin.Publish
    let notify x =
        new Promise<unit>(Ch.send inbox x)
        |> Promise.read <|> x.Timeout
        
    let backoffTimeout (route : Target) =
        match cache.TryFind route with 
        | Some release -> { target=route; release=release}.Timeout
        | None -> Alt.once ()

open Ratelimiting
    
type RatelimError =
    { [<JsonProperty "retry_after">]
      retry_ms : uint32
      [<JsonProperty "global">]
      _global : bool }
      
module RatelimError =
    let cessation route x = 
        {   target = 
                if x._global then Ratelimiting.Target.Global
                else Ratelimiting.Target.Route route
            release = 
                float x.retry_ms
                |> TimeSpan.FromMilliseconds
                |> (+) DateTime.Now
        }
 
module Response =
    open Chessie.Hopac
    open HttpFs.Client

    let rateCk r =
        if r.statusCode = 429 then
            let cessation =
                Response.readBodyAsString r >>- ofJson<RatelimError>
                >>- RatelimError.cessation (routeStr r.responseUri)
            cessation >>= Ratelimiting.notify |> Job.startIgnore |> ignore
            cessation >>- FSharp.Core.Result.Error
        else Job.result <| FSharp.Core.Ok r
    
    let errCk r =
        if r.statusCode - 200 < 100 && r.statusCode <> 429 then
            Job.result <| Result.Succeed r
        else Response.readBodyAsString r 
                >>- ofJson<ApiError>
                >>- (fun x -> (r.statusCode, x))
                >>- Result.FailWith
    
    let rec rateGuard (req : Request) = 
        job {
            do! backoffTimeout Global
            do! backoffTimeout (Route <| routeStr req.url)
            let! rsp = getResponse req >>= rateCk
            match rsp with
                | FSharp.Core.Ok rsp -> return rsp
                | Error cessation -> 
                    do! notify cessation |> Job.startIgnore
                    do! cessation.Timeout
                    return! rateGuard req
        }
        
    let parseRtn<'t> r =
        rateGuard r >>= errCk |> ofJobOfResult
        |> mapFun (Response.readBodyAsString>>run>>ofJson<'t>)
        |> toJobOfResult

    let parseStat = 
        rateGuard >> Job.bind errCk
        >> ofJobOfResult >> mapFun ignore
        >> toJobOfResult

module Request =
    let jsonBody<'t> (x : 't option) =
        match x with
        | Some x ->
            Request.bodyString (toJson x)
            >> Request.setHeader
                (ContentType <| ContentType.create ("application", "json"))
        | _ when typeof<'t> = typeof<unit> -> id
        | None -> id
    
    let enqueue<'i> token method path (body: 'i option) =
        Request.createUrl method <| basePath path
        |> setHeaders token
        |> jsonBody<'i> body
        
    let call<'i, 'o> token method path body =
        enqueue<'i> token method path body |> Response.parseRtn<'o>

    let thunk<'i> token method path (body : 'i option) =
        enqueue<'i> token method path body |> Response.parseStat

let restForm<'t> token method path (data : FormData list) =
    Request.createUrl method <| basePath path
    |> setHeaders token
    |> Request.body (RequestBody.BodyForm data)
    |> Response.parseRtn<'t>

// these expect return values from the cloud.
let restGetCall<'i, 'o>    = Request.call<'i, 'o> /> Get
let restDelCall<'i, 'o>    = Request.call<'i, 'o> /> Delete
let restPostCall<'i, 'o>   = Request.call<'i, 'o> /> Post
let restPutCall<'i, 'o>    = Request.call<'i, 'o> /> Put
let restPatchCall<'i, 'o>  = Request.call<'i, 'o> /> Patch

// these ignore any return values unless they're error payloads.
let restGetThunk<'i>   = Request.thunk<'i> /> Get
let restDelThunk<'i>   = Request.thunk<'i> /> Delete
let restPutThunk<'i>   = Request.thunk<'i> /> Put
let restPatchThunk<'i> = Request.thunk<'i> /> Patch
let restPostThunk<'i>  = Request.thunk<'i> /> Post
