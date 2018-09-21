module MessageEmbed

// Learn more about F# at http://fsharp.org

open System
open BrokenDiscord.Client
open BrokenDiscord.Events
open BrokenDiscord.Types
open Hopac
open Hopac.Infixes

let token = Environment.GetEnvironmentVariable "MESSAGEEMBED_BOT_TOKEN"
let client = new Client(token)

let sendMessageEmbed (m : Message) =
    job {
        if m.content = "!sendEmbed" then
            let embed = {Embed.Simple "My Title" "Cool Description" with color = Some 0xFFF}
            let message : MessageCreate.T = (MessageCreate.T.New "pong!").WithEmbed(embed)
            return! client.CreateMessage m.channelId message
                    |> Job.startIgnore
        else return ()
    } |> start

let handleEvents = function
    | MessageCreate m -> (sendMessageEmbed m)
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
