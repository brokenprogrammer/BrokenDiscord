namespace BrokenDiscord.WebSocket

open System.Net.WebSockets

module WebSocket =
    open System
    open System.Threading

    /// Size of the buffer when sending messages over the socket.
    type BufferSize = int

    /// (16 * 1024) = 16384
    /// https://referencesource.microsoft.com/#System/net/System/Net/WebSockets/WebSocketHelpers.cs,285b8b64a4da6851
    [<Literal>]
    let defaultBufferSize : BufferSize = 16384

    let receive (buffer : ArraySegment<byte>) (socket : ClientWebSocket) = 
        async {
            let! result = socket.ReceiveAsync(buffer, CancellationToken.None) |> Async.AwaitTask
            return result
        }

    let send (buffer : ArraySegment<byte>) messageType endOfMessage (socket : ClientWebSocket) =
        async {
            do! socket.SendAsync(buffer, messageType, endOfMessage, CancellationToken.None) |> Async.AwaitTask
        }

    let close = 0 // TODO: Implement
    
    let sendMessage bufferSize messageType (stream : IO.Stream) (socket : ClientWebSocket) = 
        async {
            //TODO: Implement
            return ()
        }

    let sendMessageUTF8 (text : string) (socket : ClientWebSocket) = 
        async {
            let stream = new IO.MemoryStream(Text.Encoding.UTF8.GetBytes text)
            do! sendMessage defaultBufferSize WebSocketMessageType.Text stream socket
        }

    let receieveMessage cancellationToken bufferSize (writeableStream : IO.Stream) (socket : ClientWebSocket) =
        async {
            let buffer = new ArraySegment<Byte>( Array.create (bufferSize) Byte.MinValue)
            
            let rec recieveTilEnd' () =
                async {
                    let! result = socket.ReceiveAsync(buffer, cancellationToken) |> Async.AwaitTask
                    
                    match result with
                    | result when result.MessageType = WebSocketMessageType.Close || socket.State = WebSocketState.CloseReceived ->
                        do! socket.CloseOutputAsync (WebSocketCloseStatus.NormalClosure, "Close Received", cancellationToken) |> Async.AwaitTask
                    | result ->
                        if result.MessageType <> WebSocketMessageType.Text then return ()
                        
                        do! writeableStream.AsyncWrite(buffer.Array,buffer.Offset,result.Count)
                        
                        if result.EndOfMessage then
                            return ()
                        else return! recieveTilEnd' ()

                }
            do! recieveTilEnd' ()
        }

    let receieveMessageUTF8 = //TODO: Implement
