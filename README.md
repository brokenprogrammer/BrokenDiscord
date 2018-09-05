# BrokenDiscord
Discord Library written in F#

[![Build Status](https://travis-ci.org/brokenprogrammer/BrokenDiscord.svg?branch=master)](https://travis-ci.org/brokenprogrammer/BrokenDiscord)

## PingPong
```fsharp
module Program
    open System
    open BrokenDiscord.Client
    open BrokenDiscord.Events
    open BrokenDiscord.Types

    let client = new Client("BOT TOKEN HERE")

    let handleMessage (message : Message) =
     if message.content.Equals("ping") then
      let msg = {content = "pong"; nonce = None; tts = None; file = None; embed = None; payload_json = None}
      client.CreateMessage(message.channelId, msg) |> ignore
        
    let handleEvents = function
     | MessageCreate m -> (handleMessage m)
     | _ -> ()

    [<EntryPoint>]
    let main argv =
     client.Events |> Event.add handleEvents
     client.login()
     Console.ReadLine() |> ignore 
     0
```
