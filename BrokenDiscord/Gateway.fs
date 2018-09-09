module BrokenDiscord.Gateway

open System
open System.Net.WebSockets
open System.Threading
open System.Threading.Tasks

open Newtonsoft.Json.Linq

open BrokenDiscord.Events
open BrokenDiscord.Types
open BrokenDiscord.Packets
open BrokenDiscord.Json
open BrokenDiscord.Json.Json
open BrokenDiscord.WebSockets
open BrokenDiscord.WebSockets.WebSocket
    
module Gateway =
    let mutable private socket : ClientWebSocket = new ClientWebSocket()
    let mutable private tokenvalue = ""
    let mutable private session_id = ""
    let mutable private seq = -1

    [<Literal>]
    let private MAX_RECONNECTS = 6
    
    [<Literal>]
    let private GatewayURI = "wss://gateway.discord.gg/?v=6&encoding=json"
    
    let private GatewayEvent = new Event<GatewayEvents>()
    
    [<CLIEvent>]
    let  gatewayEvent = GatewayEvent.Publish

    let send (packet : ISerializable) = 
        async {
            do! WebSocket.sendMessageUTF8 (packet.Serialize()) socket
        }

    let rec heartbeat (interval : int) =
        async {
            do! interval |> Task.Delay |> Async.AwaitTask |> Async.Ignore
            do! send(HeartbeatPacket(interval)) |> Async.Ignore
            do! heartbeat interval
        }

    let handleDispatch (payload : Payload) =
        match payload.s with 
        | Some s -> seq <- s
        | _ -> ()
        
        let t = payload.t.Value
        if t = "READY" then
            session_id <- (ofJsonValue "session_id" payload.d)

        let payloadData = payload.d
        let payloadJson = payloadData.ToString()
        
        let trigger eventType =
            ofJson payloadJson |> eventType |> GatewayEvent.Trigger

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

    let reconnect (token : string) (identify : bool) =
        async {
            if socket.State = WebSocketState.Closed then
                socket.Dispose()
            else 
                do! WebSocket.close WebSocketCloseStatus.Empty "" socket
                socket.Dispose()

            socket <- new ClientWebSocket()
            do! socket.ConnectAsync(Uri(GatewayURI), CancellationToken.None) |> Async.AwaitTask

            let resume = 
                async {
                    let resume : ResumePacket = {token = token; session_id = session_id; seq = seq}
                    do! send(resume) |> Async.Ignore
                }

            let reidentify =
                async {
                    do! 5000 |> Task.Delay |> Async.AwaitTask |> Async.Ignore
                    let identification = IdentifyPacket(token, 0, 1)
                    do! send(identification) |> Async.Ignore
                }
                
            if identify then
                do! reidentify
            else
                do! resume
        }

    let parse (message : string) =
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
        | OpCode.InvalidSession -> reconnect tokenvalue true |> Async.RunSynchronously
        | OpCode.Hello ->
            let payload = ofJson<Payload> message
            let heartbeatInterval = payload.d.["heartbeat_interval"].Value<int>()
            heartbeat(heartbeatInterval) |> Async.Start
        | OpCode.HeartbeatACK -> 11 |> ignore
        | _ -> 0 |> ignore

    let run (token : string) = 
        async {
            tokenvalue <- token

            do! socket.ConnectAsync(Uri(GatewayURI), CancellationToken.None) |> Async.AwaitTask
            
            let identification = IdentifyPacket(token, 0, 1)
            do! send(identification) |> Async.Ignore
            
            for i in 1..MAX_RECONNECTS do
                while socket.State = WebSocketState.Open do
                    let! message =  WebSocket.receieveMessageUTF8 socket
                    if message <> null && message <> String.Empty then
                        parse message
                        
                do! reconnect token false
            
            socket.Dispose()
            ()
        }