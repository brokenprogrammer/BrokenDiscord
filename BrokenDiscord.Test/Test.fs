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

let getenv x = try Some <| Environment.GetEnvironmentVariable x with _ -> None
if getenv "CI" = Some "true" then do
    // TODO: If any of these environment variables aren't available, warn the user about it
    // and hold off on mock tests.
    let mockvarkeys = seq [ "BDISCORDGUILDID"; "BDISCORDCHANNELID"; "BOTTOKEN" ]
    let mockvars = Seq.map getenv mockvarkeys
    let unavailable = Seq.zip mockvarkeys mockvars |> Seq.filter (fun (_, x) -> x = None) |> Seq.map fst
    if Seq.contains None mockvars then do
        printfn "WARN: mock testing variables %s unavailable. Skipping mock tests."
        <| String.concat ", " mockvarkeys
    else do
    // TODO: mock tests here! (e.g. MessageCreate).
        ()

/// TODO: Verify that JSON payloads are serialized as intended to check
/// against edge cases, e.g. literal "None" values
(*
{ content="this is a triumph" } |> toJson
|> should (= (Json.ofJson<WebEditMessageParams> """
*)
