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
open Chessie.ErrorHandling
open Chessie.ErrorHandling.Trial
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
let private historyEndpoint = channelEndpoint >> (+) /> "/messages"
let private messageEndpoint = historyEndpoint >> sprintf "%s/%d"
let private msgReactionsEndpoint mgid chid = messageEndpoint mgid chid + "/reactions"
let private emoteReactionsEndpoint chid mgid (e : Emoji) =
    msgReactionsEndpoint chid mgid
    + (sprintf "/%s"
        <| Option.defaultValue e.name (Option.map string e.id) |> uesc)
        
let private userReactionsEndpoint chid mgid e (u : USpec) =
    emoteReactionsEndpoint chid mgid e + sprintf "/%s" (string u)

let private guildEndpoint (guid : Snowflake option) : string = 
    match guid with
    | Some x -> sprintf "/guilds/%d" x
    | None -> sprintf "/guilds"
let private guildChannelEndpoint = guildEndpoint >> (+) /> "/channels"
let private guildListMemberEndpoint = guildEndpoint >> (+) /> "/members"
let private guildBansEndpoint = guildEndpoint >> (+) /> "/bans"
let private guildPruneEndpoint = guildEndpoint >> (+) /> "/prune"
let private guildVanityURLEndpoint = guildEndpoint >> (+) /> "/vanity-url"
let private guildRegionsEndpoint = guildEndpoint >> (+) /> "/regions"
let private guildInviteEndpoint = guildEndpoint >> (+) /> "/invites"
let private guildEmbedEndpoint = guildEndpoint >> (+) /> "/embed"
let private guildIntegrationsEndpoint = guildEndpoint >> (+) /> "/integrations"
let private guildIntegrationEndpoint = guildIntegrationsEndpoint >> sprintf "%s/%d"
let private guildSyncIntegrationEndpoint intid = 
    guildIntegrationEndpoint intid >> (+) /> "/sync" 
let private guildRolesEndpoint = guildEndpoint >> (+) /> "/roles"
let private guildRoleEndpoint = guildRolesEndpoint >> sprintf "%s/%d"
let private guildBanEndpoint = guildBansEndpoint >> sprintf "%s/%d"
let private guildMemberEndpoint = guildListMemberEndpoint >> sprintf "%s/%d" 
let private guildMemberRolesEndpoint uid = guildMemberEndpoint uid >> sprintf "%s/roles/%d"

let private guildModifyCurrentNickEndpoint guid (u: USpec) = 
    sprintf "%s/%s/nick" (guildListMemberEndpoint guid) (string u)

let private userEndpoint (u : USpec) = sprintf "/users/%s" (string u)
let private userDMEndpoint = userEndpoint >> (+) /> "/channels"
let private userConnectionEndpoint = userEndpoint >> (+) /> "/connections"
let private userGuildEndpoint = userEndpoint >> (+) /> "/guilds" 
let private userGuildIdEndpoint guid = userGuildEndpoint >> (+) /> sprintf "/%d" guid

let private voiceRegionsEndpoint = "/voice/regions"

type Client (token : string) =
    let token = token
    let gw = new Gateway()
    
    let mutable Sessionid = 0

    member val GatewayVersion = 0 with get, set
    member val PrivateChannels = [| |] with get, set
    member val Guilds = [| |] with get,set
    
    member val Events = gw.GatewayEvent
    
    member this.subscribe () = gw.connect token

    /// Get a channel by ID. Returns a channel object.

    member this.GetChannel chid = 
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
            let! payload = retrieve (Some args.Payload) >>- returnOrFail |> Job.toAsync
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
    member this.CreateMessage (chid : Snowflake) (args : MessageCreate.T) =
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
                    yield FormData.FormFile(f.name, (f.name, f.mime, StreamData f.content)) ]
            | None -> []
            |> List.append
                <| List.concat [
                    [NameValue("content", args.content)]
                    unwrap rc
                    args.nonce |> Option.map (fun x -> NameValue("nonce", string x)) |> unwrap
                    args.tts |> Option.map (fun x -> NameValue("tts", string x)) |> unwrap ]
         
        restForm<Message> token Post
        <| historyEndpoint chid
        <| body

    /// Create a reaction for the message. 
    member this.CreateReaction chid mgid emote =
        restPutThunk<unit> token <| userReactionsEndpoint chid mgid emote Me <| None
        
    /// Deletes another user's reaction. 
    member this.DeleteUserReaction chid mgid uid emote =
        restDelThunk<unit> token <| userReactionsEndpoint chid mgid emote (Uid uid)
    
    /// Delete a reaction the current user has made for the message.
    member this.DeleteOwnReaction chid mgid emote = 
        restDelThunk token <| userReactionsEndpoint chid mgid emote Me <| None
    
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
        restPatchCall<_,Message> token <| messageEndpoint chid mgid <| Some args

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
        restPutThunk<unit> token <| channelPinEndpoint chid mgid <| None
    
    /// Delete a pinned message in a channel. 
    member this.DeletePinnedChannelMessage chid mgid = 
        restDelThunk<unit> token <| channelPinEndpoint chid mgid <| None

    /// Adds a recipient to a Group DM using their access token.
    member this.GroupDMAddRecipient chid mgid (args : WebGroupDMAddRecipientParams option) =
        restPutThunk<_> token <| channelRoutingEndpoint chid mgid <| args
    
    /// Removes a recipient from a Group DM.
    member this.GroupDMRemoveRecipient chid mgid =
        restDelThunk<unit> token <| channelRoutingEndpoint chid mgid <| None
    
    ///Create a new guild. Returns a guild object on success. Fires a Guild Create Gateway event.
    member this.CreateGuild (args : CreateGuild) =
        restGetCall<_,Guild> token <| guildEndpoint None <| Some args

    /// Returns the guild object for the given id.
    member this.GetGuild guid =
        restGetCall<unit,Guild> token <| guildEndpoint (Some guid) <| None

    /// Modify a guild's settings. Requires the 'MANAGE_GUILD' permission. 
    /// Returns the updated guild object on success. Fires a Guild Update Gateway event.
    member this.ModifyGuild guid (args : ModifyGuild) =
        restPatchCall<_,Guild> token <| guildEndpoint (Some guid) <| Some args

    /// Delete a guild permanently. User must be owner. 
    /// Returns 204 No Content on success. Fires a Guild Delete Gateway event.
    member this.DeleteGuild guid =
        restDelThunk<unit> token <| guildEndpoint (Some guid) <| None

    /// Returns a list of guild channel objects.
    member this.GetGuildChannels guid = 
        restGetCall<unit,Channel> token <| guildChannelEndpoint (Some guid) <| None

    member this.CreateGuildChannel guid (args : CreateGuildChannel) = 
        restPostCall<_,Channel> token <| guildChannelEndpoint (Some guid) <| Some args

    /// Modify the positions of a set of channel objects for the guild. 
    /// Returns a 204 empty response on success. 
    member this.ModifyGuildChannelPositions guid (args : ModifyPosition) =
        restPatchThunk token <| guildChannelEndpoint (Some guid) <| Some args
    
    /// Returns a guild member object for the specified user.
    member this.GetGuildMember guid (uid : Snowflake) = 
        restGetCall<unit,GuildMember> token <| guildMemberEndpoint (Some guid) uid <| None
    
    /// Returns a list of guild member objects that are members of the guild.
    member this.ListGuildMembers guid (args : GuildMembersList option) = 
        restGetCall<_,GuildMember> token <| guildListMemberEndpoint (Some guid) <| args
    
    /// Adds a user to the guild, provided you have a valid oauth2 access 
    /// token for the user with the guilds.join scope.
    /// Returns a 201 Created with the guild member as the body, 
    /// or 204 No Content if the user is already a member of the guild. 
    member this.AddGuildMember guid uid (args : GuildMemberAdd) =
        restPutThunk token <| guildMemberEndpoint (Some guid) uid <| Some args

    /// Modify attributes of a guild member. Returns a 204 empty response on success. 
    member this.ModifyGuildMember guid uid (args : GuildMemberModify) =
        restPatchThunk token <| guildMemberEndpoint (Some guid) uid <| Some args

    /// Modifies the nickname of the current user in a guild. 
    /// Returns a 200 with the nickname on success. 
    member this.ModifyCurrentUserNick guid (args : CurrentUserModifyNick) =
        restPatchThunk token <| guildModifyCurrentNickEndpoint (Some guid) Me <| Some args

    /// Adds a role to a guild member. Requires the 'MANAGE_ROLES' permission. 
    /// Returns a 204 empty response on success.
    member this.AddGuildMemberRole guid uid (rid : Snowflake) =
        restPutThunk token <| guildMemberRolesEndpoint (Some guid) uid rid <| None

    /// Removes a role from a guild member. Requires the 'MANAGE_ROLES' permission. 
    /// Returns a 204 empty response on success.
    member this.RemoveGuildMemberRole guid uid rid = 
        restDelThunk token <| guildMemberRolesEndpoint (Some guid) uid rid <| None

    /// Remove a member from a guild. Requires 'KICK_MEMBERS' permission.
    /// Returns a 204 empty response on success. 
    member this.RemoveGuildMember guid uid =
        restDelThunk token <| guildMemberEndpoint (Some guid) uid <| None

    /// Returns a list of ban objects for the users banned from this guild. 
    /// Requires the 'BAN_MEMBERS' permission.
    member this.GetGuildBans guid = 
        restGetCall<unit,Ban> token <| guildBansEndpoint (Some guid) <| None 
        
    /// Returns a ban object for the given user or a 404 not found if the ban cannot be found. 
    member this.GetGuildBan guid (uid : Snowflake) = 
        restGetCall<unit,Ban> token <| guildBanEndpoint (Some guid) uid <| None

    /// Create a guild ban, and optionally delete previous messages sent by the banned user.
    /// Returns a 204 empty response on success.
    member this.CreateGuildBan guid uid (args : CreateBan)= 
         restPutThunk token <| guildBanEndpoint (Some guid) uid <| Some args

    /// Remove the ban for a user. Requires the 'BAN_MEMBERS' permissions. 
    /// Returns a 204 empty response on success. 
    member this.RemoveGuildBan guid uid =
        restDelThunk token <| guildBanEndpoint (Some guid) uid <| None
        
    // Returns a list of role objects for the guild.
    member this.GetGuildRoles guid =
        restGetCall<unit,Role[]> token <| guildRolesEndpoint (Some guid) <| None
    
    /// Create a new role for the guild. Requires the 'MANAGE_ROLES' permission. 
    /// Returns the new role object on success. 
    member this.CreateGuildRole guid (args : CreateRole) =
        restPostCall<_,Role> token <| guildRolesEndpoint (Some guid) <| Some args
    
    /// Modify the positions of a set of role objects for the guild.
    /// Returns a list of all of the guild's role objects on success.
    member this.ModifyGuildRolePositions guid (args : ModifyPosition) =
        restPatchCall<_,Role[]> token <| guildRolesEndpoint (Some guid) <| Some args
    
    /// Modify a guild role. Requires the 'MANAGE_ROLES' permission. 
    /// Returns the updated role on success. 
    member this.ModifyGuildRole guid (rid : Snowflake) (args : ModifyRole) =
        restPatchCall<_,Role> token <| guildRoleEndpoint (Some guid) rid <| Some args

    /// Delete a guild role. Requires the 'MANAGE_ROLES' permission. 
    /// Returns a 204 empty response on success.
    member this.DeleteGuildRole guid rid = 
        restDelThunk token <| guildRoleEndpoint (Some guid) rid <| None

    /// Returns an object with one 'pruned' key indicating the number of 
    /// members that would be removed in a prune operation. 
    member this.GetPruneCount guid (args : PruneQueryParams) = 
        restGetCall<_,Prune> token <| guildPruneEndpoint (Some guid) <| Some args

    /// Begin a prune operation.
    /// Returns an object with one 'pruned' key indicating the number 
    /// of members that were removed in the prune operation. 
    member this.BeginGuildPrune guid (args : PruneQueryParams) =
        restPostCall<_,Prune> token <| guildPruneEndpoint (Some guid) <| Some args

    /// Returns a list of voice region objects for the guild.
    member this.GetGuildVoiceRegions guid = 
        restGetCall<unit,VoiceRegion> token <| guildRegionsEndpoint (Some guid) <| None

    /// Returns a list of invite objects (with invite metadata) for the guild. 
    member this.GetGuildInvites guid = 
        restGetCall<unit,Invite> token <| guildInviteEndpoint (Some guid) <| None

    /// Returns a list of integration objects for the guild. 
    member this.GetGuildIntegrations guid = 
        restGetCall<unit,Invite> token <| guildIntegrationsEndpoint (Some guid) <| None
    
    /// Attach an integration object from the current user to the guild. 
    /// Returns a 204 empty response on success. 
    member this.CreateGuildIntegration guid (args : CreateIntegration) = 
        restPostThunk token <| guildIntegrationsEndpoint (Some guid) <| Some args

    /// Modify the behavior and settings of a integration object for the guild. 
    /// Returns a 204 empty response on success.
    member this.ModifyGuildIntegration guid (intid : Snowflake) (args : ModifyIntegration) =
        restPatchThunk token <| guildIntegrationEndpoint (Some guid) intid <| Some args

    /// Delete the attached integration object for the guild. 
    /// Returns a 204 empty response on success. 
    member this.DeleteGuildIntegration guid intid =
        restDelThunk token <| guildIntegrationEndpoint (Some guid) intid <| None

    /// Sync an integration. 
    /// Returns a 204 empty response on success.
    member this.SyncGuildIntegration guid intid =
        restPostThunk token <| guildSyncIntegrationEndpoint (Some guid) intid <| None

    /// Returns the guild embed object.
    member this.GetGuildEmbed guid =
        restGetCall<unit,GuildEmbed> token <| guildEmbedEndpoint (Some guid) <| None

    /// Modify a guild embed object for the guild. All attributes may be passed in with JSON and modified. 
    /// Returns the updated guild embed object.
    member this.ModifyGuildEmbed guid (args : GuildEmbed) =
        restPatchCall<_,GuildEmbed> token <| guildEmbedEndpoint (Some guid) <| Some args

    /// Returns a partial invite object for guilds with that feature enabled.
    member this.GetGuildVanityURL guid =
        restGetCall<unit,Invite> token <| guildVanityURLEndpoint (Some guid) <| None

    /// Returns the user object of the requester's account.
    member this.GetCurrentUser =
        restGetCall<unit,User> token <| userEndpoint Me <| None
    
    /// Returns a user object for a given user ID.
    member this.GetUser uid =
        restGetCall<unit,User> token <| userEndpoint (Uid uid) <| None

    /// Modify the requester's user account settings. Returns a user object on success.
    member this.ModifyCurrentUser (args : WebModifyCurrentUserParams) =
        restPatchCall<_,User> <| token <| userEndpoint Me <| Some args
    
    /// Returns a list of partial guild objects the current user is a member of. 
    /// Requires the guilds OAuth2 scope.
    member this.GetCurrentUserGuilds (args : WebGetCurrentUserGuildParams option) =
        restGetCall<_,Guild> <| token <| userGuildEndpoint Me <| args
    
    /// Leaves the guild with the given ID.
    member this.LeaveGuild (guid : Snowflake) =
        restDelThunk<unit> <| token <| userGuildIdEndpoint guid Me <| None
    
    /// Returns a list of DM channel objects. For bots, this is no longer a 
    /// supported method of getting recent DMs, and will return an empty array.
    member this.GetUserDMs =
        restGetCall<unit,Channel[]> <| token <| userDMEndpoint Me <| None

    /// Create a new DM channel with a user. Returns a DM channel object.
    member this.CreateDM (args : WebCreateDMParams) =
        restPostCall<_,Channel> <| token <| userDMEndpoint Me <| Some args
    
    /// Create a new group DM channel with multiple users. Returns a DM channel object.
    /// Note: This endpoint is limited to 10 active group DMs.
    member this.CreateGroupDM (args : WebCreateGroupDMParams) =
        restPostCall<_,Channel> <| token <| userDMEndpoint Me <| Some args
    
    /// Returns a list of connection objects. 
    /// Requires the connections OAuth2 scope.
    member this.GetUserConnections =
        restGetCall<unit,Connection[]> <| token <| userConnectionEndpoint Me <| None
    
    /// Returns an array of voice region objects that can be used when creating servers.
    member this.ListVoiceRegions = 
        restGetCall<unit,VoiceRegion[]> <| token <| voiceRegionsEndpoint <| None

    interface System.IDisposable with
        member this.Dispose () =
            (gw :> IDisposable).Dispose()
