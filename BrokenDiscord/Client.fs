module BrokenDiscord.Client

open BrokenDiscord.Gateway
open BrokenDiscord.Api
open BrokenDiscord.Types

open System
open Events

type Client (token : string) =
    let token = token

    let gw = new Gateway()
    let api = new Api(token)
    
    let mutable SessionId = 0

    member val GatewayVersion = 0 with get, set
    member val PrivateChannels = [] with get, set
    member val Guilds = [] with get,set
    
    member val Events = gw.GatewayEvent
    
    member this.login() = token |> gw.connect |> Async.RunSynchronously

    ///Get a channel by ID. Returns a channel object.
    member this.GetChannel(id : Snowflake) = 
        let endpoint = String.Format("/channels/{0}", id)
        let channelJson = endpoint |> api.GET |> Async.RunSynchronously 
        channelJson |> ofJson<Channel>

    interface System.IDisposable with
        member this.Dispose () =
            (gw :> IDisposable).Dispose()
            (api :> IDisposable).Dispose()