module BrokenDiscord.Events

open Types
open System
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

type ChannelPinsUpdateMessage = {
    [<JsonProperty "channel_id">]
    channelId : Snowflake;
    timestamp : DateTime;
}

type GuildDeleteMessage = {
    [<JsonProperty "guild_id">]
    GuildId : Snowflake;
    [<JsonProperty "unavailable">]
    Unavailable : bool;
}

type GuildEmojisUpdateMessage = {
    [<JsonProperty "guild_id">]
    GuildId : Snowflake;
    [<JsonProperty "emojis">]
    Emojis : list<Emoji>
}

type GuildMemberRemoveMessage = {
    [<JsonProperty "guild_id">]
    GuildId : Snowflake;
    [<JsonProperty "user">]
    User : User
}

type GuildMemberUpdateMessage = {
    [<JsonProperty "guild_id">]
    GuildId : Snowflake;
    [<JsonProperty "roles">]
    Roles : list<Snowflake>;
    [<JsonProperty "user">]
    User : User
    [<JsonProperty "nick">]
    Nick : string
}

type GuildMembersChunkMessage = {
    [<JsonProperty "guild_id">]
    GuildId : Snowflake;
    [<JsonProperty "members">]
    Members : list<GuildMember>
}

type GuildRoleCreateMessage = {
    [<JsonProperty "guild_id">]
    GuildId : Snowflake;
    [<JsonProperty "role">]
    Role : Role
}

type GuildRoleUpdateMessage = {
    [<JsonProperty "guild_id">]
    GuildId : Snowflake;
    [<JsonProperty "role">]
    Role : Role
}

type GuildRoleDeleteMessage = {
    [<JsonProperty "guild_id">]
    GuildId : Snowflake;
    [<JsonProperty "role_id">]
    RoleId : Snowflake
}

type MessageDeleteMessage = {
    [<JsonProperty "id">]
    Id : Snowflake;
    [<JsonProperty "channel_id">]
    ChannelId : Snowflake;
}

type MessageDeleteBulkMessage = {
    [<JsonProperty "ids">]
    Ids : list<Snowflake>;
    [<JsonProperty "channel_id">]
    ChannelId : Snowflake;
}

type MessageReactionAddedMessage = {
    [<JsonProperty "user_id">]
    UserId : Snowflake;
    [<JsonProperty "channel_id">]
    ChannelId : Snowflake;
    [<JsonProperty "message_id">]
    MessageId : Snowflake;
    [<JsonProperty "emoji">]
    Emoji : Emoji;
}

type MessageReactionRemovedMessage = {
    [<JsonProperty "user_id">]
    UserId : Snowflake;
    [<JsonProperty "channel_id">]
    ChannelId : Snowflake;
    [<JsonProperty "message_id">]
    MessageId : Snowflake;
    [<JsonProperty "emoji">]
    Emoji : Emoji;
}

type MessageReactionClearedMessage = {
    [<JsonProperty "channel_id">]
    ChannelId : Snowflake;
    [<JsonProperty "message_id">]
    MessageId : Snowflake;
    
}

type PresenceUpdateMessage = {
    [<JsonProperty "user">]
    User : User;
    [<JsonProperty "roles">]
    Roles : list<Snowflake>;
    [<JsonProperty "game">]
    Game : Activity;
    [<JsonProperty "guild_id">]
    GuildId : Snowflake;
    [<JsonProperty "status">]
    Status : string
}

type TypingStartMessage = {
    [<JsonProperty "channel_id">]
    ChannelId : Snowflake;
    [<JsonProperty "user_id">]
    UserId : Snowflake;
    [<JsonProperty "timestamp">]
    Timestamp : int
}

type VoiceServerUpdateMessage = {
    [<JsonProperty "token">]
    Token : string;
    [<JsonProperty "guild_id">]
    GuildId : Snowflake;
    [<JsonProperty "endpoint">]
    Endpoint : string;
}

type ChannelEvents =
    | Ready of Payload
    | Resume of Payload
    | ChannelCreate of Channel
    | ChannelUpdate of Channel
    | ChannelDelete of Channel
    | ChannelPinsUpdate of ChannelPinsUpdateMessage
    | GuildCreate of Guild
    | GuildUpdate of Guild
    | GuildDelete of GuildDeleteMessage
    | GuildBanAdd of User
    | GuildBanRemove of User
    | GuildEmojisUpdate of GuildEmojisUpdateMessage
    | GuildIntegrationsUpdate of Snowflake
    | GuildMemberAdd of GuildMember
    | GuildMemberRemove of GuildMemberRemoveMessage
    | GuildMemberUpdate of GuildMemberUpdateMessage
    | GuildMembersChunk of GuildMembersChunkMessage
    | GuildRoleCreate of GuildRoleCreateMessage
    | GuildRoleUpdate of GuildRoleUpdateMessage
    | GuildRoleDelete of GuildRoleDeleteMessage
    | MessageCreate of Message
    | MessageUpdate of Message
    | MessageDelete of MessageDeleteMessage
    | MessageDeleteBulk of MessageDeleteBulkMessage
    | MessageReactionAdded of MessageReactionAddedMessage
    | MessageReactionRemoved of MessageReactionRemovedMessage
    | MessageReactionCleared of MessageReactionClearedMessage 
    | PresenceUpdate of PresenceUpdateMessage
    | TypingStart of TypingStartMessage
    | UserUpdate of User
    | VoiceStateUpdate of VoiceState
    | VoiceServerUpdate of VoiceServerUpdateMessage