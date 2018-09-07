module BrokenDiscord.Test

open System
open NUnit.Framework
open FsUnit.TopLevelOperators
open FSharp.Control
open BrokenDiscord.Client
open BrokenDiscord.Types
open BrokenDiscord.Json
open BrokenDiscord.Json
open Newtonsoft.Json.Linq
open Chessie.ErrorHandling
open Chessie.ErrorHandling.Trial
open Hopac
open Hopac.Infixes


let getenv x = try Some <| Environment.GetEnvironmentVariable x with _ -> None

   
if getenv "CI" = Some "true" then do
    let mockvarkeys = [ "BDISCORDGUILDID"; "BDISCORDCHANNELID"; "BOTTOKEN" ]
    let mockvars = List.map getenv mockvarkeys
    let unavailable =
        Seq.zip mockvarkeys mockvars
        |> Seq.filter (fun (_, x) -> x = None)
        |> Seq.map fst
    if Seq.contains None mockvars then do
        printfn "WARN: mock testing vars unavailable (%s). Skipping mock tests."
        <| String.concat ", " mockvarkeys
    else do
        let [ gid; chid; token ] = List.choose id mockvars
        let [ gid; chid ] = List.map uint64 [ gid; chid ]
        let client = new Client(token)
        // TODO: Log mock error payloads, don't just throw exceptions
        let created =
            client.CreateMessage <| chid <| MessageCreate.T.New "this is a triumph"
            |> run |> returnOrFail
        client.EditMessage chid created.id
        <| { content="i'm making a note here: ***huge success***"; embed=None }
        |> run |> returnOrFail

/// TODO: Verify that JSON payloads are serialized as intended to check
/// against edge cases, e.g. literal "None" values
// { content="this is a triumph" } |> toJson
// |> should (=) (Json.ofJson<WebEditMessageParams> """"""
