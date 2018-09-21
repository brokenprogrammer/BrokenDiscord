module Ping

// Learn more about F# at http://fsharp.org

open System
open BrokenDiscord.Client
open BrokenDiscord.Events
open BrokenDiscord.Types
open Hopac
open Hopac.Infixes

let token = Environment.GetEnvironmentVariable "PING_BOT_TOKEN"
let client = new Client(token)

let pong (m : Message) =
    job {
        if m.content = "!ping" then
            return! client.CreateMessage m.channelId <| MessageCreate.T.New "pong!"
                    |> Job.startIgnore
        else return ()
    } |> start

let handleEvents = function
    | MessageCreate m -> (pong m)
    | _ -> ()

[<EntryPoint>]
let main _argv =
    client.Events
    |> Event.add handleEvents
    printfn "Listening for pings..."
    client.start()
    printfn "Press enter to quit"
    stdin.ReadLine () |> ignore
    0
