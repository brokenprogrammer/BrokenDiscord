module BrokenDiscord.Types

open System

open Newtonsoft.Json
open FSharp.Data
open HttpFs.Client

type ISerializable =
    abstract member Serialize : unit -> string

type IMentionable =
    abstract member Mention : string

type Snowflake = uint64
module Snowflake =
    let epoch = new DateTime(2015, 1, 1)
    let epochLocal = new DateTimeOffset(epoch, TimeZoneInfo.Local.BaseUtcOffset)
    let timestamp (s : Snowflake) = 
        s >>> 22 |> float
        |> TimeSpan.FromMilliseconds
        |> (+) epoch
        
    let ofTime t =
        (t - epoch).TotalMilliseconds
        |> uint64 <<< 22
    
    let withTime t s = ofTime t ||| s
    
[<Flags>]
type Perms =
    | CreateInstantInvite = 0x00000001
    | KickMembers         = 0x00000002
    | BanMembers          = 0x00000004
    | Administrator       = 0x00000008
    | ManageChannels      = 0x00000010
    | ManageGuild         = 0x00000020
    | AddReactions        = 0x00000040
    | ViewAuditLog        = 0x00000080
    | ViewChannel         = 0x00000400
    | SendMessages        = 0x00000800
    | SendTtsMessages     = 0x00001000
    | ManageMessages      = 0x00002000
    | EmbedLinks          = 0x00004000
    | AttachFiles         = 0x00008000
    | ReadMessageHistory  = 0x00010000
    | MentionEveryone     = 0x00020000
    | UseExternalEmojis   = 0x00040000
    | Connect             = 0x00100000
    | Speak               = 0x00200000
    | MuteMembers         = 0x00400000
    | DeafenMembers       = 0x00800000
    | MoveMembers         = 0x01000000
    | UseVAD              = 0x02000000
    | PrioritySpeaker     = 0x00000100
    | ChangeNickname      = 0x04000000
    | ManageNicknames     = 0x08000000
    | ManageRoles         = 0x10000000
    | ManageWebhooks      = 0x20000000
    | ManageEmojis        = 0x40000000

let serverWideMFARequiresMFA (p : Perms) =
    function
    | Perms.KickMembers   | Perms.BanMembers
    | Perms.Administrator | Perms.ManageChannels
    | Perms.ManageGuild   | Perms.ManageMessages
    | Perms.ManageRoles   | Perms.ManageWebhooks
    | Perms.ManageEmojis -> true
    | _ -> false
    
type Overwrite = {
        id      : Snowflake
        [<JsonProperty "type">]
        kind    : string
        allow   : int
        deny    : int
    }

type User = {
        id              : Snowflake
        username        : string
        discriminator   : string
        avatar          : string option
        bot             : bool option
        [<JsonProperty "mfa_enabled">]
        mfaEnabled      : bool option
        locale          : string option
        verified        : bool option
        email           : string option
    }
    with
    member x.NickMention = sprintf "<@!%d>" x.id
    interface IMentionable with
        member x.Mention = sprintf "<@%d>" x.id
            
type Prune = {
        pruned : int
    }

type PruneQueryParams = {
        days : int
    }

type Ban = {
        reson : string option
        user  : User
    }

type CreateBan = {
        [<JsonProperty "delete-message-days">]
        deleteMessageDays : int
        reason            : string
    }

type IntegrationAccount = {
        id      : string
        name    : string
    }

type Integration = {
        id                  : Snowflake
        name                : string
        [<JsonProperty "type">]
        kind                : string
        enabled             : bool
        syncing             : bool
        [<JsonProperty "role_id">]
        roleId              : Snowflake
        [<JsonProperty "expire_behavior">]
        expireBehavior      : int
        [<JsonProperty "expire_grace_period">]
        expireGracePeriod   : int
        user                : User
        account             : IntegrationAccount
        [<JsonProperty "synced_at">]
        syncedAt            : DateTime
    }

type CreateIntegration = {
        [<JsonProperty "type">]
        kind : string
        id   : Snowflake
    }

type ModifyIntegration = {
        [<JsonProperty "expire_behavior">]
        expireBehavior     : int
        [<JsonProperty "expire_grace_period">]
        expireGracePeriod : int
        [<JsonProperty "enable_emoticons">]
        enableEmoticons    : bool
    }

type Connection = {
        id           : string
        name         : string
        [<JsonProperty "type">]
        kind         : string
        revoked      : bool
        integrations : Integration[]
    }

type Role = {
        id          : Snowflake
        name        : string
        color       : int
        hoist       : bool
        position    : int
        permissions : int
        managed     : bool
        mentionable : bool
    }
    with
    interface IMentionable with
        member x.Mention = sprintf "<@&%d>" x.id

type CreateRole = {
        name        : string option
        permission  : int option
        color       : int option
        hoist       : bool option
        mentionable : bool option
    }

type ModifyRole = {
        name        : string
        permissions : int
        color       : int
        hoist       : bool
        mentionable : bool
    }

type ChannelKind = 
    | GuildText     = 0
    | DM            = 1
    | GuildVoice    = 2
    | GroupDM       = 3
    | GuildCategory = 4
    
type Channel = {
        id                      : Snowflake
        [<JsonProperty "type">]
        kind                    : ChannelKind
        [<JsonProperty "guild_id">]
        guildId                 : Snowflake option
        position                : int option
        [<JsonProperty "permission_overwrites">]
        permissionOverwrites    : Overwrite[]
        name                    : string option
        topic                   : string option
        nsfw                    : bool option
        [<JsonProperty "last_message_id">]
        lastMessageId           : Snowflake option
        bitrate                 : int option
        [<JsonProperty "user_limit">]
        userLimit               : int option
        recipients              : User[]
        icon                    : string option
        [<JsonProperty "owner_id">]
        ownerId                 : Snowflake option
        [<JsonProperty "application_id">]
        applicationId           : Snowflake option
        [<JsonProperty "parent_id">]
        parentId                : Snowflake option
        [<JsonProperty "last_pin_timestamp">]
        lastPinTimestamp        : DateTime option
    } with
    interface IMentionable with
        member x.Mention = sprintf "<#%d>" x.id
    
type EmbedThumbnail = {
        url         : string option
        [<JsonProperty "proxy_url">]
        proxyURL    : string option
        height      : int option
        width       : int option
    } with
    static member Default = {url = None; proxyURL = None; height = None; width = None}

type EmbedVideo = {
        url     : string option
        height  : int option
        width   : int option
    } with
    static member Default = {url = None; height = None; width = None}

type EmbedImage = {
        url         : string option
        [<JsonProperty "proxy_url">]
        proxyURL    : string option
        height      : int option
        width       : int option
    } with
    static member Default = {url = None; proxyURL = None; height = None; width = None}

type EmbedProvider = {
        name    : string option
        url     : string option
    } with
    static member Default = {name = None; url = None}

type EmbedAuthor = {
        name            : string option
        url             : string option
        [<JsonProperty "icon_url">]
        iconURL         : string option
        [<JsonProperty "proxy_icon_url">]
        proxyIconURL    : string option
    } with
    static member Default = {name = None; url = None; iconURL = None; proxyIconURL = None}

type EmbedFooter = {
        text            : string
        [<JsonProperty "icon_url">]
        iconURL         : string option
        [<JsonProperty "proxy_icon_url">]
        proxyIconURL    : string option
    } with
    static member Default = {text = ""; iconURL = None; proxyIconURL = None}

type EmbedField = {
        name    : string
        value   : string
        inlinep : bool option
    }
    
type Embed = {
        title       : string option
        [<JsonProperty "type">]
        kind        : string option
        description : string option
        url         : string option
        timestamp   : DateTime option
        color       : int option
        footer      : EmbedFooter option
        image       : EmbedImage option
        thumbnail   : EmbedThumbnail option
        video       : EmbedVideo option
        provider    : EmbedProvider option
        author      : EmbedAuthor option
        fields      : EmbedField[] option
    } with
    static member Default = {title=None; kind = None; description = None; url = None; timestamp = None; 
                             color = None; footer = None; image = None; thumbnail = None; video = None; 
                             provider = None; author = None; fields = None}
    static member Simple title description = { Embed.Default with title = Some title; description = Some description}

type Attachment = {
        id          : Snowflake
        filename    : string
        size        : int
        url         : string
        [<JsonProperty "proxy_url">]
        proxyURL    : string
        height      : int option
        width       : int option
    }

type Emoji = {
        id              : Snowflake option
        name            : string
        roles           : Role[] option
        user            : User option
        [<JsonProperty "require_colons">]
        requireColons   : bool option
        managed         : bool option
        animated        : bool option
    } with
    static member Empty = 
        {id = None; name = ""; roles = None; user = None; requireColons = None; managed = None; animated = None;}
    static member CreateOfUnicode unicode = { Emoji.Empty with name=unicode}
    static member CreateOfNameId name id = {Emoji.Empty with id = id; name = name}
    static member CreateAnimated name id animated = {Emoji.Empty with id = id; name = name; animated = animated}
    
    interface IMentionable with
        member x.Mention =
            match x.id with
            | Some id ->
                sprintf "<%s:%s:%d>"
                <| if Option.defaultValue false x.animated then "a" else ""
                <| x.name <| id
            | _ -> x.name

type Reaction = {
        count   : int
        me      : bool
        emoji   : Emoji
    }

type MessageActivityKind =
    | Join          = 1
    | Spectate      = 2
    | Listen        = 3
    | JoinRequest   = 5

type MessageActivity = {
        [<JsonProperty "type">]
        kind : MessageActivityKind
        partyId: string option
    }

type MessageApplication = {
        id          : Snowflake
        [<JsonProperty "cover_image">]
        coverImage  : string
        description : string
        icon        : string
        name        : string
    }

type MessageKind = 
    | Default               = 0
    | RecipientAdd          = 1
    | RecipientRemove       = 2
    | Call                  = 3
    | ChannelNameChange     = 4
    | ChannelIconChange     = 5
    | ChannelPinnedMessage  = 6
    | GuildMemberJoin       = 7

type Message = {
        id              : Snowflake
        [<JsonProperty "channel_id">]
        channelId       : Snowflake
        [<JsonProperty "guild_id">]
        guildId         : Snowflake option
        author          : User
        content         : string
        timestamp       : DateTime
        [<JsonProperty "edit_timestamp">]
        editedTimestamp : DateTime option
        tts             : bool
        [<JsonProperty "mention_everyone">]
        mentionEveryone : bool
        mentions        : User[]
        [<JsonProperty "mention_roles">]
        mentionRoles    : Role[]
        attachments     : Attachment[]
        embeds          : Embed[]
        reactions       : Reaction[] option
        nonce           : Snowflake option
        pinned          : bool
        [<JsonProperty "webhook_id">]
        webhookId       : Snowflake option
        kind            : MessageKind
        activity        : MessageActivity option
        application     : MessageApplication option
    } with
    static member Create author content =
        {
            id              = Snowflake.ofTime DateTime.Now
            channelId       = 0UL
            guildId         = None
            author          = author
            content         = content
            timestamp       = DateTime.Now
            editedTimestamp = None
            tts             = false
            mentionEveryone = false
            mentions        = [| |]
            mentionRoles    = [| |]
            attachments     = [| |]
            embeds          = [| |]
            reactions       = None
            nonce           = None
            pinned          = false
            webhookId       = None
            kind            = MessageKind.Default
            activity        = None
            application     = None
        }
        
type ActivityType = 
    | Game      = 0
    | Streaming = 1
    | Listening = 2

type ActivityTimestamps = {
        start       : int option
        [<JsonProperty "end">]
        finish    : int option
    }

type ActivityParty = {
        id      : string option
        size    : int[] option
    }

type ActivityAssets = {
        [<JsonProperty "large_image">]
        largeImage  : string option
        [<JsonProperty "large_text">]
        largeText   : string option
        [<JsonProperty "small_image">]
        smallImage  : string option
        [<JsonProperty "small_text">]
        smallText   : string option
    }

type ActivitySecrets = {
        join        : string option
        spectate    : string option
        [<JsonProperty "match">]
        match_      : string option
    }

[<Flags>]
type ActivityFlags = 
    | INSTANCE      = 0b0000_0001
    | JOIN          = 0b0000_0010
    | SPECTATE      = 0b0000_0100
    | JOIN_REQUEST  = 0b0000_1000
    | SYNC          = 0b0001_0000
    | PLAY          = 0b0010_0000

type Activity = {
        name            : string
        [<JsonProperty "type">]
        kind            : ActivityType
        url             : string option
        timestamps      : ActivityTimestamps option
        [<JsonProperty "application_id">]
        applicationId   : Snowflake option
        details         : string option
        state           : string option
        party           : ActivityParty option
        assets          : ActivityAssets option
        secrets         : ActivitySecrets option
        instance        : bool option
        flags           : int
    }

type VoiceState = {
        [<JsonProperty "guild_id">]
        guildId     : Snowflake option
        [<JsonProperty "channel_id">]
        channelId   : Snowflake option
        [<JsonProperty "user_id">]
        userId      : Snowflake
        [<JsonProperty "session_id">]
        sessionId   : string
        deaf        : bool
        mute        : bool
        [<JsonProperty "self_deaf">]
        selfDeaf    : bool
        [<JsonProperty "self_mute">]
        selfMute    : bool
        suppress    : bool
    }

type VoiceRegion = {
        id          : string
        name        : string
        vip         : bool
        optimal     : bool
        deprecated  : bool
        custom      : bool
}

type PresenceUpdate = {
        user    : User
        roles   : Snowflake[]
        game    : Activity option
        [<JsonProperty "guild_id">]
        guildId : Snowflake
        status  : string
    }

type GuildMember = {
        user        : User
        nick        : string option
        roles       : Snowflake[]
        [<JsonProperty "joined_at">]
        joinedAt    : DateTime
        deaf        : bool
        mute        : bool
    }

type GuildEmbed = {
        enabled     : bool
        [<JsonProperty "channel_id">]
        channelId  : Snowflake option
    }

type Guild = {
        id                              : Snowflake
        name                            : string
        icon                            : string option
        splash                          : string option
        owner                           : bool option
        [<JsonProperty "owner_id">]
        ownerId                         : Snowflake
        permissions                     : int option
        region                          : string
        [<JsonProperty "afk_channel_id">]
        afkChannelId                    : Snowflake option
        [<JsonProperty "afk_timeout">]
        afkTimeout                      : int
        [<JsonProperty "embed_enabled">]
        embedEnabled                    : bool option
        [<JsonProperty "embed_channel_id">]
        embedChannelId                  : Snowflake option
        [<JsonProperty "verification_level">]
        verificationLevel               : int
        [<JsonProperty "default_message_notifications">]
        defaultMessageNotifications     : int
        [<JsonProperty "explicit_content_filter">]
        explicitContentFilter           : int
        roles                           : Role[]
        emojis                          : Emoji[]
        features                        : string[]
        [<JsonProperty "mfa_level">]
        mfaLevel                        : int
        [<JsonProperty "application_id">]
        applicationId                   : Snowflake option
        [<JsonProperty "widget_enabled">]
        widgetEnabled                   : bool option
        [<JsonProperty "widget_channel_id">]
        widgetChannelId                 : Snowflake option
        [<JsonProperty "system_channel_id">]
        systemChannelId                 : Snowflake option
        // Below are only sent with GUILD_CREATE Event
        [<JsonProperty "joined_at">]
        joinedAt                        : DateTime option
        large                           : bool option
        unavailable                     : bool option
        [<JsonProperty "member_count">]
        memberCount                     : int option
        [<JsonProperty "voice_states">]
        voiceStates                     : VoiceState[] option
        members                         : GuildMember[] option
        channels                        : Channel[] option
        presences                       : PresenceUpdate[] option
    }

type CreateGuild = {
        name                            : string
        region                          : string
        icon                            : string
        [<JsonProperty "verification_level">]
        verificationLevel               : int
        [<JsonProperty "default_message_notifications">]
        defaultMessageNotifications     : int
        [<JsonProperty "explicit_content_filter">]
        explicitContentFilter           : int
        roles                           : Role[]
        channels                        : Channel[]
    }

type ModifyGuild = {
        name                            : string
        region                          : string
        [<JsonProperty "verification_level">]
        verification_level              : int
        [<JsonProperty "default_message_notifications">]
        defaultMessageNotifications     : int
        [<JsonProperty "explicit_content_filter">]
        explicitContentFilter           : int
        [<JsonProperty "afk_channel_id">]
        afkChannelId                    : Snowflake
        [<JsonProperty "afk_timeout">]
        afkTimeout                      : int
        icon                            : string
        [<JsonProperty "owner_id">]
        ownerId                         : Snowflake
        splash                          : string
        [<JsonProperty "system_channel_id">]
        systemChannelId                 : Snowflake
    }

type CreateGuildChannel = {
        name                    : string
        [<JsonProperty "type">]
        kind                    : int
        topic                   : string
        bitrate                 : int
        [<JsonProperty "user_limit">]
        userLimit               : int
        [<JsonProperty "permission_overwrites">]
        permissionOverwrites    : Overwrite[]
        [<JsonProperty "parent_id">]
        parentId                : Snowflake
        nsfw                    : bool
    }

type ModifyPosition = {
        id          : Snowflake
        position    : int
    }


type GuildMembersList = {
        limit : int
        after : Snowflake
    }

type GuildMemberAdd = {
        [<JsonProperty "access_token">]
        accessToken  : string
        nick         : string option
        roles        : Snowflake[] option
        mute         : bool option
        deaf         : bool option
    }

type GuildMemberModify = {
        nick        : string option
        roles       : Snowflake[] option
        mute        : bool option
        deaf        : bool option
        [<JsonProperty "channel_id">]
        channelId   : Snowflake option
    }

type CurrentUserModifyNick = { nick : string }

type InviteMetadata = {
        inviter     : User
        uses        : int
        [<JsonProperty "max_uses">]
        maxUses     : int
        [<JsonProperty "max_age">]
        maxAge      : int
        temporary   : bool
        [<JsonProperty "created_at">]
        createdAt   : DateTime
        revoked     : bool
    }
    
type Invite = {
        code                        : string
        guild                       : Guild option
        channel                     : Channel
        [<JsonProperty "approximate_presence_count">]
        approximatePresenceCount    : int option
        [<JsonProperty "approximate_member_count">]
        approximateMemberCount      : int option
        [<JsonProperty "meta_data">]
        metaData                    : InviteMetadata option
    }

type WebModifyChannelParams = {
        name                    : string
        position                : int
        topic                   : string
        nsfw                    : bool
        bitrate                 : int
        [<JsonProperty "user_limit">]
        userLimit               : int
        [<JsonProperty "permission_overwrites">]
        permissionOverwrites    : Overwrite[]
        [<JsonProperty "parent_id">]
        parentId                : Snowflake
    }

type WebGetChannelMessagesParams = {
        around  : Snowflake option
        before  : Snowflake option
        after   : Snowflake option
        limit   : int option
    }
    with static member None = { around=None; before=None; after=None; limit=None }

// https://discordapp.com/developers/docs/resources/channel#create-message
type WebCreateMessageParams = {
        content      : string
        nonce        : Snowflake option
        tts          : bool option
        file         : System.IO.Stream option
        embed        : Embed option
        payload_json : string option
    }

module MessageCreate = 
    type File = {
            mime : ContentType; name: string
            content: FileData }
      
    type T = {
            content     : string
            nonce       : Snowflake option
            tts         : bool option
            embed       : Embed option
            files       : File[] option
        } with
        static member Default = { content=""; nonce=None; tts=None; embed = None; files = None}
        static member New content = { T.Default with content=content }
        member this.WithFile (file:File) = 
            let f = Option.defaultValue ([| |]) this.files
            { this with files = Some (Array.append f [| file |]) }

        member this.WithEmbed(embed) = { this with embed = Some embed}
            
        member this.WithFile(name:string, body) =
            let ext = name.[name.LastIndexOf('.')..]
            let contentType = MimeTypes.tryFind ext
            let contentType =
                match contentType with
                | Some x -> x
                | _ -> failwithf "No content-type found corresponding to extension %s" ext
                |> ContentType.parse
                |> Option.get
            let file = 
                {   mime=contentType; name=name; content=body }
            this.WithFile(file)

        member this.HasFile() =
            match this.files with
            | Some files -> files.Length <> 0
            | None -> false

type WebGetReactionsParams = {
        before  : Snowflake option
        after   : Snowflake option
        limit   : int option
    } with
    static member Default = { before=None; after=None; limit=None }

type WebEditMessageParams = {
        content : string
        embed   : Embed option
    } with
    static member OfContent s = { content=s; embed=None }

        
type WebEditChannelPermissionsParams = {
        allow       : int
        deny        : int
        [<JsonProperty "type">]
        kind        : string
    }

type WebCreateChannelInviteParams = {
        [<JsonProperty "max_age">]
        maxAge      : int option
        [<JsonProperty "max_uses">]
        maxUses     : int option
        temporary   : bool option
        unique      : bool option
    } with
    static member Default =
        { maxAge=None; maxUses=None
          temporary=Some true; unique=Some true }

type WebGroupDMAddRecipientParams = {
        [<JsonProperty "access_token">]
        accessToken     : string
        nick            : string
    }

type WebModifyCurrentUserParams = {
        username : string
        avatar   : string   // TODO: Data URI Scheme for images.
    }

type WebGetCurrentUserGuildParams = {
        before : Snowflake
        after  : Snowflake
        limit  : int
    }

type WebCreateDMParams = {
        [<JsonProperty "recipient_id">]
        recipientId : Snowflake
    }

type WebCreateGroupDMParams = {
        [<JsonProperty "access_token">]
        accessTokens  : string[]
        nicks         : Map<Snowflake, string> // TODO: Is this serialized correctly? aka right type of dict expected.
    }

module HistoryParams =
    type BeforeSpec = Latest | Snowflake of Snowflake
    type Direction =
        | Before of BeforeSpec
        | After of Snowflake
        | Around of Snowflake
        
open HistoryParams 

type HistoryParams =
    { limit : int; direction : Direction }
    with
    static member Default =
        { limit=15; direction=Before Latest }
    
    member x.Payload =
        if x.limit > 100 then failwith "limit must be in the range (0, 100]."
        {   WebGetChannelMessagesParams.None
            with
                before =  
                    match x.direction with
                    | Before (Snowflake x) -> Some x
                    | _ -> None
                after = match x.direction with After x -> Some x | _ -> None
                around = match x.direction with Around x -> Some x | _ -> None }
                
    /// Creates a specification which retrieves the adjacent batch of messages
    /// with the current limit=n in the given direction, given
    /// the last snowflake retrieved.
    member this.ScrollBy n from =
        {   this
            with
                direction = 
                    match this.direction with 
                    | Before _ -> Before (Snowflake from)
                    | After _ -> After from
                    | Around _ -> failwith "Direction.Around is not scrollable."
                limit = this.limit - n }

type Webhook = {
        id          : Snowflake
        [<JsonProperty "guild_id">]
        guildId    : Snowflake option
        [<JsonProperty "channel_id">]
        channelId  : Snowflake option
        user        : User option
        name        : string option
        avatar      : string option
        token       : string option
    }

type CreateWebhook = {
        name    : string
        avatar  : string
    }

type ModifyWebhook = {
        name        : string option
        avatar      : string option
        [<JsonProperty "channel_id">]
        channelId  : Snowflake option
    }

type ExecuteWebhook = {
        content     : string
        username    : string
        [<JsonProperty "avatar_url">]
        avatarUrl  : string
        tts         : bool
        file        : File
        embeds      : Embed[]
    }

type ApiError =
    { code : uint32; message : string option }
    
exception ApiException of ApiError

type USpec = Me | Uid of Snowflake
    with
    override x.ToString () =
        match x with Me -> "@me" | Uid x -> (string x)
