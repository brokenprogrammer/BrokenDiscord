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
    | dispatch = 0
    | heartbeat = 1
    | identify = 2
    | statusUpdate = 3
    | voiceStateUpdate = 4
    | voiceServerPing = 5
    | resume = 6
    | reconnect = 7
    | requestGuildMembers = 8
    | invalidSession = 9
    | hello = 10
    | heartbeatACK = 11

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
    
    let readyEvent = new Event<ReadyEventArgs>()

    let handleDispatch (payload : Payload) =
        let s = payload.s

        let t = payload.t.Value

        printf "%s %s" "\nHandling DISPATCH " t
        printf "%s" "\n"
        
        //TODO: All events from link is not added: https://discordapp.com/developers/docs/topics/gateway#commands-and-events
        //TODO: Make this Gateway type emit some kind of event that the implementer later can listen to.
        //TODO: Properly handle send the correct payload.
        match t with
        | "READY" -> readyEvent.Trigger(ReadyEventArgs({op=11; d=new JObject(new JProperty("op", 2)); s = Some 12; t = Some "ads"}))
        | "RESUMED" -> ()
        | "CHANNEL_CREATE" -> ()
        | "CHANNEL_UPDATE" -> ()
        | "CHANNEL_DELETE" -> ()
        | "CHANNEL_PINS_UPDATE" -> ()
        | "GUILD_CREATE" -> ()
        | "GUILD_UPDATE" -> ()
        | "GUILD_DELETE" -> ()
        | "GUILD_BAN_ADD" -> ()
        | "GUILD_BAN_REMOVE" -> ()
        | "GUILD_EMOJIS_UPDATE" -> ()
        | "GUILD_INTEGRATIONS_UPDATE" -> ()
        | "GUILD_MEMBER_ADD" -> ()
        | "GUILD_MEMBER_REMOVE" -> ()
        | "GUILD_MEMBER_UPDATE" -> ()
        | "GUILD_MEMBERS_CHUNK" -> ()
        | "GUILD_ROLE_CREATE" -> ()
        | "GUILD_ROLE_UPDATE" -> ()
        | "GUILD_ROLE_DELETE" -> ()
        | "MESSAGE_CREATE" -> ()
        | "MESSAGE_UPDATE" -> ()
        | "MESSAGE_DELETE" -> ()
        | "MESSAGE_DELETE_BULK" -> ()
        | "MESSAGE_REACTION_ADDED" -> ()
        | "MESSAGE_REACTION_REMOVED" -> ()
        | "MESSAGE_REACTIONS_CLEARED" -> ()
        | "PRESENCE_UPDATE" -> ()
        | "TYPING_START" -> ()
        | "USER_UPDATE" -> ()
        | "USER_SETTINGS_UPDATE" -> ()
        | "VOICE_STATE_UPDATE" -> ()
        | "VOICE_SERVER_UPDATE" -> ()
        | _ -> () // TODO: Log Unhandled event

    //TODO: better naming for this function
    let parseMessage (payload : Payload) =
        let op = enum<OpCode>(payload.op)
        
        match op with
        | OpCode.dispatch -> 
            handleDispatch payload
        | OpCode.heartbeat -> 1 |> ignore
        | OpCode.identify -> 2 |> ignore
        | OpCode.statusUpdate -> 3 |> ignore
        | OpCode.voiceStateUpdate -> 4 |> ignore
        | OpCode.voiceServerPing -> 5 |> ignore
        | OpCode.resume -> 6 |> ignore
        | OpCode.reconnect -> 7 |> ignore
        | OpCode.requestGuildMembers -> 8 |> ignore
        | OpCode.invalidSession -> 9 |> ignore
        | OpCode.hello -> 
            printf "%s" "Receieved Hello opcode, starting heartbeater...\n"
            let heartbeatInterval = payload.d.["heartbeat_interval"].Value<int>()
            heartbeat(heartbeatInterval) |> Async.Start
        | OpCode.heartbeatACK -> 11 |> ignore
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

    [<CLIEvent>]
    member this.ReadyEvent = readyEvent.Publish

    // Test method that calls the Run function with the target websocket uri
    member this.con() = Run "wss://gateway.discord.gg/?v=6&encoding=json" "NDc2NzQyMjI4NTg1MzQ5MTQy.DkyAlw.t9qBUy5MEfFGoHlIFYacVXIKxL4"

    interface IDisposable with
        member this.Dispose() = 
            socket.Dispose()