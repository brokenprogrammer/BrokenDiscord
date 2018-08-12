module BrokenDiscord.Types


//TODO: Make use of this object, Every object sent and receieved should be wrapped in the payload type
// OP = opcode 
// d = event data
// s = sequence number
// t = event name
type Payload = {op:int; d:string; s:int; t:string}

//TODO: Implement these types
type Channel = int
type Snowflake = int
type Guild = int
type GuildMember = int
type User = int
type Role = int
type Emoji = int
type Message = int
type Activity = int
type VoiceState = int