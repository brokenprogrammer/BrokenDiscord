module BrokenDiscord.Test

open System
open NUnit.Framework
open FsUnit.TopLevelOperators
open FSharp.Control
open BrokenDiscord.Client
open BrokenDiscord.Types
open BrokenDiscord.Json
open Newtonsoft.Json

let getenv x = try Some <| Environment.GetEnvironmentVariable x with _ -> None
if getenv "CI" = Some "true" then do
    // TODO: If any of these environment variables aren't available, warn the user about it
    // and hold off on mock tests. *)
    // let [ gid; chid; token ] =
    //     [ "BDISCORDGUILDID"; "BDISCORDCHANNELID"; "BOTTOKEN" ]
    //     |> List.map getenv
    // TODO: mock tests here! (e.g. MessageCreate).
    ()

/// TODO: Verify that JSON payloads are serialized as intended to check
/// against edge cases, e.g. literal "None" values

// MessageCreate.T.New "test" |> Json.toJson |> Newtonsoft.Json.JsonReader
// |> should (= 
