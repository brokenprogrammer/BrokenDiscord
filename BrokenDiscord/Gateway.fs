module BrokenDiscord.Gateway

open System
open System.Net.WebSockets
open System.Threading
open System.Threading.Tasks

open Newtonsoft.Json.Linq

open BrokenDiscord.Events
open BrokenDiscord.Types
open BrokenDiscord.Json
open BrokenDiscord.Json.Json
open BrokenDiscord.WebSockets
open BrokenDiscord.WebSockets.WebSocket
    
type ISerializable =
    abstract member Serialize : unit -> string

type HeartbeatPacket (seq : int) =
    member this.seq = seq

    interface ISerializable with
        member this.Serialize() =
            let payload = {op = OpCode.Heartbeat; d = JObject.FromObject(this); s = None; t = None}
            toJson payload

type IdentifyPacket (token : string, shard : int, numshards : int) =
    //TODO: Better way to construct the properties.
    let getProperties = 
        new JObject(new JProperty("$os", "linux"), 
            new JProperty("$browser", "brokendiscord"), 
            new JProperty("$device", "brokendiscord"))
    
    member this.token = token
    member this.properties = getProperties
    member this.compress = false //true TODO: Change this to true when zlib decompression has been added.
    member this.large_threshold = 250
    member this.shard =  [|shard; numshards|]

    interface ISerializable with
        member this.Serialize() =
            let payload = {op = OpCode.Identify; d = JObject.FromObject(this); s = None; t = None}
            toJson payload

type ResumePacket = {
        token : string
        session_id : string
        seq : int
    } with
    interface ISerializable with
        member this.Serialize() =
            let payload = {op = OpCode.Resume; d = JObject.FromObject(this); s = None; t = None}
            toJson payload

type Gateway () =
    let mutable socket : ClientWebSocket = new ClientWebSocket()
    let MAX_RECONNECTS = 6

    let mutable tokenvalue = ""
    let mutable session_id = ""
    let mutable seq = -1

    let gatewayURI = "wss://gateway.discord.gg/?v=6&encoding=json"
    let gatewayEvent = new Event<GatewayEvents>()
    
    let Send (packet : ISerializable) = 
        async {
            do! WebSocket.sendMessageUTF8 (packet.Serialize()) socket
        }

    let rec heartbeat (interval : int) =
        async {
            do! interval |> Task.Delay |> Async.AwaitTask |> Async.Ignore
            do! Send(HeartbeatPacket(interval)) |> Async.Ignore
            do! heartbeat interval
        }

    let handleDispatch (payload : Payload) =
        let t = payload.t.Value
        
        match payload.s with 
        | Some x -> seq <- x
        | _ -> ()
        
        if t = "READY" then
            session_id <- (ofJsonValue "session_id" payload.d)

        let payloadData = payload.d
        let payloadJson = payloadData.ToString()
        
        //TODO: Naming
        let trigger eventType =
            ofJson payloadJson |> eventType |> gatewayEvent.Trigger

        match t with
        | "READY"                       -> trigger Ready
        | "RESUMED"                     -> trigger Resume
        | "CHANNEL_CREATE"              -> trigger ChannelCreate
        | "CHANNEL_UPDATE"              -> trigger ChannelUpdate
        | "CHANNEL_DELETE"              -> trigger ChannelDelete
        | "CHANNEL_PINS_UPDATE"         -> trigger ChannelPinsUpdate  //TODO: Timestamp not being parsed correctly.
        | "GUILD_CREATE"                -> trigger GuildCreate
        | "GUILD_UPDATE"                -> trigger GuildUpdate
        | "GUILD_DELETE"                -> trigger GuildDelete
        | "GUILD_BAN_ADD"               -> trigger GuildBanAdd 
        | "GUILD_BAN_REMOVE"            -> trigger GuildBanRemove
        | "GUILD_EMOJIS_UPDATE"         -> trigger GuildEmojisUpdate
        | "GUILD_INTEGRATIONS_UPDATE"   -> trigger GuildIntegrationsUpdate
        | "GUILD_MEMBER_ADD"            -> trigger GuildMemberAdd
        | "GUILD_MEMBER_REMOVE"         -> trigger GuildMemberRemove
        | "GUILD_MEMBER_UPDATE"         -> trigger GuildMemberUpdate
        | "GUILD_MEMBERS_CHUNK"         -> trigger GuildMembersChunk
        | "GUILD_ROLE_CREATE"           -> trigger GuildRoleCreate
        | "GUILD_ROLE_UPDATE"           -> trigger GuildRoleUpdate
        | "GUILD_ROLE_DELETE"           -> trigger GuildRoleDelete
        | "MESSAGE_CREATE"              -> trigger MessageCreate
        | "MESSAGE_UPDATE"              -> trigger MessageUpdate
        | "MESSAGE_DELETE"              -> trigger MessageDelete
        | "MESSAGE_DELETE_BULK"         -> trigger MessageDeleteBulk
        | "MESSAGE_REACTION_ADDED"      -> trigger MessageReactionAdded
        | "MESSAGE_REACTION_REMOVE"     -> trigger MessageReactionRemoved
        | "MESSAGE_REACTIONS_CLEARED"   -> trigger MessageReactionCleared
        | "PRESENCE_UPDATE"             -> trigger PresenceUpdate
        | "TYPING_START"                -> trigger TypingStart
        | "USER_UPDATE"                 -> trigger UserUpdate
        | "USER_SETTINGS_UPDATE"        -> ()
        | "VOICE_STATE_UPDATE"          -> trigger VoiceStateUpdate
        | "VOICE_SERVER_UPDATE"         -> trigger VoiceServerUpdate
        | _                             -> () // TODO: Log Unhandled event

    let Reconnect (token : string) (identify : bool) =
        async {
            if socket.State = WebSocketState.Closed then
                socket.Dispose()
            else 
                do! WebSocket.close WebSocketCloseStatus.Empty "" socket
                socket.Dispose()

            socket <- new ClientWebSocket()
            do! socket.ConnectAsync(Uri(gatewayURI), CancellationToken.None) |> Async.AwaitTask


            let resume = 
                async {
                    let resume : ResumePacket = {token = token; session_id = session_id; seq = seq}
                    do! Send(resume) |> Async.Ignore
                }

            let reidentify =
                async {
                    do! 5000 |> Task.Delay |> Async.AwaitTask |> Async.Ignore
                    let identification = IdentifyPacket(token, 0, 1)
                    do! Send(identification) |> Async.Ignore
                }
                
            if identify then
                do! reidentify
            else
                do! resume
        }

    //TODO: better naming for this function
    let parseMessage (message : string) =
        let op = JObject.Parse(message) |> ofJsonValue<int> "op" |> enum<OpCode>

        match op with
        | OpCode.Dispatch -> 
            ofJson<Payload> message |> handleDispatch
        | OpCode.Heartbeat -> 1 |> ignore
        | OpCode.Identify -> 2 |> ignore
        | OpCode.StatusUpdate -> 3 |> ignore
        | OpCode.VoiceStateUpdate -> 4 |> ignore
        | OpCode.VoiceServerPing -> 5 |> ignore
        | OpCode.Resume -> 6 |> ignore
        | OpCode.Reconnect -> 7 |> ignore
        | OpCode.RequestGuildMembers -> 8 |> ignore
        | OpCode.InvalidSession -> Reconnect tokenvalue true |> Async.RunSynchronously //9 |> ignore
        | OpCode.Hello ->
            let payload = ofJson<Payload> message
            let heartbeatInterval = payload.d.["heartbeat_interval"].Value<int>()
            heartbeat(heartbeatInterval) |> Async.Start
        | OpCode.HeartbeatACK -> 11 |> ignore
        | _ -> 0 |> ignore

    let Run (token : string) = 
        async {
            tokenvalue <- token

            do! socket.ConnectAsync(Uri(gatewayURI), CancellationToken.None) |> Async.AwaitTask
            
            let identification = IdentifyPacket(token, 0, 1)
            do! Send(identification) |> Async.Ignore

            let rec running = 
                async {
                    for i in 1..MAX_RECONNECTS do
                        while socket.State = WebSocketState.Open do
                            let! message =  WebSocket.receieveMessageUTF8 socket
                            if message <> null && message <> String.Empty then
                                parseMessage message
                        
                        do! Reconnect token false
                }
            do! running

            ()
        }

    // Test method that calls the Run function with the target websocket uri
    member this.connect (token : string) = Run token

    [<CLIEvent>]
    member this.GatewayEvent = gatewayEvent.Publish

    interface IDisposable with
        member this.Dispose() = 
            socket.Dispose()