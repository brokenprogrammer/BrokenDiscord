module BrokenDiscord.Types

open System

open Newtonsoft.Json.Linq
open Newtonsoft.Json

//TODO: This payload object might be isolated to the Gateway module.
// OP = opcode 
// d = event data
// s = sequence number
// t = event name
type Payload = {op : int; d : JObject; s : int option; t : string option}

type Snowflake = uint64

type Overwrite = {Id : Snowflake; Type : string; Allow : int; Deny : int}

type EmbedThumbnail = {
        URL         : string option;
        ProxyURL    : string option;
        Height      : int option;
        Width       : int option;
    }

type EmbedVideo = {
        URL     : string option;
        Height  : int option;
        Width   : int option;
    }

type EmbedImage = {
        URL         : string option;
        ProxyURL    : string option;
        Height      : int option;
        Width       : int option;
    }

type EmbedProvider = {
        Name    : string option;
        URL     : string option;
    }

type EmbedAuthor = {
        Name            : string option;
        URL             : string option;
        IconURL         : string option;
        ProxyIconURL    : string option;
    }

type EmbedFooter = {
        Text            : string;
        IconURL         : string option;
        ProxyIconURL    : string option;
    }

type EmbedField = {
        Name    : string;
        Value   : string;
        Inline  : bool option;
    }

type Embed = {
        Title       : string option;
        Type        : string option;
        Description : string option;
        URL         : string option;
        Timestamp   : DateTime option;
        Color       : int option;
        Footer      : EmbedFooter option;
        Image       : EmbedImage option;
        Thumbnail   : EmbedThumbnail option;
        Video       : EmbedVideo option;
        Provider    : EmbedProvider option;
        Author      : EmbedAuthor option;
        Fields      : list<EmbedField> option;
    }

type Attachment = {
        Id          : Snowflake;
        Filename    : string;
        Size        : int;
        URL         : string;
        ProxyURL    : string;
        Height      : int option;
        Width       : int option;
    }

type User = {
        Id              : Snowflake;
        Username        : string;
        Discriminator   : string;
        Avatar          : string option;
        Bot             : bool option;
        MFAEnabled      : bool option;
        Locale          : string option;
        Verified        : bool option;
        Email           : string option;
    }

type Role = {
        Id          : Snowflake;
        Name        : string;
        Color       : int;
        Hoise       : bool;
        Position    : int;
        Permissions : int;
        Managed     : bool;
        Mentionable : bool;
    }

type Emoji = {
        Id              : Snowflake option;
        Name            : string;
        Roles           : list<Role> option;
        User            : User option;
        RequireColons   : bool option;
        Managed         : bool option;
        Animated        : bool option;
    }

type Reaction = {
        Count   : int;
        Me      : bool;
        Emoji   : Emoji;
    }
    
type ChannelType = 
    | GuildText     = 0
    | DM            = 1
    | GuildVoice    = 2
    | GroupDM       = 3
    | GuildCategory = 4

type Channel = {
        Id                      : Snowflake;
        Type                    : ChannelType;
        GuildID                 : Snowflake option;
        Position                : int option;
        PermissionOverwrites    : list<Overwrite>;
        Name                    : string option;
        Topic                   : string option;
        NSFW                    : bool option;
        LastMessageID           : Snowflake option;
        Bitrate                 : int option;
        UserLimit               : int option;
        Recipients              : list<User>;
        Icon                    : string option;
        OwnerID                 : Snowflake option;
        ApplicationID           : Snowflake option;
        ParentID                : Snowflake option;
        LastPinTimestamp        : DateTime option;
    }

type MessageActivityType =
    | Join          = 1
    | Spectate      = 2
    | Listen        = 3
    | JoinRequest   = 5

type MessageActivity = {Type : MessageActivityType; PartyId : string option}

type MessageApplication = {
        Id          : Snowflake;
        CoverImage  : string;
        Description : string;
        Icon        : string;
        Name        : string;
    }

type MessageType = 
    | Default               = 0
    | RecipientAdd          = 1
    | RecipientRemove       = 2
    | Call                  = 3
    | ChannelNameChange     = 4
    | ChannelIconChange     = 5
    | ChannelPinnedMessage  = 6
    | GuildMemberJoin       = 7

type Message = {
        Id              : Snowflake;
        ChannelId       : Snowflake;
        Author          : User;
        Content         : string;
        Timestamp       : DateTime;
        EditedTimestamp : DateTime option;
        TTS             : bool;
        MentionEveryone : bool;
        Mentions        : list<User>;
        MentionRoles    : list<Role>
        Attachments     : list<Attachment>
        Embeds          : list<Embed>
        Reactions       : list<Reaction> option
        Nounce          : Snowflake option;
        Pinned          : bool;
        WebhookId       : Snowflake option;
        Type            : MessageType;
        Activiy         : MessageActivity option;
        Application     : MessageApplication option;
    }

type ActivityType = 
    | Game      = 0
    | Streaming = 1
    | Listening = 2

type ActivityTimestamps = {Start : int option; End : int option}

type ActivityParty = {Id : string option; Size : list<int> option}

type ActivityAssets = {
        LargeImage  : string option;
        LargeText   : string option;
        SmallImage  : string option;
        SmallText   : string option;
    }

type ActivitySecrets = {
        Join        : string option;
        Spectate    : string option;
        Match       : string option;
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
        Name            : string;
        Type            : ActivityType;
        URL             : string option;
        Timestamps      : ActivityTimestamps option;
        ApplicationId   : Snowflake option;
        Details         : string option;
        State           : string option;
        Party           : ActivityParty option;
        Assets          : ActivityAssets option;
        Secrets         : ActivitySecrets option;
        Instance        : bool option;
        Flags           : int;
    }

type VoiceState = {
        GuildId     : Snowflake option;
        ChannelId   : Snowflake option;
        UserId      : Snowflake;
        SessionId   : string;
        Deaf        : bool;
        Mute        : bool;
        SelfDeaf    : bool;
        SelfMute    : bool;
        Suppress    : bool;
    }

type PresenceUpdate = {
        User    : User;
        Roles   : list<Snowflake>;
        Game    : Activity option;
        GuildId : Snowflake;
        Status  : string;
    }

type GuildMember = {
        User        : User;
        Nick        : string option;
        Roles       : list<Snowflake>;
        JoinedAt    : DateTime;
        Deaf        : bool;
        Mute        : bool;
    }

type Guild =  {
        Id                              : Snowflake;
        Name                            : string;
        Icon                            : string option;
        Splash                          : string option;
        Owner                           : bool option;
        OwnerId                         : Snowflake;
        Permissions                     : int option;
        Region                          : string;
        AFKChannelId                    : Snowflake option;
        AFKTimeout                      : int;
        EmbedEnabled                    : bool option;
        EmbedChannelId                  : Snowflake option;
        VerificationLevel               : int;
        DefaultMessageNotifications     : int;
        ExplicitContentFilter           : int;
        Roles                           : list<Role>;
        Emojis                          : list<Emoji>;
        Features                        : list<string>;
        MFALevel                        : int;
        ApplicationId                   : Snowflake option;
        WidgetEnabled                   : bool option;
        WidgetChannelId                 : Snowflake option;
        SystemChannelId                 : Snowflake option;
        // Bellow are only sent with GUILD_CREATE Event
        JoinedAt                        : DateTime option;
        Large                           : bool option;
        Unavailable                     : bool option;
        MemberCount                     : int option;
        VoiceStates                     : list<VoiceState> option;
        Members                         : list<GuildMember> option;
        Channels                        : list<Channel> option;
        Presences                       : list<PresenceUpdate> option;
    }

//TODO: Should contain invite metadata
type Invite = {
        code                        : string;
        guild                       : Guild option;
        channel                     : Channel;
        approximate_presence_count  : int option;
        approximate_member_count    : int option;
    }

type InviteMetadata = {
        inviter     : User;
        uses        : int;
        max_uses    : int;
        max_age     : int;
        temporary   : bool;
        created_at  : DateTime;
        revoked     : bool;
    }

type WebModifyChannelParams = {
        name                    : string;
        position                : int;
        topic                   : string;
        nsfw                    : bool;
        bitrate                 : int;
        user_limit              : int;
        permission_overwrites   : list<Overwrite>;
        parent_id               : Snowflake;
    }

type WebGetChannelMessagesParams = {
        around  : Snowflake option;
        before  : Snowflake option;
        after   : Snowflake option;
        limit   : int option;
    }

type WebCreateMessageParams = {
        content      : string;
        nounce       : Snowflake option;
        tts          : bool option;
        file         : string option; //TODO: Api says type is "File contents"
        embed        : Embed option;
        payload_json : string option;
    }

type WebGetReactionsParams = {
        before  : Snowflake option;
        after   : Snowflake option;
        limit   : int option;
    }

type WebEditMessageParams = {
        content : string option;
        embed   : Embed option;
    }

type WebEditChannelPermissionsParams = {
        allow       : int;
        deny        : int;
        [<JsonProperty "type">]
        editType    : string;
    }

type WebCreateChannelInviteParams = {
        max_age     : int option;
        max_uses    : int option;
        temporary   : bool option;
        unique      : bool option;
    }

type WebGroupDMAddRecipientParams = {
        access_token    : string;
        nick            : string;
    }