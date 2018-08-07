module BrokenDiscord.Gateway

open System
open System.Text
open System.Threading
open System.Threading.Tasks
open System.Net.WebSockets
open Newtonsoft.Json.Linq

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
    let socket : ClientWebSocket = new ClientWebSocket()

    let Receive () = 
        async {
            let buffer : byte[] = Array.zeroCreate 4096
            do! socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None) |> Async.AwaitTask |> Async.Ignore
            let content = Encoding.UTF8.GetString buffer
            printf "%s" content
        }

    let Run (uri : string) = 
        async {
            printf "%s" "Connecting..."
            
            do! socket.ConnectAsync(new Uri(uri), CancellationToken.None) |> Async.AwaitTask
            do! Async.Parallel([Receive()]) |> Async.Ignore
        }

    // Test method that calls the Run function with the target websocket uri
    member this.con() = Run "wss://gateway.discord.gg/?v=6&encoding=json"