module BrokenDiscord.Gateway

open System
open System.Text
open System.Threading
open System.Threading.Tasks
open System.Net.WebSockets
open Newtonsoft.Json.Linq
open Newtonsoft.Json
open System.IO

//TODO: Make use of this object, Every object sent and receieved should be wrapped in the payload type
// OP = opcode 
// d = event data
// s = sequence number
// t = event name
type Payload = {op:int; d:string;}// s:int; t:string}

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
    member this.seq = seq

    interface ISerializable with
        member this.Serialize() =
            let j = 
                new JObject(
                    new JProperty("op", 1), 
                    new JProperty("d", JObject.FromObject(this)))
            j.ToString()

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
            let j = 
                new JObject(
                    new JProperty("op", 2), 
                    new JProperty("d", JObject.FromObject(this)))
            j.ToString()

type Gateway () =
    let socket : ClientWebSocket = new ClientWebSocket()

    let Send (packet : ISerializable) = 
        async {
            let buffer = Encoding.UTF8.GetBytes(packet.Serialize())
            
            printf "%s" ("Sending packet" + packet.Serialize())

            do! socket.SendAsync(ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None) |> Async.AwaitTask
            printf "%s" "Sent packet...\n"
        }

    let rec heartbeat (interval : int) =
        async {
            do! interval |> Task.Delay |> Async.AwaitTask |> Async.Ignore
            do! Send(HeartbeatPacket(interval)) |> Async.Ignore
            printf "%s" "Sent Heartbeat packet\n"
            do! heartbeat interval
        }

    //TODO: better naming for this function
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
            printf "%s" "Receieved Hello opcode, starting heartbeater...\n"
            let heartbeatInterval = rawJson.["d"].["heartbeat_interval"].Value<int>()
            heartbeat(heartbeatInterval) |> Async.Start
        | OpCode.heartbeatACK -> 11 |> ignore
        | _ -> 0 |> ignore

    let Receive () = 
        async {
            printf "%s" "Starting to recieve"
            let buffer : byte[] = Array.zeroCreate 4096
            let! result = socket.ReceiveAsync(ArraySegment<byte>(buffer), CancellationToken.None) |> Async.AwaitTask
            
            let content = 
                match result.Count with
                | 0 -> ""
                | _ -> buffer.[0..result.Count] |> Encoding.UTF8.GetString


            printf "%s" content

            //TODO: Retrieve hello packet, construct heartbeat packet using it and initiate sending heartbeats
            if content <> "" || content = null then
                parseMessage(JObject.Parse(content))
            printf "%s" "finished receive\n"
            
            // Bellow is test code.
            //let obj = JObject.Parse(content)
            //let d = obj.GetValue("d")
            //printf "%d" (d.["heartbeat_interval"].Value<int>())

            //let hert = new HeartbeatPacket(d.["heartbeat_interval"].Value<int>())
            //printf "%s" ((hert :> ISerializable).Serialize())

            //let exIdent = new IdentifyPacket("my_token", 1, 10)
            //printf "%s" ((exIdent :> ISerializable).Serialize())
        }

    let Run (uri : string) (token : string) = 
        async {
            printf "%s" "Connecting...\n"
            
            do! socket.ConnectAsync(Uri(uri), CancellationToken.None) |> Async.AwaitTask
            
            //TODO: Send identification packet
            let identification = IdentifyPacket(token, 1, 10)
            //do! Receive() |> Async.Ignore
            do! Send(identification) |> Async.Ignore

            while socket.State = WebSocketState.Open do
                //TODO: Recieve should return something instead of the logic that is currently present in the recieve function
                do! Receive() |> Async.Ignore
                printf "%s" "End of run loop \n"
            
            printf "%s" (socket.State.ToString())
        }

    // Test method that calls the Run function with the target websocket uri
    member this.con() = Run "wss://gateway.discord.gg/?v=6&encoding=json" "NDc2NzQyMjI4NTg1MzQ5MTQy.DkyAlw.t9qBUy5MEfFGoHlIFYacVXIKxL4"

    interface IDisposable with
        member this.Dispose() = 
            socket.Dispose()