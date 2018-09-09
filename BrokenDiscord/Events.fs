module BrokenDiscord.Events

open Types
open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type ReadyMessage = {
    [<JsonProperty "v">]
    version : int
    user : User
    guilds : Guild[]
    session_id : string
    _trace : string[]
}

type ResumeMessage = {
    _trace : string[]
}

type ChannelPinsUpdateMessage = {
    [<JsonProperty "channel_id">]
    channelId : Snowflake;
    timestamp : DateTime;
}

type GuildDeleteMessage = {
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "unavailable">]
    unavailable : bool;
}

type GuildEmojisUpdateMessage = {
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "emojis">]
    emojis : Emoji[]
}

type GuildMemberRemoveMessage = {
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "user">]
    user : User
}

type GuildMemberUpdateMessage = {
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "roles">]
    roles : Snowflake[];
    [<JsonProperty "user">]
    user : User
    [<JsonProperty "nick">]
    nick : string
}

type GuildMembersChunkMessage = {
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "members">]
    members : GuildMember[]
}

type GuildRoleCreateMessage = {
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "role">]
    role : Role
}

type GuildRoleUpdateMessage = {
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "role">]
    role : Role
}

type GuildRoleDeleteMessage = {
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "role_id">]
    roleId : Snowflake
}

type MessageDeleteMessage = {
    [<JsonProperty "id">]
    id : Snowflake;
    [<JsonProperty "channel_id">]
    channelId : Snowflake;
}

type MessageDeleteBulkMessage = {
    [<JsonProperty "ids">]
    ids : Snowflake[];
    [<JsonProperty "channel_id">]
    channelId : Snowflake;
}

type MessageReactionAddedMessage = {
    [<JsonProperty "user_id">]
    userId : Snowflake;
    [<JsonProperty "channel_id">]
    channelId : Snowflake;
    [<JsonProperty "message_id">]
    messageId : Snowflake;
    [<JsonProperty "emoji">]
    emoji : Emoji;
}

type MessageReactionRemovedMessage = {
    [<JsonProperty "user_id">]
    userId : Snowflake;
    [<JsonProperty "channel_id">]
    channelId : Snowflake;
    [<JsonProperty "message_id">]
    messageId : Snowflake;
    [<JsonProperty "emoji">]
    emoji : Emoji;
}

type MessageReactionClearedMessage = {
    [<JsonProperty "channel_id">]
    channelId : Snowflake;
    [<JsonProperty "message_id">]
    messageId : Snowflake;
    
}

type PresenceUpdateMessage = {
    [<JsonProperty "user">]
    user : User;
    [<JsonProperty "roles">]
    roles : Snowflake[];
    [<JsonProperty "game">]
    game : Activity;
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "status">]
    status : string
}

type TypingStartMessage = {
    [<JsonProperty "channel_id">]
    channelId : Snowflake;
    [<JsonProperty "user_id">]
    userId : Snowflake;
    [<JsonProperty "timestamp">]
    timestamp : int
}

type VoiceServerUpdateMessage = {
    [<JsonProperty "token">]
    token : string;
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "endpoint">]
    endpoint : string;
}

type GatewayEvents =
    | Ready of ReadyMessage
    | Resume of ResumeMessage
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