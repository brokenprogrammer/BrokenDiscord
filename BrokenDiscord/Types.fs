module BrokenDiscord.Types

open System

open Newtonsoft.Json.Linq
open Newtonsoft.Json
open FSharp.Data
open HttpFs.Client

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
    
type PermsTarget =
    User | Role
    with
    override x.ToString () =
        match x with User -> "user" | Role -> "role"
    member __.OfString =
        function "user" -> User | "role" -> Role
    

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
        kind    : PermsTarget
        allow   : int
        deny    : int
    }

type User = {
        id              : Snowflake
        username        : string
        discriminator   : string
        avatar          : string option
        bot             : bool option
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
        delete_message_days : int
        reason              : string
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
        role_id             : Snowflake
        expire_behavior     : int
        expire_grace_period : int
        user                : User
        account             : IntegrationAccount
        synced_at           : DateTime
    }

type CreateIntegration = {
        [<JsonProperty "type">]
        kind : string
        id   : Snowflake
    }

type ModifyIntegration = {
        expire_behavior     : int
        expire_grace_period : int
        enable_emoticons    : bool
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
        guildId                 : Snowflake option
        position                : int option
        permissionOverwrites    : Overwrite[]
        name                    : string option
        topic                   : string option
        nsfw                    : bool option
        lastMessageId           : Snowflake option
        bitrate                 : int option
        userLimit               : int option
        recipients              : User[]
        icon                    : string option
        ownerId                 : Snowflake option
        applicationId           : Snowflake option
        parentId                : Snowflake option
        lastPinTimestamp        : DateTime option
    } with
    interface IMentionable with
        member x.Mention = sprintf "<#%d>" x.id
    

type EmbedThumbnail = {
        url         : string option
        proxyURL    : string option
        height      : int option
        width       : int option
    }

type EmbedVideo = {
        url     : string option
        height  : int option
        width   : int option
    }

type EmbedImage = {
        url         : string option
        proxyURL    : string option
        height      : int option
        width       : int option
    }

type EmbedProvider = {
        name    : string option
        url     : string option
    }

type EmbedAuthor = {
        name            : string option
        url             : string option
        iconURL         : string option
        proxyIconURL    : string option
    }

type EmbedFooter = {
        text            : string
        iconURL         : string option
        proxyIconURL    : string option
    }

type EmbedField = {
        name    : string
        value   : string
        inlinep : bool option
    }

    
type Embed = {
        title       : string option
        [<JsonProperty "type">]
        kind        : ChannelKind
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
    }

type Attachment = {
        id          : Snowflake
        filename    : string
        size        : int
        url         : string
        proxyURL    : string
        height      : int option
        width       : int option
    }

type Emoji = {
        id              : Snowflake option
        name            : string
        roles           : Role[] option
        user            : User option
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
        [<JsonProperty "kind">]
        kind : MessageActivityKind
        partyId: string option
    }

type MessageApplication = {
        id          : Snowflake
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
        author          : User
        content         : string
        timestamp       : DateTime
        editedTimestamp : DateTime option
        tts             : bool
        mentionEveryone : bool
        mentions        : User[]
        mentionRoles    : Role[]
        attachments     : Attachment[]
        embeds          : Embed[]
        reactions       : Reaction[] option
        nonce           : Snowflake option
        pinned          : bool
        webhookId       : Snowflake option
        kind            : MessageKind
        activity        : MessageActivity option
        application     : MessageApplication option
    } with
    static member Create author content =
        {
            id              = Snowflake.ofTime DateTime.Now
            channelId       = 0UL
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

type ActivityTimestamps = {Start : int option; End : int option}

type ActivityParty = {Id : string option; Size : int[] option}

type ActivityAssets = {
        largeImage  : string option
        largeText   : string option
        smallImage  : string option
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
        guildId     : Snowflake option
        channelId   : Snowflake option
        userId      : Snowflake
        sessionId   : string
        deaf        : bool
        mute        : bool
        selfDeaf    : bool
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
        guildId : Snowflake
        status  : string
    }

type GuildMember = {
        user        : User
        nick        : string option
        roles       : Snowflake[]
        joinedAt    : DateTime
        deaf        : bool
        mute        : bool
    }

type GuildEmbed = {
        enabled     : bool
        channel_id  : Snowflake option
    }

type Guild = {
        id                              : Snowflake
        name                            : string
        icon                            : string option
        splash                          : string option
        owner                           : bool option
        ownerId                         : Snowflake
        permissions                     : int option
        region                          : string
        afkChannelId                    : Snowflake option
        afkTimeout                      : int
        embedEnabled                    : bool option
        embedChannelId                  : Snowflake option
        verificationLevel               : int
        defaultMessageNotifications     : int
        explicitContentFilter           : int
        roles                           : Role[]
        emojis                          : Emoji[]
        features                        : string[]
        mfaLevel                        : int
        applicationId                   : Snowflake option
        widgetEnabled                   : bool option
        widgetChannelId                 : Snowflake option
        systemChannelId                 : Snowflake option
        // Below are only sent with GUILD_CREATE Event
        joinedAt                        : DateTime option
        large                           : bool option
        unavailable                     : bool option
        memberCount                     : int option
        voiceStates                     : VoiceState[] option
        members                         : GuildMember[] option
        channels                        : Channel[] option
        presences                       : PresenceUpdate[] option
    }

type CreateGuild = {
        name                            : string
        region                          : string
        icon                            : string
        verification_level              : int
        default_message_notifications   : int
        explicit_content_filter         : int
        roles                           : Role[]
        channels                        : Channel[]
    }

type ModifyGuild = {
        name                            : string
        region                          : string
        verification_level              : int
        default_message_notifications   : int
        explicit_content_filter         : int
        afk_channel_id                  : Snowflake
        afk_timeout                     : int
        icon                            : string
        owner_id                        : Snowflake
        splash                          : string
        system_channel_id               : Snowflake
    }

type CreateGuildChannel = {
        name                    : string
        [<JsonProperty "type">]
        kind                    : int
        topic                   : string
        bitrate                 : int
        user_limit              : int
        permission_overwrites   : Overwrite[]
        parent_id               : Snowflake
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
        access_token : string
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
        channel_id  : Snowflake option
    }

type CurrentUserModifyNick = { nick : string }

type InviteMetadata = {
        inviter     : User
        uses        : int
        max_uses    : int
        max_age     : int
        temporary   : bool
        created_at  : DateTime
        revoked     : bool
    }
    
type Invite = {
        code                        : string
        guild                       : Guild option
        channel                     : Channel
        approximate_presence_count  : int option
        approximate_member_count    : int option
        meta_data                   : InviteMetadata option
    }

type WebModifyChannelParams = {
        name                    : string
        position                : int
        topic                   : string
        nsfw                    : bool
        bitrate                 : int
        user_limit              : int
        permission_overwrites   : Overwrite[]
        parent_id               : Snowflake
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
            content: System.IO.Stream }
    type RichContent =
        { embed : Embed option; files : File[] }
        with static member Default = { embed=None; files=[| |] }
        
    type T = {
            content     : string
            nonce       : Snowflake option
            tts         : bool option
            richContent : RichContent option
        } with
        static member Default = { content=""; nonce=None; tts=None; richContent=None }
        static member New content = { T.Default with content=content }
        member this.WithFile (file:File) =
            let rich = Option.defaultValue RichContent.Default this.richContent
            { this with richContent = 
                        Some { rich with files=(Array.append rich.files [| file |]) } }
                        
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
            
        member this.WithEmbed(embed) =
            let rich = Option.defaultValue RichContent.Default this.richContent
            { this with richContent =
                        Some { rich with embed=Some embed } }
                
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
        editType    : PermsTarget
    }

type WebCreateChannelInviteParams = {
        max_age     : int option
        max_uses    : int option
        temporary   : bool option
        unique      : bool option
    } with
    static member Default =
        { max_age=None; max_uses=None
          temporary=Some true; unique=Some true }

type WebGroupDMAddRecipientParams = {
        access_token    : string
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
        recipient_id : Snowflake
    }

type WebCreateGroupDMParams = {
        access_tokens : string[]
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
        guild_id    : Snowflake option
        channel_id  : Snowflake option
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
        channel_id  : Snowflake option
    }

type ExecuteWebhook = {
        content     : string
        username    : string
        avatar_url  : string
        tts         : bool
        file        : File
        embeds      : Embed[]
    }

type ApiError =
    { code : uint32; message : string }
    
exception ApiException of ApiError

type USpec = Me | Uid of Snowflake
    with
    override x.ToString () =
        match x with Me -> "@me" | Uid x -> (string x)
