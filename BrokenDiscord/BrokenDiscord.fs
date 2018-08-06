namespace BrokenDiscord

open Microsoft.AspNetCore.WebSockets

/// Keep this as the main entry-point of the library, handle the websocket as well as the session, heartbeat
/// Include what was the D gateway in this file / class.
/// It can keep track of the shards as well.
/// 

type BrokenDiscord() =
    let apiString = "https://discordapp.com/api/v6/"
    let gatewayString = "wss://gateway.discord.gg/?v=6&encoding=json"


    member this.X = "F#"