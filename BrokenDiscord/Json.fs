namespace BrokenDiscord.Json

module Json =
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq

    /// An alias for the Fable JsonConverter which overrides the
    /// standard way of converting an object to and from JSON to a
    /// more accurate and easy way to share F# to server side JSON.
    let jsonConverter = 
        Fable.JsonConverter() :> JsonConverter
    
    /// Serializes an F# objet into JSON.
    let toJson value = 
        JsonConvert.SerializeObject(value, [|jsonConverter|])
    
    /// Deserializes an JSON string into an F# object.
    let ofJson<'T> value = 
        JsonConvert.DeserializeObject<'T>(value, [|jsonConverter|])
    
    /// Deserializes a part of a bigger JObject into an F# object.
    let ofJsonPart<'T> value (source : JObject) = 
        ofJson<'T> (source.[value].ToString())
    
    /// Deserializes a value from a JObject into specified F# type.
    let ofJsonValue<'T> value (source : JObject) = 
        (source.[value].Value<'T>())

