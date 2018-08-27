module BrokenDiscord.Client

open BrokenDiscord.Gateway

open System
open Events

type Client () =
    let gw = new Gateway()
    
    let mutable SessionId = 0

    member val GatewayVersion = 0 with get, set
    member val PrivateChannels = [] with get, set
    member val Guilds = [] with get,set
    
    member val Events = gw.GatewayEvent
    
    //TODO: Should take token in params.
    member this.login(token : string) = token |> gw.connect |> Async.RunSynchronously

    interface System.IDisposable with
        member this.Dispose () =
            (gw :> IDisposable).Dispose()