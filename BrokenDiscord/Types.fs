module BrokenDiscord.Types

open System

//TODO: Go through all types and fix TODOs and conventions.
//TODO: Should flags, int types be represented as their type or as ints?

//TODO: This payload object might be isolated to the Gateway module.
//TODO: Make use of this object, Every object sent and receieved should be wrapped in the payload type
// OP = opcode 
// d = event data
// s = sequence number
// t = event name
type Payload = {op:int; d:string; s:int; t:string}

//TODO: Implement these types
type Snowflake = uint64

type User = {
        Id : Snowflake;
        Username : string;
        Discriminator : string;
        Avatar : string option;
        Bot : bool option;
        MFAEnabled : bool option;
        Locale : string option;
        Verified : bool option;
        Email : string option;
    }

type Role = {
        Id : Snowflake;
        Name : string;
        Color : int;
        Hoise : bool;
        Position : int;
        Permissions : int;
        Managed : bool;
        Mentionable : bool;
    }

type Emoji = {
        Id : Snowflake;
        Name : string;
        Roles : array<Role> option;
        User : User option;
        RequireColons : bool option;
        Managed : bool option;
        Animated : bool option;
    }

type Reaction = {
        Count : int;
        Me : bool;
        Emoji : Emoji;
    }

type Overwrite = {Id : Snowflake; Type : string; Allow : int; Deny : int}

type EmbedThumbnail = {
        URL : string option;
        ProxyURL : string option;
        Height : int option;
        Width : int option;
    }

type EmbedVideo = {
        URL : string option;
        Height : int option;
        Width : int option;
    }

type EmbedImage = {
        URL : string option;
        ProxyURL : string option;
        Height : int option;
        Width : int option;
    }

type EmbedProvider = {
        Name : string option;
        URL : string option;
    }

type EmbedAuthor = {
        Name : string option;
        URL : string option;
        IconURL : string option;
        ProxyIconURL : string option;
    }

type EmbedFooter = {
        Text : string;
        IconURL : string option;
        ProxyIconURL : string option;
    }

type EmbedField = {
        Name : string;
        Value : string;
        Inline : bool option;
    }

type Embed = {
        Title : string option;
        Type : string option;
        Description : string option;
        URL : string option;
        Timestamp : int option; //TODO: Timestamp type
        Color : int option;
        Footer : EmbedFooter option;
        Image : EmbedImage option;
        Thumbnail : EmbedThumbnail option;
        Video : EmbedVideo option;
        Provider : EmbedProvider option;
        Author : EmbedAuthor option;
        Fields : array<EmbedField> option;
    }

type Attachment = {
        Id : Snowflake;
        Filename : string;
        Size : int;
        URL : string;
        ProxyURL : string;
        Height : int option;
        Width : int option;
    }

//TODO: Naming conventions
//TODO: Make use of this in the Channel type
type ChannelType = 
    | guildText = 0
    | dM = 1
    | guildVoice = 2
    | groupDM = 3
    | guildCategory = 4

type Channel = {
        Id : Snowflake;
        Type : int;
        GuildID : Snowflake option;
        Position : int option;
        PermissionOverwrites : array<Overwrite>;
        Name : string option;
        Topic : string option;
        NSFW : bool option;
        LastMessageID : Snowflake option;
        Bitrate : int option;
        UserLimit : int option;
        Recipients : array<User>;
        Icon : string option;
        OwnerID : Snowflake option;
        ApplicationID : Snowflake option;
        ParentID : Snowflake option;
        LastPinTimestamp : int option; //TODO: Timestamp type?
    }

type MessageActivityType =
    | Join = 1
    | Spectate = 2
    | Listen = 3
    | JoinRequest = 4

type MessageActivity = {Type : int; PartyId : string}

type MessageApplication = {
        Id : Snowflake;
        CoverImage : string;
        Description : string;
        Icon : string;
        Name : string;
    }

type MessageType = 
    | Default = 0
    | RecipientAdd = 1
    | RecipientRemove = 2
    | Call = 3
    | ChannelNameChange = 4
    | ChannelIconChange = 5
    | ChannelPinnedMessage = 6
    | GuildMemberJoin = 7

type Message = {
        Id : Snowflake;
        ChannelId : Snowflake;
        Author : User;
        Content : string;
        Timestamp : int; //TODO: Timestamp type?
        EditedTimestamp : int option;
        TTS : bool;
        MentionEveryone : bool;
        Mentions : array<User>;
        MentionRoles : array<Role>
        Attachments : array<Attachment>
        Embeds : array<Embed>
        Reactions : array<Reaction> option
        Nounce : Snowflake option;
        Pinned : bool;
        WebhookId : Snowflake option;
        Type : int;
        Activiy : MessageActivity option;
        Application : MessageApplication option;
    }

type ActivityType = 
    | Game = 0
    | Streaming = 1
    | Listening = 2

type ActivityTimestamps = {Start : int option; End : int option}
type ActivityParty = {Id : string option; Size : array<int> option}

type ActivityAssets = {
        LargeImage : string option;
        LargeText : string option;
        SmallImage : string option;
        SmallText : string option;
    }

type ActivitySecrets = {
        Join : string option;
        Spectate : string option;
        Match : string option;
    }

[<Flags>]
type ActivityFlags = 
    | INSTANCE = 0b0000_0001
    | JOIN = 0b0000_0010
    | SPECTATE = 0b0000_0100
    | JOIN_REQUEST = 0b0000_1000
    | SYNC = 0b0001_0000
    | PLAY = 0b0010_0000

type Activity = {
        Name : string;
        Type : int;
        URL : string option;
        Timestamps : ActivityTimestamps option;
        ApplicationId : Snowflake option;
        Details : string option;
        State : string option;
        Party : ActivityParty option;
        Assets : ActivityAssets option;
        Secrets : ActivitySecrets option;
        Instance : bool option;
        Flags : int;
    }

type VoiceState = {
        GuildId : Snowflake option;
        ChannelId : Snowflake option;
        UserId : Snowflake;
        SessionId : string;
        Deaf : bool;
        Mute : bool;
        SelfDeaf : bool;
        SelfMute : bool;
        Suppress : bool;
    }

type PresenceUpdate = {
        User : User;
        Roles : array<Snowflake>;
        Game : Activity option;
        GuildId : Snowflake;
        Status : string;
    }

type GuildMember = {
        User : User;
        Nick : string option;
        Roles : array<Snowflake>;
        JoinedAt : int; //TODO: ISO8601 timestamp
        Deaf : bool;
        Mute : bool;
    }

type Guild =  {
        Id : Snowflake;
        Name : string;
        Icon : string option;
        Splash : string option;
        Owner : bool option;
        OwnerId : Snowflake;
        Permissions : int option;
        Region : string;
        AFKChannelId : Snowflake option;
        AFKTimeout : int;
        EmbedEnabled : bool option;
        EmbedChannelId : Snowflake option;
        VerificationLevel : int;
        DefaultMessageNotifications : int;
        ExplicitContentFilter : int;
        Roles : array<Role>;
        Emojis : array<Emoji>;
        Features : array<string>;
        MFALevel : int;
        ApplicationId : Snowflake option;
        WidgetEnabled : bool option;
        WidgetChannelId : Snowflake option;
        SystemChannelId : Snowflake option;
        // Bellow are only sent with GUILD_CREATE Event
        JoinedAt : int option; //TODO: Timestamp type
        Large : bool option;
        Unavailable : bool option;
        MemberCount : int option;
        VoiceStates : array<VoiceState> option;
        Members : array<GuildMember> option;
        Channels : array<Channel> option;
        Presences : array<PresenceUpdate> option;
    }