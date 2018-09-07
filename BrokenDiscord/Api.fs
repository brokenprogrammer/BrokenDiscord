module BrokenDiscord.RESTful

open System
open FSharpPlus

open BrokenDiscord.Types
open BrokenDiscord.Json.Json
open Newtonsoft.Json

open Hopac
open Hopac.Infixes
open HttpFs.Client
    
let private userAgent =
    sprintf "DiscordBot (%s, %s)"
    <| "https://github.com/brokenprogrammer/BrokenDiscord"
    <| "6"


let private setHeaders token req =
    req
    |> Request.setHeader (Authorization (sprintf "Bot %s" token))
    |> Request.setHeader (UserAgent userAgent)

let private basePath = sprintf "https://discordapp.com/api/%s"

module Response =
    let parseRtn<'t> r = 
        let s = Response.readBodyAsString r
        try s >>- ofJson<'t> >>- Ok
        with :? JsonException -> s >>- ofJson<ApiError> >>- Error

    let parseStat r = job {
            let stat = r.statusCode
            if stat - 200 < 100 then return Ok ()
            else return! Response.readBodyAsString r >>- ofJson<ApiError> >>- Error
        }

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
        |> getResponse
        
    let call<'i, 'o> token method path body =
        enqueue<'i> token method path body >>= Response.parseRtn<'o>

    let thunk<'i> token method path (body : 'i option) =
        enqueue<'i> token method path body >>= Response.parseStat

let restForm<'t> token method path (data : FormData list) =
    Request.createUrl method <| basePath path
    |> setHeaders token
    |> Request.body (RequestBody.BodyForm data)
    |> getResponse >>= Response.parseRtn<'t>

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
