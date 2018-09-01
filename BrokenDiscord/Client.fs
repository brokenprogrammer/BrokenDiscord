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
        let endpoint = String.Format("/channels/{0}/messages", channelId)
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
        api.DELETE(endpoint) |> Async.RunSynchronously

    /// Edit a previously sent message.
    /// Returns a message object
    member this.EditMessage (channelId : Snowflake, messageId : Snowflake, jsonParams : WebEditMessageParams) =
        let endpoint = String.Format("/channels/{0}/messages/{1}", channelId, messageId)
        api.PUT<Message>(endpoint, (jsonParams |> toJson)) |> Async.RunSynchronously

    /// Delete a message.
    member this.DeleteMessage (channelId : Snowflake, messageId : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/messages/{1}", channelId, messageId)
        api.DELETE(endpoint) |> Async.RunSynchronously

    /// Delete multiple messages in a single request.
    member this.BulkDeleteMessages (channelId : Snowflake, messageIds : list<Snowflake>) =
        let endpoint = String.Format("/channels/{0}/messages/bulk-delete", channelId)
        api.POST(endpoint, (messageIds |> toJson)) |> Async.RunSynchronously

    /// Edit the channel permission overwrites for a user or role in a channel.
    member this.EditChannelPermissions (channelId : Snowflake, overwriteId : Snowflake, 
                                        jsonParams : WebEditChannelPermissionsParams) = 
        let endpoint = String.Format("/channels/{0}/permissions/{1}", channelId, overwriteId)
        api.PUT(endpoint, (jsonParams |> toJson)) |> Async.RunSynchronously |> ignore
    
    /// Returns a list of invite objects for the channel.
    member this.GetChannelInvites (channelId : Snowflake) =
        let endpoint = String.Format("/channels/{0}/invites", channelId)
        api.GET<Invite>(endpoint) |> Async.RunSynchronously
    
    /// Create a new invite object for the channel.
    member this.CreateChannelInvite (channelId : Snowflake, jsonParams : WebCreateChannelInviteParams)= 
        let endpoint = String.Format("/channels/{0}/invites", channelId)
        api.POST<Invite>(endpoint, (jsonParams |> toJson)) |> Async.RunSynchronously
        
    /// Delete a channel permission overwrite for a user or role in a channel.
    member this.DeleteChannelPermission (channelId : Snowflake, overwriteId : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/permissions/{1}", channelId, overwriteId)
        api.DELETE(endpoint) |> Async.RunSynchronously |> ignore
    
    /// Post a typing indicator for the specified channel. 
    member this.TriggerTypingIndicator (channelId : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/typing", channelId)
        api.POST(endpoint, "") |> Async.RunSynchronously |> ignore
    
    /// Returns all pinned messages in the channel as an array of message objects.
    member this.GetPinnedMessages (channelId : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/typing", channelId)
        api.GET<list<Message>>(endpoint) |> Async.RunSynchronously
    
    /// Pin a message in a channel.
    member this.AddPinnedChannelMessage (channelId : Snowflake, messageId : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/pins/{1}", channelId, messageId)
        api.PUT(endpoint, "") |> Async.RunSynchronously |> ignore
    
    /// Delete a pinned message in a channel. 
    member this.DeletePinnedChannelMessage (channelId : Snowflake, messageId : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/pins/{1}", channelId, messageId)
        api.DELETE(endpoint) |> Async.RunSynchronously |> ignore

    /// Adds a recipient to a Group DM using their access token.
    member this.GroupDMAddRecipient (channelId : Snowflake, userId : Snowflake, 
                                     jsonParams : WebGroupDMAddRecipientParams) = 
        let endpoint = String.Format("/channels/{0}/recipients/{1}", channelId, userId)
        api.PUT(endpoint, "") |> Async.RunSynchronously |> ignore
    
    /// Removes a recipient from a Group DM.
    member this.GroupDMRemoveRecipient (channelId : Snowflake, userId : Snowflake) =
        let endpoint = String.Format("/channels/{0}/recipients/{1}", channelId, userId)
        api.DELETE(endpoint) |> Async.RunSynchronously |> ignore

    interface System.IDisposable with
        member this.Dispose () =
            (gw :> IDisposable).Dispose()
            (api :> IDisposable).Dispose()