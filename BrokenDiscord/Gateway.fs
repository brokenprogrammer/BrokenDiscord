module BrokenDiscord.Gateway

open System
open System.Net.WebSockets
open System.Threading
open System.Threading.Tasks

open Newtonsoft.Json
open Newtonsoft.Json.Linq

open BrokenDiscord.Events
open BrokenDiscord.Types
open BrokenDiscord.WebSockets
open BrokenDiscord.WebSockets.WebSocket

type OpCode = 
    | Dispatch = 0
    | Heartbeat = 1
    | Identify = 2
    | StatusUpdate = 3
    | VoiceStateUpdate = 4
    | VoiceServerPing = 5
    | Resume = 6
    | Reconnect = 7
    | RequestGuildMembers = 8
    | InvalidSession = 9
    | Hello = 10
    | HeartbeatACK = 11

//TODO: Place these in their own json module
let jsonConverter = Fable.JsonConverter() :> JsonConverter
let toJson value = JsonConvert.SerializeObject(value, [|jsonConverter|])
let ofJson<'T> value = JsonConvert.DeserializeObject<'T>(value, [|jsonConverter|])

type ISerializable =
    abstract member Serialize : unit -> string

type HeartbeatPacket (seq : int) =
    member this.seq = seq

    interface ISerializable with
        member this.Serialize() =
            let payload = {op = 1; d = JObject.FromObject(this); s = None; t = None}
            toJson payload

type IdentifyPacket (token : string, shard : int, numshards : int) =
    //TODO: Better way to construct the properties.
    let getProperties = 
        new JObject(new JProperty("$os", "linux"), 
            new JProperty("$browser", "brokendiscord"), 
            new JProperty("$device", "brokendiscord"))
    
    member this.token = token
    member this.properties = getProperties
    member this.compress = false //true TODO: Change this to true when zlib decryption has been added.
    member this.large_threshold = 250
    member this.shard =  [|shard; numshards|]

    interface ISerializable with
        member this.Serialize() =
            let payload = {op = 2; d = JObject.FromObject(this); s = None; t = None}
            toJson payload

type Gateway () =
    let socket : ClientWebSocket = new ClientWebSocket()

    let readyEvent                      = new Event<ReadyEventArgs>()
    let resumedEvent                    = new Event<ReadyEventArgs>()
    let channelCreatedEvent             = new Event<ChannelCreateArgs>()
    let channelUpdatedEvent             = new Event<ChannelUpdateArgs>()
    let channelDeletedEvent             = new Event<ChannelDeleteArgs>()
    let channelPinsUpdatedEvent         = new Event<ChannelPinsUpdateArgs>()
    let guildCreatedEvent               = new Event<GuildCreateArgs>()
    let guildUpdatedEvent               = new Event<GuildUpdateArgs>()
    let guildDeletedEvent               = new Event<GuildDeleteArgs>()
    let guildBanAddEvent                = new Event<GuildBanAddArgs>()
    let guildBanRemoveEvent             = new Event<GuildBanRemoveArgs>()
    let guildEmojisUpdatedEvent         = new Event<GuildEmojisUpdateArgs>()
    let guildIntegrationsUpdatedEvent   = new Event<GuildIntegrationsUpdateArgs>()
    let guildMemberAddEvent             = new Event<GuildMemberAddArgs>()
    let guildMemberUpdateEvent          = new Event<GuildMemberUpdateArgs>()
    let guildMemberRemoveEvent          = new Event<GuildMemberRemoveArgs>()
    let guildMembersChunkEvent          = new Event<GuildMembersChunkArgs>()
    let guildRoleCreateEvent            = new Event<GuildRoleCreateArgs>()
    let guildRoleUpdateEvent            = new Event<GuildRoleUpdateArgs>()
    let guildRoleDeleteEvent            = new Event<GuildRoleDeleteArgs>()
    let messageCreateEvent              = new Event<MessageCreateArgs>()
    let messageUpdateEvent              = new Event<MessageUpdateArgs>()
    let messageDeleteEvent              = new Event<MessageDeleteArgs>()
    let messageDeleteBulkEvent          = new Event<MessageDeleteBulkArgs>()
    let messageReactionAddEvent         = new Event<MessageReactionAddedArgs>()
    let messageReactionRemoveEvent      = new Event<MessageReactionRemovedArgs>()
    let messageReactionClearedEvent     = new Event<MessageReactionRemoveAllArgs>()
    let presenceUpdateEvent             = new Event<PresenceUpdateArgs>()
    let typingStartEvent                = new Event<TypingStartArgs>()
    let userUpdateEvent                 = new Event<UserUpdateArgs>()
    //let userSettingsUpdateEvent = 
    let voiceStateUpdateEvent           = new Event<VoiceStateUpdateArgs>()
    let voiceServerUpdateEvent          = new Event<VoiceServerUpdateArgs>()

    let Send (packet : ISerializable) = 
        async {
            printf "%s" ("Sending packet" + packet.Serialize())
            do! WebSocket.sendMessageUTF8 (packet.Serialize()) socket
            printf "%s" "Sent packet...\n"
        }

    let rec heartbeat (interval : int) =
        async {
            do! interval |> Task.Delay |> Async.AwaitTask |> Async.Ignore
            do! Send(HeartbeatPacket(interval)) |> Async.Ignore
            printf "%s" "Sent Heartbeat packet\n"
            do! heartbeat interval
        }

    let handleDispatch (payload : Payload) =
        let s = payload.s

        let t = payload.t.Value

        printf "%s %s" "\nHandling DISPATCH " t
        printf "%s" "\n"

        let j = payload.d
        printf "CONTENT: %s" (j.ToString())
        
        //TODO: All events from link is not added: https://discordapp.com/developers/docs/topics/gateway#commands-and-events
        //TODO: Make this Gateway type emit some kind of event that the implementer later can listen to.
        //TODO: Properly handle send the correct payload.
        match t with
        | "READY" -> readyEvent.Trigger(ReadyEventArgs(payload))
        | "RESUMED" -> readyEvent.Trigger(ReadyEventArgs(payload))
        | "CHANNEL_CREATE" -> channelCreatedEvent.Trigger(ChannelCreateArgs(ofJson<Channel> (payload.d.ToString())))
        | "CHANNEL_UPDATE" -> channelUpdatedEvent.Trigger(ChannelUpdateArgs(ofJson<Channel> (payload.d.ToString())))
        | "CHANNEL_DELETE" -> channelDeletedEvent.Trigger(ChannelDeleteArgs(ofJson<Channel> (payload.d.ToString())))
        | "CHANNEL_PINS_UPDATE" ->
            //TODO: Check for last_pin_timestamp and guildId being optional
            let channelId = payload.d.["channel_id"].Value<Snowflake>()
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            let timestamp = DateTime.Parse(payload.d.["last_pin_timestamp"].Value<string>())
            channelPinsUpdatedEvent.Trigger(ChannelPinsUpdateArgs(channelId, timestamp))
        | "GUILD_CREATE" -> guildCreatedEvent.Trigger(GuildCreateArgs(ofJson<Guild> (payload.d.ToString())))
        | "GUILD_UPDATE" -> guildUpdatedEvent.Trigger(GuildUpdateArgs(ofJson<Guild> (payload.d.ToString())))
        | "GUILD_DELETE" -> 
            let guildId = payload.d.["id"].Value<Snowflake>()
            let unavailable = payload.d.["unavailable"].Value<bool>()
            guildDeletedEvent.Trigger(GuildDeleteArgs(guildId, unavailable))
        | "GUILD_BAN_ADD" -> guildBanAddEvent.Trigger(GuildBanAddArgs(ofJson<User> (payload.d.ToString())))
        | "GUILD_BAN_REMOVE" -> guildBanRemoveEvent.Trigger(GuildBanRemoveArgs(ofJson<User> (payload.d.ToString())))
        | "GUILD_EMOJIS_UPDATE" -> 
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            let emojis = ofJson<list<Emoji>> (payload.d.["emojis"].ToString())
            guildEmojisUpdatedEvent.Trigger(GuildEmojisUpdateArgs(guildId, emojis))
        | "GUILD_INTEGRATIONS_UPDATE" -> 
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            guildIntegrationsUpdatedEvent.Trigger(GuildIntegrationsUpdateArgs(guildId))
        | "GUILD_MEMBER_ADD" -> guildMemberAddEvent.Trigger(GuildMemberAddArgs(ofJson<GuildMember> (payload.d.ToString())))
        | "GUILD_MEMBER_REMOVE" -> 
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            let user = ofJson<User> (payload.d.["user"].ToString())
            guildMemberRemoveEvent.Trigger(GuildMemberRemoveArgs(guildId, user))
        | "GUILD_MEMBER_UPDATE" -> 
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            let roles = ofJson<list<Snowflake>> (payload.d.["roles"].ToString())
            let user = ofJson<User> (payload.d.["user"].ToString())
            let nick = payload.d.["nick"].Value<string>()
            guildMemberUpdateEvent.Trigger(GuildMemberUpdateArgs(guildId, roles, user, nick))
        | "GUILD_MEMBERS_CHUNK" -> 
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            let members = ofJson<list<GuildMember>> (payload.d.["members"].ToString())
            guildMembersChunkEvent.Trigger(GuildMembersChunkArgs(guildId, members))
        | "GUILD_ROLE_CREATE" -> 
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            let role = ofJson<Role> (payload.d.["role"].ToString())
            guildRoleCreateEvent.Trigger(GuildRoleCreateArgs(guildId, role))
        | "GUILD_ROLE_UPDATE" -> 
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            let role = ofJson<Role> (payload.d.["role"].ToString())
            guildRoleUpdateEvent.Trigger(GuildRoleUpdateArgs(guildId, role))
        | "GUILD_ROLE_DELETE" -> 
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            let roleId = payload.d.["role_id"].Value<Snowflake>()
            guildRoleDeleteEvent.Trigger(GuildRoleDeleteArgs(guildId, roleId))
        | "MESSAGE_CREATE" -> messageCreateEvent.Trigger(MessageCreateArgs(ofJson<Message> (payload.d.ToString())))
        | "MESSAGE_UPDATE" -> messageUpdateEvent.Trigger(MessageUpdateArgs(ofJson<Message> (payload.d.ToString())))
        | "MESSAGE_DELETE" -> 
            let id = payload.d.["id"].Value<Snowflake>()
            let channelId = payload.d.["channel_id"].Value<Snowflake>()
            messageDeleteEvent.Trigger(MessageDeleteArgs(id, channelId))
        | "MESSAGE_DELETE_BULK" -> 
            let ids = payload.d.["ids"].Value<list<Snowflake>>()
            let channelId = payload.d.["channel_id"].Value<Snowflake>()
            messageDeleteBulkEvent.Trigger(MessageDeleteBulkArgs(ids, channelId))
        | "MESSAGE_REACTION_ADDED" -> 
            let userId = payload.d.["user_id"].Value<Snowflake>()
            let channelId = payload.d.["channel_id"].Value<Snowflake>()
            let messageId = payload.d.["message_id"].Value<Snowflake>()
            let emoji = ofJson<Emoji> (payload.d.ToString())
            messageReactionAddEvent.Trigger(MessageReactionAddedArgs(userId, channelId, messageId, emoji))
        | "MESSAGE_REACTION_REMOVED" -> 
            let userId = payload.d.["user_id"].Value<Snowflake>()
            let channelId = payload.d.["channel_id"].Value<Snowflake>()
            let messageId = payload.d.["message_id"].Value<Snowflake>()
            let emoji = ofJson<Emoji> (payload.d.ToString())
            messageReactionRemoveEvent.Trigger(MessageReactionRemovedArgs(userId, channelId, messageId, emoji))
        | "MESSAGE_REACTIONS_CLEARED" ->
            let channelId = payload.d.["channel_id"].Value<Snowflake>()
            let messageId = payload.d.["message_id"].Value<Snowflake>()
            messageReactionClearedEvent.Trigger(MessageReactionRemoveAllArgs(channelId, messageId))
        | "PRESENCE_UPDATE" -> 
            let user = ofJson<User> (payload.d.["user"].ToString())
            let roles = ofJson<list<Snowflake>> (payload.d.["roles"].ToString())
            let game = ofJson<Activity> (payload.d.["game"].ToString())
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            let status = payload.d.["status"].Value<string>()
            presenceUpdateEvent.Trigger(PresenceUpdateArgs(user, roles, game, guildId, status))
        | "TYPING_START" -> 
            let channelId = payload.d.["channel_id"].Value<Snowflake>()
            let userId = payload.d.["user_id"].Value<Snowflake>()
            let timestamp = payload.d.["timestamp"].Value<int>()
            typingStartEvent.Trigger(TypingStartArgs(channelId, userId, timestamp))
        | "USER_UPDATE" -> userUpdateEvent.Trigger(UserUpdateArgs(ofJson<User> (payload.d.ToString())))
        | "USER_SETTINGS_UPDATE" -> ()
        | "VOICE_STATE_UPDATE" -> voiceStateUpdateEvent.Trigger(VoiceStateUpdateArgs(ofJson<VoiceState> (payload.d.ToString())))
        | "VOICE_SERVER_UPDATE" -> 
            let token = payload.d.["token"].Value<string>()
            let guildId = payload.d.["guild_id"].Value<Snowflake>()
            let endpoint = payload.d.["endpoint"].Value<string>()
            voiceServerUpdateEvent.Trigger(VoiceServerUpdateArgs(token, guildId, endpoint))
        | _ -> () // TODO: Log Unhandled event

    //TODO: better naming for this function
    let parseMessage (payload : Payload) =
        let op = enum<OpCode>(payload.op)
        
        match op with
        | OpCode.Dispatch -> 
            handleDispatch payload
        | OpCode.Heartbeat -> 1 |> ignore
        | OpCode.Identify -> 2 |> ignore
        | OpCode.StatusUpdate -> 3 |> ignore
        | OpCode.VoiceStateUpdate -> 4 |> ignore
        | OpCode.VoiceServerPing -> 5 |> ignore
        | OpCode.Resume -> 6 |> ignore
        | OpCode.Reconnect -> 7 |> ignore
        | OpCode.RequestGuildMembers -> 8 |> ignore
        | OpCode.InvalidSession -> 9 |> ignore
        | OpCode.Hello -> 
            printf "%s" "Receieved Hello opcode, starting heartbeater...\n"
            let heartbeatInterval = payload.d.["heartbeat_interval"].Value<int>()
            heartbeat(heartbeatInterval) |> Async.Start
        | OpCode.HeartbeatACK -> 11 |> ignore
        | _ -> 0 |> ignore

    let Run (uri : string) (token : string) = 
        async {
            printf "%s" "Connecting...\n"
            
            do! socket.ConnectAsync(Uri(uri), CancellationToken.None) |> Async.AwaitTask
            
            let identification = IdentifyPacket(token, 0, 1)
            do! Send(identification) |> Async.Ignore

            while socket.State = WebSocketState.Open do
                let! payload =  WebSocket.receieveMessageUTF8 socket
                ofJson<Payload> payload |> parseMessage

                printf "%s" "End of run loop \n"
            
            printf "%s" (socket.State.ToString())
        }

    // Test method that calls the Run function with the target websocket uri
    member this.con() = Run "wss://gateway.discord.gg/?v=6&encoding=json" "NDc2NzQyMjI4NTg1MzQ5MTQy.DkyAlw.t9qBUy5MEfFGoHlIFYacVXIKxL4"

    [<CLIEvent>]
    member this.ReadyEvent = readyEvent.Publish

    [<CLIEvent>]
    member this.ResumedEvent = resumedEvent.Publish
    
    [<CLIEvent>]
    member this.ChannelCreatedEvent = channelCreatedEvent.Publish       
    
    [<CLIEvent>]
    member this.ChannelUpdatedEvent = channelUpdatedEvent.Publish
    
    [<CLIEvent>]
    member this.ChannelDeletedEvent = channelDeletedEvent.Publish
    
    [<CLIEvent>]
    member this.ChannelPinsUpdatedEvent = channelPinsUpdatedEvent.Publish
    
    [<CLIEvent>]
    member this.GuildCreatedEvent = guildCreatedEvent.Publish
    
    [<CLIEvent>]
    member this.GuildUpdatedEvent = guildUpdatedEvent.Publish
    
    [<CLIEvent>]
    member this.GuildDeletedEvent = guildDeletedEvent.Publish
    
    [<CLIEvent>]
    member this.GuildBanAddEvent = guildBanAddEvent.Publish    
    
    [<CLIEvent>]
    member this.GuildBanRemoveEvent = guildBanRemoveEvent.Publish     
    
    [<CLIEvent>]
    member this.GuildEmojisUpdatedEvent = guildEmojisUpdatedEvent.Publish
    
    [<CLIEvent>]
    member this.GuildIntegrationsUpdatedEvent = guildIntegrationsUpdatedEvent.Publish
    
    [<CLIEvent>]
    member this.GuildMemberAddEvent = guildMemberAddEvent.Publish
    
    [<CLIEvent>]
    member this.GuildMemberUpdateEvent = guildMemberUpdateEvent.Publish     
    
    [<CLIEvent>]
    member this.GuildMemberRemoveEvent = guildMemberRemoveEvent.Publish
    
    [<CLIEvent>]
    member this.GuildMembersChunkEvent = guildMembersChunkEvent.Publish   
    
    [<CLIEvent>]
    member this.GuildRoleCreateEvent = guildRoleCreateEvent.Publish 
    
    [<CLIEvent>]
    member this.GuildRoleUpdateEvent = guildRoleUpdateEvent.Publish 
    
    [<CLIEvent>]
    member this.GuildRoleDeleteEvent = guildRoleDeleteEvent.Publish     
    
    [<CLIEvent>]
    member this.MessageCreateEvent = messageCreateEvent.Publish   
    
    [<CLIEvent>]
    member this.MessageUpdateEvent = messageUpdateEvent.Publish   
    
    [<CLIEvent>]
    member this.MessageDeleteEvent = messageDeleteEvent.Publish
    
    [<CLIEvent>]
    member this.MessageDeleteBulkEvent = messageDeleteBulkEvent.Publish
    
    [<CLIEvent>]
    member this.MessageReactionAddEvent = messageReactionAddEvent.Publish
    
    [<CLIEvent>]
    member this.MessageReactionRemoveEvent = messageReactionRemoveEvent.Publish
    
    [<CLIEvent>]
    member this.MessageReactionClearedEvent = messageReactionClearedEvent.Publish
    
    [<CLIEvent>]
    member this.PresenceUpdateEvent = presenceUpdateEvent.Publish        
    
    [<CLIEvent>]
    member this.TypingStartEvent = typingStartEvent.Publish       
    
    [<CLIEvent>]
    member this.UserUpdateEvent = userUpdateEvent.Publish
    
    [<CLIEvent>]
    member this.VoiceStateUpdateEvent = voiceStateUpdateEvent.Publish      
    
    [<CLIEvent>]
    member this.VoiceServerUpdateEvent = voiceServerUpdateEvent.Publish

    interface IDisposable with
        member this.Dispose() = 
            socket.Dispose()