module BrokenDiscord.Client

open BrokenDiscord.Gateway
open BrokenDiscord.Api
open BrokenDiscord.Types
open BrokenDiscord.Json
open BrokenDiscord.Json.Json

open System
open Events
open System.Net

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
        api.GET<Channel>(endpoint) |> Async.RunSynchronously 
    
    /// Update a channels settings. Returns a channel on success, 
    /// and a 400 BAD REQUEST on invalid parameters.
    member this.ModifyChannel (channelId : Snowflake, jsonParams : WebModifyChannelParams) = 
        let endpoint = String.Format("/channels/{0}", channelId)
        let json = jsonParams |> toJson
        api.PUT<Channel>(endpoint, json) |> Async.RunSynchronously

    /// Delete a channel, or close a private message.
    /// Returns a channel object on success.
    member this.DeleteChannel (channelId : Snowflake) =
        let endpoint = String.Format("/channels/{0}", channelId)
        api.DELETE<Channel>(endpoint) |> Async.RunSynchronously

    /// Returns the messages for a channel.
    /// Returns an array of message objects on success.
    member this.GetChannelMessages (channelId : Snowflake, jsonParams : WebGetChannelMessagesParams) =
        //TODO: Query parameters.
        let endpoint = String.Format("/channels/{0}/messages", channelId)
        let json = jsonParams |> toJson
        api.GET<list<Message>>(endpoint) |> Async.RunSynchronously

    /// Returns a specific message in the channel. 
    /// Returns a message object on success.
    member this.GetChannelMessage (channelId : Snowflake, messageId : Snowflake) =
        let endpoint = String.Format("/channels/{0}/messages/{1}", channelId, messageId)
        api.GET<Message>(endpoint) |> Async.RunSynchronously
    
    /// Post a message to a guild text or DM channel.
    member this.CreateMessage (channelId : Snowflake, message : WebCreateMessageParams) =
        //TODO: Might have to be restructured to work with uploading files.
        let endpoint = String.Format("/channels/{channel.id}/messages")
        let json = message |> toJson
        api.POST<Message>(endpoint, json) |> Async.RunSynchronously

    /// Create a reaction for the message. 
    member this.CreateReaction (channelId : Snowflake, messageId : Snowflake, emoji : Emoji) = 
        let emojiVal = match emoji.Id with
                    | Some id -> "" + (string id) + ":" + emoji.Name
                    | None -> emoji.Name

        let endpoint = String.Format("/channels/{0}/messages/{1}/reactions/{2}/@me", channelId, messageId, emojiVal)
        api.PUT(endpoint, "") |> Async.RunSynchronously |> ignore
    
    /// Delete a reaction the current user has made for the message.
    member this.DeleteOwnReaction (channelId : Snowflake, messageId : Snowflake, emoji : Emoji) = 
        let emojiVal = match emoji.Id with
                    | Some id -> "" + (string id) + ":" + emoji.Name
                    | None -> emoji.Name

        let endpoint = String.Format("/channels/{0}/messages/{1}/reactions/{2}/@me", channelId, messageId, emojiVal)
        api.DELETE(endpoint) |> Async.RunSynchronously |> ignore
    
    /// Deletes another user's reaction. 
    member this.DeleteUserReaction (channelId : Snowflake, messageId : Snowflake, emoji : Emoji, userId : Snowflake) =
        let emojiVal = match emoji.Id with
                    | Some id -> "" + (string id) + ":" + emoji.Name
                    | None -> emoji.Name

        let endpoint = String.Format("/channels/{0}/messages/{1}/reactions/{2}/{3}", channelId, messageId, emojiVal, userId)
        api.DELETE(endpoint) |> Async.RunSynchronously |> ignore

    /// Get a list of users that reacted with this emoji. 
    /// Returns an array of user objects on success.
    member this.GetReactions (channelId : Snowflake, messageId : Snowflake, emoji : Emoji, jsonParams : WebGetReactionsParams) =
        //TODO: Query params
        let emojiVal = match emoji.Id with
                    | Some id -> "" + (string id) + ":" + emoji.Name
                    | None -> emoji.Name

        let endpoint = String.Format("/channels/{0}/messages/{1}/reactions/{2}", channelId, messageId, emojiVal)
        api.GET<list<User>>(endpoint) |> Async.RunSynchronously

    /// Deletes all reactions on a message.
    member this.DeleteAllReactions (channelId : Snowflake, messageId : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/messages/{1}/reactions", channelId, messageId)
        api.DELETE(endpoint)

    /// Edit a previously sent message.
    /// Returns a message object
    member this.EditMessage (channelId : Snowflake, messageId : Snowflake, jsonParams : WebEditMessageParams) =
        let endpoint = String.Format("/channels/{0}/messages/{1}", channelId, messageId)
        api.PUT<Message>(endpoint, (jsonParams |> toJson))

    /// Delete a message.
    member this.DeleteMessage (channelId : Snowflake, messageId : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/messages/{1}", channelId, messageId)
        api.DELETE(endpoint)

    /// Delete multiple messages in a single request.
    member this.BulkDeleteMessages (channelId : Snowflake, messageIds : list<Snowflake>) =
        let endpoint = String.Format("/channels/{0}/messages/bulk-delete", channelId)
        api.POST(endpoint, (messageIds |> toJson))

    member this.EditChannelPermissions = 0
    member this.GetChannelInvites = 0
    member this.CreateChannelInvite = 0
    member this.DeleteChannelPermission = 0
    member this.TriggerTypingIndicator = 0
    member this.GetPinnedMessages = 0
    member this.AddPinnedChannelMessage = 0
    member this.DeletePinnedChannelMessage = 0
    member this.GroupDMAddRecipient = 0
    member this.GroupDMRemoveRecipient = 0

    interface System.IDisposable with
        member this.Dispose () =
            (gw :> IDisposable).Dispose()
            (api :> IDisposable).Dispose()