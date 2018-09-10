module BrokenDiscord.Packets

open BrokenDiscord.Types
open BrokenDiscord.Json.Json

open Newtonsoft.Json.Linq

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

// OP = opcode 
// d = event data
// s = sequence number
// t = event name
type Payload = {op : OpCode; d : JObject; s : int option; t : string option}

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