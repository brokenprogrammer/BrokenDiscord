# BrokenDiscord [![Build Status](https://travis-ci.org/brokenprogrammer/BrokenDiscord.svg?branch=master)](https://travis-ci.org/brokenprogrammer/BrokenDiscord)

BrokenDiscord is a [Discord](https://discordapp.com) API Library written in [F#](https://fsharp.org/). The main purpose is to provide a functional alternative that is both clean and easy to use. 

## Installation
The library is soon to be added to NuGet.

## Usage
The classic ping pong example can be found [here](https://github.com/brokenprogrammer/BrokenDiscord/blob/master/BrokenDiscord.Examples/Ping/Ping.fs). More examples will be added in the future.

First you want to import the following modules
```fsharp
    open BrokenDiscord.Client
    open BrokenDiscord.Types
    open BrokenDiscord.Events
    open Hopac
```

You can then construct a new Discord client which can be used to access both the Discord web API and set callbacks for Discord evets.
```fsharp
let client = new Client("DISCORD_BOT_TOKEN_HERE")
```

In the main function of the program you can add event handlers to the client and then call the start function to start the client which will then connect to the Discord gateway using a websocket.
```fsharp
    // Add functionality for reacting to events here
    let handleEvents = function
        | Ready args -> ()
        | MessageCreate args -> ()
        | TypingStart args -> ()
        | _ -> ()

    [<EntryPoint>]
    let main argv =
        client.Events |> Event.add handleEvents
        client.start()
        Console.ReadLine() |> ignore 
        0
```

## Contributors
- [Oskar Mendel](https://github.com/brokenprogrammer) - Creator, maintainer
- [iseurie](https://github.com/iseurie) - Maintainer