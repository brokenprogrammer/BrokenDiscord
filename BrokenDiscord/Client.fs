module BrokenDiscord.Client

open BrokenDiscord.Gateway
open BrokenDiscord.RESTful
open BrokenDiscord.Types
open BrokenDiscord.Json
open BrokenDiscord.Json.Json

open System
open Events
open System.Net
open HttpFs.Client
open Hopac
open Hopac.Infixes

open FSharp.Control
open Newtonsoft.Json.Linq

let private channelEndpoint (id : Snowflake) = sprintf "/channels/%d" id
let private historyEndpoint   = channelEndpoint >> (+) "/messages"
let private messageEndpoint   = historyEndpoint >> sprintf "%s/%d"
let private reactionsEndpoint =
    messageEndpoint 

type Client (token : string) =
    let token = token
    let gw = new Gateway()
    
    let mutable Sessionid = 0

    member val GatewayVersion = 0 with get, set
    member val PrivateChannels = [] with get, set
    member val Guilds = [] with get,set
    
    member val Events = gw.GatewayEvent
    
    member this.login() = token |> gw.connect |> Async.RunSynchronously

    /// Get a channel by ID. Returns a channel object.

    member this.GetChannel (chid : Snowflake) = 
        restGet<unit,Channel> token <| channelEndpoint chid <| None
    
    /// Update a channels settings. Returns a channel on success, 
    /// and a 400 BAD REQUEST on invalid parameters.
    member this.EditChannel (args : WebModifyChannelParams) (chid:Snowflake) =
        restPatch<WebModifyChannelParams, Channel> token
        <| channelEndpoint chid <| Some args

    /// Delete a channel, or close a private message.
    /// Returns a channel object on success.
    member this.DeleteChannel (chid : Snowflake) =
        restDelete<unit,Channel> token <| channelEndpoint chid <| None

    /// Returns the messages for a channel.
    /// Returns an array of message objects on success.
    member this.GetChannelMessages (args : HistoryParams) (chid : Snowflake) =
        let retrieve =
            restGet<WebGetChannelMessagesParams, Message[]> token <| historyEndpoint chid
        asyncSeq {
            let! payload = retrieve (Some args.Payload) |> Job.toAsync
            let payload =
                match payload with
                | Ok x -> x
                | Error err -> raise <| ApiException err
            yield! AsyncSeq.ofSeq payload
            let remaining = 
                if args.limit > 100 then args.limit-100
                else 0
            if remaining > 0 then
                yield!
                    this.GetChannelMessages
                    <| args.ScrollBy remaining (Array.last payload).id
                    <| chid 
            }

    /// Returns a specific message in the channel. 
    /// Returns a message object on success.
    member this.GetChannelMessage (chid : Snowflake) (mgid : Snowflake) =
        restGet<unit,Message> token <| messageEndpoint chid mgid <| None
    
    /// Post a message to a guild text or DM channel.
    member this.CreateMessage (chid : Snowflake) (mgid : Snowflake) (args : MessageCreate) =
        //TODO: Might have to be restructured to work with uploading files.
        let unwrap = function Some x -> [x] | None -> []
        let body =
            let rc = 
                args.richContent
                |> Option.map
                    (fun rc -> NameValue(
                                "payload_json",
                                toJson <| JProperty("embed", toJson rc)))
            match args.richContent with
            | Some rc ->
                [ for f in rc.files do
                    yield FormData.FormFile("file", File(f.name, f.mime, StreamData f.content)) ]
            | None -> []
            |> List.append
                <| List.concat [
                    unwrap rc
                    args.nonce |> Option.map (fun x -> NameValue("nonce", string x)) |> unwrap
                    args.tts |> Option.map (fun x -> NameValue("tts", string x)) |> unwrap ]
         
        httpForm<Message> token Post
        <| messageEndpoint chid mgid
        <| body

    /// Create a reaction for the message. 
    member this.CreateReaction (chid : Snowflake) (mgid: Snowflake) (emote : Emoji) = 
    
    /// Delete a reaction the current user has made for the message.
    member this.DeleteOwnReaction (channelid : Snowflake, messageid : Snowflake, emoji : Emoji) =
   
    /// Deletes another user's reaction. 
    member this.DeleteUserReaction (channelid : Snowflake, messageid : Snowflake, emoji : Emoji, userid : Snowflake) =

    /// Get a list of users that reacted with this emoji. 
    /// Returns an array of user objects on success.
    member this.GetReactions (channelid : Snowflake, messageid : Snowflake, emoji : Emoji, jsonParams : WebGetReactionsParams) =
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