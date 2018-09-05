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
    guildId : Snowflake;
    [<JsonProperty "unavailable">]
    unavailable : bool;
}

type GuildEmojisUpdateMessage = {
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "emojis">]
    emojis : list<Emoji>
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
    roles : list<Snowflake>;
    [<JsonProperty "user">]
    user : User
    [<JsonProperty "nick">]
    nick : string
}

type GuildMembersChunkMessage = {
    [<JsonProperty "guild_id">]
    guildId : Snowflake;
    [<JsonProperty "members">]
    members : list<GuildMember>
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
    ids : list<Snowflake>;
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
    roles : list<Snowflake>;
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