// Learn more about F# at http://fsharp.org

open System
open BrokenDiscord.Client
open BrokenDiscord.Events
open BrokenDiscord.Types
open Hopac
open Hopac.Infixes

let token = sprintf "Bot %s" <| Environment.GetEnvironmentVariable "PING_BOT_TOKEN"
let client = new Client(token)

let pong (m : Message) =
    job {
        if not <| Option.defaultValue false m.author.bot && m.content = "!ping" then
            return! client.CreateMessage m.channelId <| MessageCreate.T.New "pong!"
                    |> Job.startIgnore
        else return ()
    } |> ignore

[<EntryPoint>]
let main _argv =
    let token = sprintf "Bot %s" <| Environment.GetEnvironmentVariable "PING_BOT_TOKEN"
    let client = new Client(token)
    client.Events
    |> Event.choose (function MessageCreate e -> Some e | _ -> None)
    |> Event.add pong
    printfn "Listening for pings..."
    client.subscribe () |> Async.StartAsTask |> ignore
    printfn "Press enter to quit"
    stdin.ReadLine () |> ignore
    0
