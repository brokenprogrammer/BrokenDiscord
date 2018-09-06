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
open FSharpPlus

let private uesc = System.Uri.EscapeDataString

let private channelEndpoint : (Snowflake->_) = sprintf "/channels/%d"
let private channelOverwriteEndpoint : (_->Snowflake->_) =
    channelEndpoint >> sprintf "%s/permissions/overwrite/%d"
let private channelTypingEndpoint = channelEndpoint >> (+) /> "/typing"
let private channelRoutingEndpoint : (_->Snowflake->_) =
    channelEndpoint >> sprintf "%s/recipients/%d"
let private channelPinsEndpoint = channelEndpoint >> (+) /> "/pins"
let private channelPinEndpoint : (_->Snowflake->_) =
    channelPinsEndpoint >> sprintf "%s/%d"
 
let private channelInvitesEndpoint =
    channelEndpoint >> (+) /> "/invites"
    
let private bulkDeleteEndpoint = channelEndpoint >> (+) /> "/bulk-delete"
let private historyEndpoint = channelEndpoint >> (+) "/messages"
let private messageEndpoint = historyEndpoint >> sprintf "%s/%d"
let private msgReactionsEndpoint mgid chid = messageEndpoint mgid chid + "/reactions"
let private emoteReactionsEndpoint chid mgid (e : Emoji) =
    msgReactionsEndpoint chid mgid
    + (sprintf "/%s"
        <| Option.defaultValue e.name (Option.map string e.id))
    |> uesc
        
let private userReactionsEndpoint chid mgid e (u : USpec) =
    emoteReactionsEndpoint chid mgid e + sprintf "/%s" (string u)

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
        restGetCall<unit,Channel> token <| channelEndpoint chid <| None
    
    /// Update a channels settings. Returns a channel on success, 
    /// and a 400 BAD REQUEST on invalid parameters.
    member this.EditChannel (args : WebModifyChannelParams) (chid:Snowflake) =
        restPatchCall<_, Channel> token
        <| channelEndpoint chid <| Some args

    /// Delete a channel, or close a private message.
    /// Returns a channel object on success.
    member this.DeleteChannel (chid : Snowflake) =
        restDelCall<unit,Channel> token <| channelEndpoint chid <| None

    /// Returns the messages for a channel.
    /// Returns an array of message objects on success.
    member this.GetChannelMessages (args : HistoryParams) (chid : Snowflake) =
        let retrieve =
            restGetCall<WebGetChannelMessagesParams, Message[]> token <| historyEndpoint chid
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
        restGetCall<unit,Message> token <| messageEndpoint chid mgid <| None
    
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
         
        restForm<Message> token Post
        <| channelEndpoint chid
        <| body

    /// Create a reaction for the message. 
    member this.CreateReaction chid mgid emote =
        restPostThunk<unit> token <| userReactionsEndpoint chid mgid emote Me
        
    /// Deletes another user's reaction. 
    member this.DeleteUserReaction chid mgid uid emote =
        restDelThunk<unit> token <| userReactionsEndpoint chid mgid emote (Uid uid)

    /// Get a list of users that reacted with this emoji. 
    /// Returns an array of user objects on success.
    member this.GetReactions chid mgid emote (args : WebGetReactionsParams option) =
        restGetCall<_,Reaction[]> token <| emoteReactionsEndpoint chid mgid emote <| args

    /// Deletes all reactions on a message.
    member this.DeleteAllReactions chid mgid emote =
        restDelThunk<unit> token <| emoteReactionsEndpoint chid mgid emote <| None

    /// Edit a previously sent message.
    /// Returns a message object
    member this.EditMessage chid mgid (args : WebEditMessageParams) =
        restPutCall<_,Message> token <| messageEndpoint chid mgid <| Some args

    /// Delete a reaction the current user has made for the message.
    member this.DeleteOwnReaction chid mgid emote = 
        restDelThunk token <| userReactionsEndpoint chid mgid emote Me <| None
    
    /// Delete a message.
    member this.DeleteMessage chid mgid = 
        restDelCall<unit,Message> token <| messageEndpoint chid mgid

    /// Delete multiple messages in a single request.
    member this.BulkDeleteMessages chid (mgids: Snowflake[]) =
        restPostCall<Snowflake[], unit> token <| bulkDeleteEndpoint chid

    /// Edit the channel permission overwrites for a user or role in a channel.
    member this.EditChannelPermissions
            chid targetId
            (args : WebEditChannelPermissionsParams) =
        restPostCall<_, Perms> token <| channelOverwriteEndpoint chid targetId <| Some args
    
    /// Returns a list of invite objects for the channel.
    member this.GetChannelInvites chid =
        restGetCall<unit,Invite[]> token <| channelInvitesEndpoint chid
    
    /// Create a new invite object for the channel.
    member this.CreateChannelInvite chid (args : WebCreateChannelInviteParams option) =
        restPostCall<_,Invite[]> token <| channelInvitesEndpoint chid <| args
        
    /// Delete a channel permission overwrite for a user or role in a channel.
    member this.DeleteChannelPermission chid targetId =
        restDelThunk<unit> token <| channelOverwriteEndpoint chid targetId <| None
    
    /// Post a typing indicator for the specified channel. 
    member this.TriggerTypingIndicator chid = 
        restPostThunk<unit> token <| channelTypingEndpoint chid
    
    /// Returns all pinned messages in the channel as an array of message objects.
    member this.GetPinnedMessages chid =
        restGetCall<unit,Message[]> token <| channelPinsEndpoint chid <| None
    
    /// Pin a message in a channel.
    member this.AddPinnedChannelMessage chid mgid =
        restPutThunk<unit> token <| channelPinsEndpoint chid <| None
    
    /// Delete a pinned message in a channel. 
    member this.DeletePinnedChannelMessage chid mgid = 
        restDelThunk<unit> token <| channelPinEndpoint chid mgid <| None

    /// Adds a recipient to a Group DM using their access token.
    member this.GroupDMAddRecipient chid mgid (args : WebGroupDMAddRecipientParams option) =
        restPutThunk<_> token <| channelRoutingEndpoint chid mgid <| args
    
    /// Removes a recipient from a Group DM.
    member this.GroupDMRemoveRecipient chid mgid =
        restDelThunk<unit> token <| channelRoutingEndpoint chid mgid <| None

    interface System.IDisposable with
        member this.Dispose () =
            (gw :> IDisposable).Dispose()
