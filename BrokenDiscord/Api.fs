module BrokenDiscord.RESTful

open System
open FSharpPlus

open BrokenDiscord.Types
open BrokenDiscord.Json.Json
open Newtonsoft.Json

open Chessie.ErrorHandling
open Chessie.ErrorHandling.Trial
open Hopac
open Hopac.Infixes
open HttpFs.Client
open Chessie.Hopac.JobTrial
    
let private userAgent =
    sprintf "DiscordBot (%s, %s)"
    <| "https://github.com/brokenprogrammer/BrokenDiscord"
    <| "6"

let private setHeaders token req =
    req
    |> Request.setHeader (Authorization (sprintf "Bot %s" token))
    |> Request.setHeader (UserAgent userAgent)

let private basePath = sprintf "https://discordapp.com/api/%s"

module Ratelimiting =
    type Target = Global | Route of string
    type Cessation = Target * DateTime
    let private cessations = new Mailbox<Cessation> ()
    let mutable private cache : Map<Target, DateTime> = Map.empty
    let private bulletin = new Event<Cessation> ()
    let private pager =
        job {
            while true do
                let! cessation = Mailbox.take cessations
                let target, release = cessation
                cache <- Map.add target release cache
                bulletin.Trigger cessation
                do! timeOut (DateTime.Now - release)
                cache <- Map.remove target cache
        }
    pager |> Job.startIgnore |> ignore
    let events = bulletin.Publish
    let ceased = cache
    let throttle = Mailbox.send cessations
    
type RatelimError =
    { [<JsonProperty "retry_after">]
      retry_ms : uint32
      [<JsonProperty "global">]
      _global : bool }
      
module RatelimError =
    let cessation route x = 
        let target =
            if x._global then Ratelimiting.Target.Global
            else Ratelimiting.Target.Route route
        target, TimeSpan.FromMilliseconds
                <| float x.retry_ms |> (+) DateTime.Now
 
module Response =
    open Chessie.Hopac

    let rateCk r =
        job {
            if r.statusCode = 429 then
                let cessation =
                    Response.readBodyAsString r >>- ofJson<RatelimError>
                    >>- RatelimError.cessation r.responseUri.LocalPath
                do! cessation >>= Ratelimiting.throttle
                return! cessation >>- FSharp.Core.Result.Error
            else return FSharp.Core.Ok r
        }
    
    let errCk r =
        job {
            if r.statusCode - 200 >= 100 then
                return! Response.readBodyAsString r >>- ofJson<ApiError> >>- Result.FailWith
            else return Result.Succeed r
        }
    
    let rateGuard r =
        job {
            let! r = getResponse r >>= rateCk
            let rec lp () = 
                match r with
                | FSharp.Core.Ok rsp -> Job.result <| rsp
                | Error (route, release) ->
                    job {
                        do! timeOut (release-DateTime.Now)
                        return! lp ()
                    }
            return! lp ()
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
        | Some x -> Request.bodyString (toJson x)
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
