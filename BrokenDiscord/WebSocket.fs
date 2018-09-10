module BrokenDiscord.WebSockets.WebSocket

open System
open System.IO
open System.Net.WebSockets
open System.Threading
open FSharpPlus
open Hopac 
open Hopac.Infixes

/// Size of the buffer when sending messages over the socket.
type BufferSize = int

/// (16 * 1024) = 16384
/// https://referencesource.microsoft.com/#System/net/System/Net/WebSockets/WebSocketHelpers.cs,285b8b64a4da6851
[<Literal>]
let defaultBufferSize : BufferSize = 16384

/// Wrapper for the RecieveAsync function, takes a buffer and target socket.
let receive (buffer : ArraySegment<byte>) (socket : ClientWebSocket) = 
    Job.awaitTask <| socket.ReceiveAsync(buffer, CancellationToken.None)

/// Wrapper for the SendAsync function, takes a buffer, message type and endOfMessage and target socket.
let send buffer messageType endOfMessage (socket : ClientWebSocket) =
    Job.awaitUnitTask <| socket.SendAsync(buffer, messageType, endOfMessage, CancellationToken.None)

/// Gracefull approach to closing the socket. This tells the other end that the socket is being closed.
let close status message (socket : ClientWebSocket) =
    Job.awaitUnitTask <| socket.CloseAsync(status, message, CancellationToken.None)

/// Sends a message to the specified socket from the specified stream.
let sendMessage bufferSize messageType (stream : IO.Stream) (socket : ClientWebSocket) = 
    job {
        let buffer = Array.create (bufferSize) Byte.MinValue

        let rec sendMessage' () =
            job {
                let! read = stream.ReadAsync(buffer, 0, buffer.Length) |> Async.AwaitTask
                if read > 0 then
                    do! socket |> send (ArraySegment(buffer |> Array.take read)) messageType false
                    return! sendMessage' ()
                else
                    do! send (ArraySegment(Array.empty)) messageType true socket
            }
        do! sendMessage'()
    }

/// Sends a specified UTF8 string as the message for specified socket.
let sendMessageUTF8 (text : string) (socket : ClientWebSocket) =
    new IO.MemoryStream(Text.Encoding.UTF8.GetBytes text) 
    |> sendMessage text.Length WebSocketMessageType.Text /> socket

/// Receives a message and writes it to the specified stream
/// Attempts to handle closes gracefully; returns whether the socket was closed
let receiveMessage cancellationToken bufferSize messageType (toStream : IO.Stream) (socket : ClientWebSocket) =
    job {
        let buffer = new ArraySegment<_>(Array.create (bufferSize) Byte.MinValue)
    
        let rec recvToEnd () =
            job {
                let! result = Job.awaitTask <| socket.ReceiveAsync(buffer, cancellationToken)
                match result with
                | result when result.MessageType = WebSocketMessageType.Close || socket.State = WebSocketState.CloseReceived ->
                    do! Job.awaitUnitTask <| socket.CloseOutputAsync (WebSocketCloseStatus.NormalClosure, "Close Received", cancellationToken)
                    return true
                | result ->
                    if result.MessageType <> messageType then return ()
                    
                    do! Job.fromAsync <| toStream.AsyncWrite(buffer.Array,buffer.Offset,result.Count)
                    if result.EndOfMessage then
                        return false
                    else return! recvToEnd ()

            }
        return! recvToEnd ()
    }

 /// Receives a message as an UTF8 string from specified socket.
let receiveMessageUTF8 (socket : ClientWebSocket) = 
    job {
        let stream = new IO.MemoryStream()
        let! closed =
            receiveMessage
            <| CancellationToken.None
            <| defaultBufferSize
            <| WebSocketMessageType.Text
            <| stream <| socket
        return
            stream.ToArray()
            |> Text.Encoding.UTF8.GetString
            |> fun s -> s.TrimEnd(char 0)
    }
