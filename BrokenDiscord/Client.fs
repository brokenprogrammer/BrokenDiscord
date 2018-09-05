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
    
    let mutable Sessionid = 0

    member val GatewayVersion = 0 with get, set
    member val PrivateChannels = [] with get, set
    member val Guilds = [] with get,set
    
    member val Events = gw.GatewayEvent
    
    member this.login() = token |> gw.connect |> Async.RunSynchronously

    /// Get a channel by ID. Returns a channel object.
    member this.GetChannel (channelid : Snowflake) = 
        let endpoint = String.Format("/channels/{0}", channelid)
        api.GET<Channel>(endpoint) |> Async.RunSynchronously 
    
    /// Update a channels settings. Returns a channel on success, 
    /// and a 400 BAD REQUEST on invalid parameters.
    member this.ModifyChannel (channelid : Snowflake, jsonParams : WebModifyChannelParams) = 
        let endpoint = String.Format("/channels/{0}", channelid)
        let json = jsonParams |> toJson
        api.PUT<Channel>(endpoint, json) |> Async.RunSynchronously

    /// Delete a channel, or close a private message.
    /// Returns a channel object on success.
    member this.DeleteChannel (channelid : Snowflake) =
        let endpoint = String.Format("/channels/{0}", channelid)
        api.DELETE<Channel>(endpoint) |> Async.RunSynchronously

    /// Returns the messages for a channel.
    /// Returns an array of message objects on success.
    member this.GetChannelMessages (channelid : Snowflake, jsonParams : WebGetChannelMessagesParams) =
        //TODO: Query parameters.
        let endpoint = String.Format("/channels/{0}/messages", channelid)
        let json = jsonParams |> toJson
        api.GET<list<Message>>(endpoint) |> Async.RunSynchronously

    /// Returns a specific message in the channel. 
    /// Returns a message object on success.
    member this.GetChannelMessage (channelid : Snowflake, messageid : Snowflake) =
        let endpoint = String.Format("/channels/{0}/messages/{1}", channelid, messageid)
        api.GET<Message>(endpoint) |> Async.RunSynchronously
    
    /// Post a message to a guild text or DM channel.
    member this.CreateMessage (channelid : Snowflake, message : WebCreateMessageParams) =
        //TODO: Might have to be restructured to work with uploading files.
        let endpoint = String.Format("/channels/{0}/messages", channelid)
        let json = message |> toJson
        api.POST<Message>(endpoint, json) |> Async.RunSynchronously

    /// Create a reaction for the message. 
    member this.CreateReaction (channelid : Snowflake, messageid : Snowflake, emoji : Emoji) = 
        let emojiVal = match emoji.id with
                    | Some id -> "" + (string id) + ":" + emoji.name
                    | None -> emoji.name

        let endpoint = String.Format("/channels/{0}/messages/{1}/reactions/{2}/@me", channelid, messageid, emojiVal)
        api.PUT(endpoint, "") |> Async.RunSynchronously |> ignore
    
    /// Delete a reaction the current user has made for the message.
    member this.DeleteOwnReaction (channelid : Snowflake, messageid : Snowflake, emoji : Emoji) = 
        let emojiVal = match emoji.id with
                    | Some id -> "" + (string id) + ":" + emoji.name
                    | None -> emoji.name

        let endpoint = String.Format("/channels/{0}/messages/{1}/reactions/{2}/@me", channelid, messageid, emojiVal)
        api.DELETE(endpoint) |> Async.RunSynchronously |> ignore
    
    /// Deletes another user's reaction. 
    member this.DeleteUserReaction (channelid : Snowflake, messageid : Snowflake, emoji : Emoji, userid : Snowflake) =
        let emojiVal = match emoji.id with
                    | Some id -> "" + (string id) + ":" + emoji.name
                    | None -> emoji.name

        let endpoint = String.Format("/channels/{0}/messages/{1}/reactions/{2}/{3}", channelid, messageid, emojiVal, userid)
        api.DELETE(endpoint) |> Async.RunSynchronously |> ignore

    /// Get a list of users that reacted with this emoji. 
    /// Returns an array of user objects on success.
    member this.GetReactions (channelid : Snowflake, messageid : Snowflake, emoji : Emoji, jsonParams : WebGetReactionsParams) =
        //TODO: Query params
        let emojiVal = match emoji.id with
                    | Some id -> "" + (string id) + ":" + emoji.name
                    | None -> emoji.name

        let endpoint = String.Format("/channels/{0}/messages/{1}/reactions/{2}", channelid, messageid, emojiVal)
        api.GET<list<User>>(endpoint) |> Async.RunSynchronously

    /// Deletes all reactions on a message.
    member this.DeleteAllReactions (channelid : Snowflake, messageid : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/messages/{1}/reactions", channelid, messageid)
        api.DELETE(endpoint) |> Async.RunSynchronously

    /// Edit a previously sent message.
    /// Returns a message object
    member this.EditMessage (channelid : Snowflake, messageid : Snowflake, jsonParams : WebEditMessageParams) =
        let endpoint = String.Format("/channels/{0}/messages/{1}", channelid, messageid)
        api.PUT<Message>(endpoint, (jsonParams |> toJson)) |> Async.RunSynchronously

    /// Delete a message.
    member this.DeleteMessage (channelid : Snowflake, messageid : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/messages/{1}", channelid, messageid)
        api.DELETE(endpoint) |> Async.RunSynchronously

    /// Delete multiple messages in a single request.
    member this.BulkDeleteMessages (channelid : Snowflake, messageids : list<Snowflake>) =
        let endpoint = String.Format("/channels/{0}/messages/bulk-delete", channelid)
        api.POST(endpoint, (messageids |> toJson)) |> Async.RunSynchronously

    /// Edit the channel permission overwrites for a user or role in a channel.
    member this.EditChannelPermissions (channelid : Snowflake, overwriteid : Snowflake, 
                                        jsonParams : WebEditChannelPermissionsParams) = 
        let endpoint = String.Format("/channels/{0}/permissions/{1}", channelid, overwriteid)
        api.PUT(endpoint, (jsonParams |> toJson)) |> Async.RunSynchronously |> ignore
    
    /// Returns a list of invite objects for the channel.
    member this.GetChannelInvites (channelid : Snowflake) =
        let endpoint = String.Format("/channels/{0}/invites", channelid)
        api.GET<Invite>(endpoint) |> Async.RunSynchronously
    
    /// Create a new invite object for the channel.
    member this.CreateChannelInvite (channelid : Snowflake, jsonParams : WebCreateChannelInviteParams)= 
        let endpoint = String.Format("/channels/{0}/invites", channelid)
        api.POST<Invite>(endpoint, (jsonParams |> toJson)) |> Async.RunSynchronously
        
    /// Delete a channel permission overwrite for a user or role in a channel.
    member this.DeleteChannelPermission (channelid : Snowflake, overwriteid : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/permissions/{1}", channelid, overwriteid)
        api.DELETE(endpoint) |> Async.RunSynchronously |> ignore
    
    /// Post a typing indicator for the specified channel. 
    member this.TriggerTypingIndicator (channelid : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/typing", channelid)
        api.POST(endpoint, "") |> Async.RunSynchronously |> ignore
    
    /// Returns all pinned messages in the channel as an array of message objects.
    member this.GetPinnedMessages (channelid : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/typing", channelid)
        api.GET<list<Message>>(endpoint) |> Async.RunSynchronously
    
    /// Pin a message in a channel.
    member this.AddPinnedChannelMessage (channelid : Snowflake, messageid : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/pins/{1}", channelid, messageid)
        api.PUT(endpoint, "") |> Async.RunSynchronously |> ignore
    
    /// Delete a pinned message in a channel. 
    member this.DeletePinnedChannelMessage (channelid : Snowflake, messageid : Snowflake) = 
        let endpoint = String.Format("/channels/{0}/pins/{1}", channelid, messageid)
        api.DELETE(endpoint) |> Async.RunSynchronously |> ignore

    /// Adds a recipient to a Group DM using their access token.
    member this.GroupDMAddRecipient (channelid : Snowflake, userid : Snowflake, 
                                     jsonParams : WebGroupDMAddRecipientParams) = 
        let endpoint = String.Format("/channels/{0}/recipients/{1}", channelid, userid)
        api.PUT(endpoint, "") |> Async.RunSynchronously |> ignore
    
    /// Removes a recipient from a Group DM.
    member this.GroupDMRemoveRecipient (channelid : Snowflake, userid : Snowflake) =
        let endpoint = String.Format("/channels/{0}/recipients/{1}", channelid, userid)
        api.DELETE(endpoint) |> Async.RunSynchronously |> ignore

    interface System.IDisposable with
        member this.Dispose () =
            (gw :> IDisposable).Dispose()
            (api :> IDisposable).Dispose()