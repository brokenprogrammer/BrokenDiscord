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
type ChannelPinsUpdateArgs(channelID : Snowflake, timestamp : int) =
    inherit System.EventArgs()

    member this.ChannelID = channelID
    member this.TimeStamp = timestamp

type GuildCreateArgs(guild : Guild) =
    inherit System.EventArgs()

    member this.Guild = guild

type GuildUpdateArgs(guild : Guild) =
    inherit System.EventArgs()

    member this.Guild = guild

type GuildDeleteArgs(guildID : Snowflake, unavailable : bool) =
    inherit System.EventArgs()

    member this.GuildID = guildID
    member this.Unavailable = unavailable

type GuildBanAddArgs(user : User) =
    inherit System.EventArgs()

    member this.User = user

type GuildBanRemoveArgs(user : User) =
    inherit System.EventArgs()

    member this.User = user

type GuildEmojisUpdateArgs(guildID : Snowflake, emojis : array<Emoji>) =
    inherit System.EventArgs()

    member this.GuildID = guildID
    member this.Emojis = emojis

type GuildIntegrationsUpdateArgs(guildID : Snowflake) =
    inherit System.EventArgs()

    member this.GuildID = guildID

type GuildMemberAddArgs(guildMember : GuildMember) =
    inherit System.EventArgs()

    member this.GuildMember = guildMember

type GuildMemberRemoveArgs(guildID : Snowflake, user : User) =
    inherit System.EventArgs()

    member this.GuildID = guildID
    member this.User = user

type GuildMemberUpdateArgs(guildID : Snowflake, roles : array<Snowflake>, 
                           user : User, nick : string) =
    inherit System.EventArgs()

    member this.GuildID = guildID
    member this.Roles = roles
    member this.User = user
    member this.Nick = nick

type GuildMembersChunkArgs(guildID : Snowflake, members : array<GuildMember>) =
    inherit System.EventArgs()

    member this.GuildID = guildID
    member this.Members = members

type GuildRoleCreateArgs(guildID : Snowflake, role : Role) =
    inherit System.EventArgs()

    member this.GuildID = guildID
    member this.Role = role

type GuildRoleUpdateArgs(guildID : Snowflake, role : Role) =
    inherit System.EventArgs()

    member this.GuildID = guildID
    member this.Role = role

type GuildRoleDeleteArgs(guildID : Snowflake, roleID : Snowflake) =
    inherit System.EventArgs()

    member this.GuildID = guildID
    member this.RoleID = roleID

type MessageCreateArgs(message : Message) =
    inherit System.EventArgs()

    member this.Message = message

type MessageUpdateArgs(message : Message) =
    inherit System.EventArgs()

    member this.Message = message

type MessageDeleteArgs(id : Snowflake, channelID : Snowflake) =
    inherit System.EventArgs()

    member this.ID = id
    member this.ChannelID = channelID

type MessageDeleteBulkArgs(ids : array<Snowflake>, channelID : Snowflake) =
    inherit System.EventArgs()

    member this.IDs = ids
    member this.ChannelID = channelID

type MessageReactionAddedArgs(userID : Snowflake, channelID : Snowflake, 
                              messageID : Snowflake, emoji : Emoji) =
    inherit System.EventArgs()

    member this.UserID = userID
    member this.ChannelID = channelID
    member this.MessageID = messageID
    member this.Emoji = emoji

type MessageReactionRemovedArgs(userID : Snowflake, channelID : Snowflake, 
                                messageID : Snowflake, emoji : Emoji) =
    inherit System.EventArgs()

    member this.UserID = userID
    member this.ChannelID = channelID
    member this.MessageID = messageID
    member this.Emoji = emoji

type MessageReactionRemoveAllArgs(channelID : Snowflake, messageID : Snowflake) =
    inherit System.EventArgs()

    member this.ChannelID = channelID
    member this.MessageID = messageID

type PresenceUpdateArgs(user : User, roles : array<Role>, 
                        game : Activity, guildID : Snowflake, status : string) =
    inherit System.EventArgs()

    member this.User = user
    member this.Roles = roles
    member this.Game = game
    member this.GuildID = guildID
    member this.Status = status

type TypingStartArgs(channelID : Snowflake, userID : Snowflake, timestamp : int) =
    inherit System.EventArgs()

    member this.ChannelID = channelID
    member this.UserID = userID
    member this.Timestamp = timestamp

type UserUpdateArgs(user : User) =
    inherit System.EventArgs()

    member this.User = user

type VoiceStateUpdateArgs(state : VoiceState) =
    inherit System.EventArgs()

    member this.State = state

type VoiceServerUpdateArgs(token : string, guildID : Snowflake, endpoint : string) =
    inherit System.EventArgs()

    member this.Token = token
    member this.GuildID = guildID
    member this.Endpoint = endpoint
