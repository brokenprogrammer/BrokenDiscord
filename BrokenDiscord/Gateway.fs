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
let ofJsonPart<'T> value (source : JObject) = ofJson<'T> (source.[value].ToString())
let ofJsonValue<'T> value (source : JObject) = (source.[value].Value<'T>())

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

    let gatewayEvent = new Event<ChannelEvents>()
    
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

        let payloadData = payload.d
        printf "CONTENT: %s" (payloadData.ToString())
        
        //TODO: Naming
        let trigger (du:'payload -> _) json =
            ofJson<'payload> json |> du |> gatewayEvent.Trigger

        //TODO: All events from link is not added: https://discordapp.com/developers/docs/topics/gateway#commands-and-events
        //TODO: Verify that single Snowflake cases gets parsed from JSON correctly.
        match t with
        | "READY"                       -> gatewayEvent.Trigger(Ready(payload))
        | "RESUMED"                     -> gatewayEvent.Trigger(Resume(payload))
        | "CHANNEL_CREATE"              -> payloadData.ToString() |> trigger ChannelCreate
        | "CHANNEL_UPDATE"              -> payloadData.ToString() |> trigger ChannelUpdate
        | "CHANNEL_DELETE"              -> payloadData.ToString() |> trigger ChannelDelete
        | "CHANNEL_PINS_UPDATE"         -> payloadData.ToString() |> trigger ChannelPinsUpdate  //TODO: Timestamp not being parsed correctly.
        | "GUILD_CREATE"                -> payloadData.ToString() |> trigger GuildCreate
        | "GUILD_UPDATE"                -> payloadData.ToString() |> trigger GuildUpdate
        | "GUILD_DELETE"                -> payloadData.ToString() |> trigger GuildDelete
        | "GUILD_BAN_ADD"               -> payloadData.ToString() |> trigger GuildBanAdd 
        | "GUILD_BAN_REMOVE"            -> payloadData.ToString() |> trigger GuildBanRemove
        | "GUILD_EMOJIS_UPDATE"         -> payloadData.ToString() |> trigger GuildEmojisUpdate
        | "GUILD_INTEGRATIONS_UPDATE"   -> payloadData.ToString() |> trigger GuildIntegrationsUpdate
        | "GUILD_MEMBER_ADD"            -> payloadData.ToString() |> trigger GuildMemberAdd
        | "GUILD_MEMBER_REMOVE"         -> payloadData.ToString() |> trigger GuildMemberRemove
        | "GUILD_MEMBER_UPDATE"         -> payloadData.ToString() |> trigger GuildMemberUpdate
        | "GUILD_MEMBERS_CHUNK"         -> payloadData.ToString() |> trigger GuildMembersChunk
        | "GUILD_ROLE_CREATE"           -> payloadData.ToString() |> trigger GuildRoleCreate
        | "GUILD_ROLE_UPDATE"           -> payloadData.ToString() |> trigger GuildRoleUpdate
        | "GUILD_ROLE_DELETE"           -> payloadData.ToString() |> trigger GuildRoleDelete
        | "MESSAGE_CREATE"              -> payloadData.ToString() |> trigger MessageCreate
        | "MESSAGE_UPDATE"              -> payloadData.ToString() |> trigger MessageUpdate
        | "MESSAGE_DELETE"              -> payloadData.ToString() |> trigger MessageDelete
        | "MESSAGE_DELETE_BULK"         -> payloadData.ToString() |> trigger MessageDeleteBulk
        | "MESSAGE_REACTION_ADDED"      -> payloadData.ToString() |> trigger MessageReactionAdded
        | "MESSAGE_REACTION_REMOVE"     -> payloadData.ToString() |> trigger MessageReactionRemoved
        | "MESSAGE_REACTIONS_CLEARED"   -> payloadData.ToString() |> trigger MessageReactionCleared
        | "PRESENCE_UPDATE"             -> payloadData.ToString() |> trigger PresenceUpdate
        | "TYPING_START"                -> payloadData.ToString() |> trigger TypingStart
        | "USER_UPDATE"                 -> payloadData.ToString() |> trigger UserUpdate
        | "USER_SETTINGS_UPDATE"        -> ()
        | "VOICE_STATE_UPDATE"          -> payloadData.ToString() |> trigger VoiceStateUpdate
        | "VOICE_SERVER_UPDATE"         -> payloadData.ToString() |> trigger VoiceServerUpdate
        | _                             -> () // TODO: Log Unhandled event

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
    member this.GatewayEvent = gatewayEvent.Publish

    interface IDisposable with
        member this.Dispose() = 
            socket.Dispose()