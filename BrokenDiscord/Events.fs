module BrokenDiscord.Events

open Types

type ReadyEventArgs(readyEvent : Payload) = 
    inherit System.EventArgs()

    member this.ReadyEvent = readyEvent

type ChannelCreateArgs(channel : Channel) = 
    inherit System.EventArgs()

    member this.Channel = channel

type ChannelUpdateArgs(channel : Channel) =
    inherit System.EventArgs()

    member this.Channel = channel

type ChannelDeleteArgs(channel : Channel) =
    inherit System.EventArgs()

    member this.Channel = channel

//TODO: Timestamp format ISO8601
type ChannelPinsUpdateArgs(channelId : Snowflake, timestamp : int) =
    inherit System.EventArgs()

    member this.ChannelId = channelId
    member this.TimeStamp = timestamp

type GuildCreateArgs(guild : Guild) =
    inherit System.EventArgs()

    member this.Guild = guild

type GuildUpdateArgs(guild : Guild) =
    inherit System.EventArgs()

    member this.Guild = guild

type GuildDeleteArgs(guildId : Snowflake, unavailable : bool) =
    inherit System.EventArgs()

    member this.GuildId = guildId
    member this.Unavailable = unavailable

type GuildBanAddArgs(user : User) =
    inherit System.EventArgs()

    member this.User = user

type GuildBanRemoveArgs(user : User) =
    inherit System.EventArgs()

    member this.User = user

type GuildEmojisUpdateArgs(guildId : Snowflake, emojis : list<Emoji>) =
    inherit System.EventArgs()

    member this.GuildId = guildId
    member this.Emojis = emojis

type GuildIntegrationsUpdateArgs(guildId : Snowflake) =
    inherit System.EventArgs()

    member this.GuildId = guildId

type GuildMemberAddArgs(guildMember : GuildMember) =
    inherit System.EventArgs()

    member this.GuildMember = guildMember

type GuildMemberRemoveArgs(guildId : Snowflake, user : User) =
    inherit System.EventArgs()

    member this.GuildId = guildId
    member this.User = user

type GuildMemberUpdateArgs(guildId : Snowflake, roles : list<Snowflake>, 
                           user : User, nick : string) =
    inherit System.EventArgs()

    member this.GuildId = guildId
    member this.Roles = roles
    member this.User = user
    member this.Nick = nick

type GuildMembersChunkArgs(guildId : Snowflake, members : list<GuildMember>) =
    inherit System.EventArgs()

    member this.GuildId = guildId
    member this.Members = members

type GuildRoleCreateArgs(guildId : Snowflake, role : Role) =
    inherit System.EventArgs()

    member this.GuildId = guildId
    member this.Role = role

type GuildRoleUpdateArgs(guildId : Snowflake, role : Role) =
    inherit System.EventArgs()

    member this.GuildId = guildId
    member this.Role = role

type GuildRoleDeleteArgs(guildId : Snowflake, roleId : Snowflake) =
    inherit System.EventArgs()

    member this.GuildId = guildId
    member this.RoleId = roleId

type MessageCreateArgs(message : Message) =
    inherit System.EventArgs()

    member this.Message = message

type MessageUpdateArgs(message : Message) =
    inherit System.EventArgs()

    member this.Message = message

type MessageDeleteArgs(id : Snowflake, channelId : Snowflake) =
    inherit System.EventArgs()

    member this.Id = id
    member this.ChannelID = channelId

type MessageDeleteBulkArgs(ids : list<Snowflake>, channelId : Snowflake) =
    inherit System.EventArgs()

    member this.Ids = ids
    member this.ChannelId = channelId

type MessageReactionAddedArgs(userId : Snowflake, channelId : Snowflake, 
                              messageId : Snowflake, emoji : Emoji) =
    inherit System.EventArgs()

    member this.UserId = userId
    member this.ChannelId = channelId
    member this.MessageId = messageId
    member this.Emoji = emoji

type MessageReactionRemovedArgs(userId : Snowflake, channelId : Snowflake, 
                                messageId : Snowflake, emoji : Emoji) =
    inherit System.EventArgs()

    member this.UserId = userId
    member this.ChannelId = channelId
    member this.MessageId = messageId
    member this.Emoji = emoji

type MessageReactionRemoveAllArgs(channelId : Snowflake, messageId : Snowflake) =
    inherit System.EventArgs()

    member this.ChannelId = channelId
    member this.MessageId = messageId

type PresenceUpdateArgs(user : User, roles : list<Role>, 
                        game : Activity, guildId : Snowflake, status : string) =
    inherit System.EventArgs()

    member this.User = user
    member this.Roles = roles
    member this.Game = game
    member this.GuildId = guildId
    member this.Status = status

type TypingStartArgs(channelId : Snowflake, userId : Snowflake, timestamp : int) =
    inherit System.EventArgs()

    member this.ChannelId = channelId
    member this.UserId = userId
    member this.Timestamp = timestamp

type UserUpdateArgs(user : User) =
    inherit System.EventArgs()

    member this.User = user

type VoiceStateUpdateArgs(state : VoiceState) =
    inherit System.EventArgs()

    member this.State = state

type VoiceServerUpdateArgs(token : string, guildId : Snowflake, endpoint : string) =
    inherit System.EventArgs()

    member this.Token = token
    member this.GuildId = guildId
    member this.Endpoint = endpoint
