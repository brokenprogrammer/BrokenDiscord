namespace BrokenDiscord.Json

module Json =
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq

    ///
    let jsonConverter = 
        Fable.JsonConverter() :> JsonConverter
    
    ///
    let toJson value = 
        JsonConvert.SerializeObject(value, [|jsonConverter|])
    
    ///
    let ofJson<'T> value = 
        JsonConvert.DeserializeObject<'T>(value, [|jsonConverter|])
    
    ///
    let ofJsonPart<'T> value (source : JObject) = 
        ofJson<'T> (source.[value].ToString())
    
    ///
    let ofJsonValue<'T> value (source : JObject) = 
        (source.[value].Value<'T>())

