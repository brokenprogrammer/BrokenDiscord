module BrokenDiscord.Client

open BrokenDiscord.Gateway
open BrokenDiscord.Api
open BrokenDiscord.Types
open BrokenDiscord.Json
open BrokenDiscord.Json.Json

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

    /// Get a channel by ID. Returns a channel object.
    member this.GetChannel (channelId : Snowflake) = 
        let endpoint = String.Format("/channels/{0}", channelId)
        let channelJson = endpoint |> api.GET |> Async.RunSynchronously 
        channelJson |> ofJson<Channel>
    
    /// Update a channels settings. Returns a channel on success, 
    /// and a 400 BAD REQUEST on invalid parameters.
    member this.ModifyChannel (channelId : Snowflake) (jsonParams : WebModifyChannelParams) = 
        let endpoint = String.Format("/channels/{0}", channelId)
        let json = jsonParams |> toJson
        let channelJson = api.PUT endpoint json |> Async.RunSynchronously
        match channelJson with
        | Some x -> ofJson<Channel> |> Some
        | None -> None

    /// Delete a channel, or close a private message.
    /// Returns a channel object on success.
    member this.DeleteChannel (channelId : Snowflake) =
        let endpoint = String.Format("/channels/{0}", channelId)
        let channelJson = endpoint |> api.DELETE |> Async.RunSynchronously
        match channelJson with
        | Some x -> ofJson<Channel> |> Some
        | None -> None

    member this.GetChannelMessages (channelId : Snowflake) =
        0

    member this.GetChannelMessage (channelId : Snowflake) (messageId : Snowflake) =
        0
    
    ///Post a message to a guild text or DM channel.
    member this.CreateMessage (channelId : Snowflake) =
        0

    interface System.IDisposable with
        member this.Dispose () =
            (gw :> IDisposable).Dispose()
            (api :> IDisposable).Dispose()