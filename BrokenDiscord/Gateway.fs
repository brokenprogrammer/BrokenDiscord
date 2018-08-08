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

type ISerializable =
    abstract member Serialize : unit -> string

type HeartbeatPacket (seq : int) =
    member this.op = OpCode.heartbeat
    member this.seq = seq

    interface ISerializable with
        member this.Serialize() =
            JsonConvert.SerializeObject(this)

type IdentifyPacket (token : string, shard : int, numshards : int) =
    //TODO: Better way to construct the properties.
    let getProperties = 
        new JObject(new JProperty("$os", "linux"), 
            new JProperty("$browser", "brokendiscord"), 
            new JProperty("$device", "brokendiscord"))
    
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
            // TODO: Send heartbeat packet into the send function
            printf "%s" "Sent Heartbeat packet"
            do! heartbeat interval
        }

    let parseMessage (rawJson : JObject) =
        let op = enum<OpCode>(rawJson.["op"].Value<int>())
        
        match op with
        | OpCode.dispatch -> 0 |> ignore
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
            printf "%s" "Receieved Hello opcode, starting heartbeater..."
            let heartbeatInterval = rawJson.["d"].["heartbeat_interval"].Value<int>()
            heartbeat(heartbeatInterval) |> Async.Start
        | OpCode.heartbeatACK -> 11 |> ignore
        | _ -> 0 |> ignore

    let Receive () = 
        async {
            let buffer : byte[] = Array.zeroCreate 4096
            do! socket.ReceiveAsync(ArraySegment<byte>(buffer), CancellationToken.None) |> Async.AwaitTask |> Async.Ignore
            let content = Encoding.UTF8.GetString buffer
            printf "%s" content

            //TODO: Retrieve hello packet, construct heartbeat packet using it and initiate sending heartbeats

            //TODO: Send identification packet

            parseMessage(JObject.Parse(content))
            printf "%s" "finhed parsing"
            
            // Bellow is test code.
            //let obj = JObject.Parse(content)
            //let d = obj.GetValue("d")
            //printf "%d" (d.["heartbeat_interval"].Value<int>())

            //let hert = new HeartbeatPacket(d.["heartbeat_interval"].Value<int>())
            //printf "%s" ((hert :> ISerializable).Serialize())

            //let exIdent = new IdentifyPacket("my_token", 1, 10)
            //printf "%s" ((exIdent :> ISerializable).Serialize())
        }
    
    //TODO: Change into taking in a packet type instead of string
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

            // TODO: Some loop that continues listening to messages.
        }

    // Test method that calls the Run function with the target websocket uri
    member this.con() = Run "wss://gateway.discord.gg/?v=6&encoding=json"

    interface IDisposable with
        member this.Dispose() = 
            socket.Dispose()