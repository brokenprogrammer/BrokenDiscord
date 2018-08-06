module BrokenDiscord.Gateway

open System
open System.Threading
open System.Threading.Tasks
open System.Net.WebSockets
open Microsoft.AspNetCore.Http
open Newtonsoft.Json.Linq
open System.Text

// OP = opcode 
// d = event data
// s = sequence number
// t = event name
type Payload = {op:int; d:JObject; s:int; t:string}

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

type Gateway () =
    let GATEWAY_VERSION = 6

    let mutable sockets = list<WebSocket>.Empty

    let addSocket sockets socket = socket :: sockets

    let removeSocket sockets socket =
        sockets
        |> List.choose (fun s -> if s <> socket then Some s else None)
    
    let sendMessage =
        fun (socket : WebSocket) (message : string) ->
            let buffer = Encoding.UTF8.GetBytes(message)
            let segment = new ArraySegment<byte>(buffer)

            if socket.State = WebSocketState.Open then
                do socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None) |> ignore
            else
                sockets <- removeSocket sockets socket

    member this.connect (context : HttpContext) =
        async {
            if context.Request.Path = PathString("wss://gateway.discord.gg/?v=6&encoding=json") then
                match context.WebSockets.IsWebSocketRequest with
                | true ->
                    let! websocket = context.WebSockets.AcceptWebSocketAsync() |> Async.AwaitTask
                    sockets <- addSocket sockets websocket

                    let buffer : byte[] = Array.zeroCreate 4096
                    let! ct = Async.CancellationToken
                    
                    websocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct)
                        |> Async.AwaitTask
                        |> ignore
                    
                    let content = Encoding.UTF8.GetString buffer
                    printf "%s" content


                | false -> context.Response.StatusCode <- 400
        } |> Async.StartAsTask :> Task