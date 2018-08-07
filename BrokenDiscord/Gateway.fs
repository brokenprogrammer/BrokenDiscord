module BrokenDiscord.Gateway

open System
open System.Text
open System.Threading
open System.Threading.Tasks
open System.Net.WebSockets
open Newtonsoft.Json.Linq
open Newtonsoft.Json
open System.IO

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


type ISerializable =
    abstract member Serialize : unit -> string

type HeartbeatPacket (seq : int) =
    member this.op = OpCode.Heartbeat
    member this.seq = seq

    interface ISerializable with
        member this.Serialize() =
            JsonConvert.SerializeObject(this)

type IdentifyPacket (token : string, shard : int, numshards : int) =
    //TODO: Better way to construct the properties.
    let getProperties = 
        new JObject(new JProperty("$os", "linux"), new JProperty("$browser", "brokendiscord"), new JProperty("$device", "brokendiscord"))
    
    member this.token = token
    member this.properties = getProperties
    member this.compress = true
    member this.large_threshold = 250
    member this.shard =  [|shard; numshards|]

    interface ISerializable with
        member this.Serialize() =
            JsonConvert.SerializeObject(this)

type Gateway () =
    let socket : ClientWebSocket = new ClientWebSocket()

    let rec heartbeat (interval : int) =
        async {
            do! interval |> Task.Delay |> Async.AwaitTask |> Async.Ignore
            // TODO: Send heartbeat message
            do! heartbeat interval
        }

    let Receive () = 
        async {
            let buffer : byte[] = Array.zeroCreate 4096
            do! socket.ReceiveAsync(ArraySegment<byte>(buffer), CancellationToken.None) |> Async.AwaitTask |> Async.Ignore
            let content = Encoding.UTF8.GetString buffer
            printf "%s" content

            //TODO: Retrieve hello packet, construct heartbeat packet using it and initiate sending heartbeats

            //TODO: Send identification packet

            // Bellow is test code.
            let obj = JObject.Parse(content)
            let d = obj.GetValue("d")
            printf "%d" (d.["heartbeat_interval"].Value<int>())

            let hert = new HeartbeatPacket(d.["heartbeat_interval"].Value<int>())
            printf "%s" ((hert :> ISerializable).Serialize())

            let exIdent = new IdentifyPacket("my_token", 1, 10)
            printf "%s" ((exIdent :> ISerializable).Serialize())
        }
    
    let Send (message : string) = 
        async {
            let buffer = Encoding.UTF8.GetBytes(message)
            
            do! socket.SendAsync(ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None) |> Async.AwaitTask
        }

    let Run (uri : string) = 
        async {
            printf "%s" "Connecting..."
            
            do! socket.ConnectAsync(Uri(uri), CancellationToken.None) |> Async.AwaitTask
            do! Async.Parallel([Receive()]) |> Async.Ignore
        }

    // Test method that calls the Run function with the target websocket uri
    member this.con() = Run "wss://gateway.discord.gg/?v=6&encoding=json"

    interface IDisposable with
        member this.Dispose() = 
            socket.Dispose()