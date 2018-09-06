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

let parseRsp<'t> r = job {
        let! s = Response.readBodyAsString r
        return
            try Ok <| ofJson<'t> s
            with :? JsonException ->
                Error (ofJson<ApiError> s)
    }
    
let http<'i, 'o> token method path (body : 'i option) =
    Request.createUrl method <| basePath path
    |> setHeaders token
    |> (match body with 
            | Some x -> Request.bodyString (toJson x)
            | _ when typeof<'i> = typeof<unit> -> id
            | None -> id)
    |> getResponse >>= parseRsp<'o>

let httpForm<'t> token method path (data : FormData list) =
    Request.createUrl method <| basePath path
    |> setHeaders token
    |> Request.body (RequestBody.BodyForm data)
    |> getResponse >>= parseRsp<'t>
                
let restGet<'i, 'o>    = http<'i, 'o> /> Get
let restDelete<'i, 'o> = http<'i, 'o> /> Delete
let restPost<'i, 'o>   = http<'i, 'o> /> Post
let restPut<'i, 'o>    = http<'i, 'o> /> Put
let restPatch<'i, 'o>  = http<'i, 'o> /> Patch
