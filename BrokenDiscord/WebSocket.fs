namespace BrokenDiscord.WebSockets

module WebSocket =
    open System
    open System.IO
    open System.Net.WebSockets
    open System.Threading

    /// Size of the buffer when sending messages over the socket.
    type BufferSize = int

    /// (16 * 1024) = 16384
    /// https://referencesource.microsoft.com/#System/net/System/Net/WebSockets/WebSocketHelpers.cs,285b8b64a4da6851
    [<Literal>]
    let defaultBufferSize : BufferSize = 16384

    /// Wrapper for the RecieveAsync function, takes a buffer and target socket.
    let receive (buffer : ArraySegment<byte>) (socket : ClientWebSocket) = 
        async {
            let! result = socket.ReceiveAsync(buffer, CancellationToken.None) |> Async.AwaitTask
            return result
        }
    
    /// Wrapper for the SendAsync function, takes a buffer, message type and endOfMessage and target socket.
    let send (buffer : ArraySegment<byte>) messageType endOfMessage (socket : ClientWebSocket) =
        async {
            do! socket.SendAsync(buffer, messageType, endOfMessage, CancellationToken.None) |> Async.AwaitTask
        }
    
    /// Gracefull approach to closing the socket. This tells the other end that the socket is being closed.
    let close status message (socket : ClientWebSocket) =
        async {
            do! socket.CloseAsync(status, message, CancellationToken.None) |> Async.AwaitTask
        }
    
    /// Sends a message to the specified socket from the specified stream.
    let sendMessage bufferSize messageType (stream : IO.Stream) (socket : ClientWebSocket) = 
        async {
            let buffer = Array.create (bufferSize) Byte.MinValue

            let rec sendMessage' () =
                async {
                    let! read = stream.ReadAsync(buffer, 0, buffer.Length) |> Async.AwaitTask
                    
                    if read > 0 then
                        do! socket |> send (ArraySegment(buffer |> Array.take read)) messageType false
                        return! sendMessage'()
                    else
                        do! socket |> send (ArraySegment(Array.empty)) messageType true
                }
            do! sendMessage'()
        }
    
    /// Sends a specified UTF8 string as the message for specified socket.
    let sendMessageUTF8 (text : string) (socket : ClientWebSocket) = 
        async {
            let stream = new IO.MemoryStream(Text.Encoding.UTF8.GetBytes text)
            do! sendMessage defaultBufferSize WebSocketMessageType.Text stream socket
        }
    
    /// Receives a message and writes it to the specified stream
    /// Attempts to handle closes gracefully
    let receiveMessage cancellationToken bufferSize messageType (writeableStream : IO.Stream) (socket : ClientWebSocket) =
        async {
            let buffer = new ArraySegment<Byte>(Array.create (bufferSize) Byte.MinValue)
            
            let rec recieveTilEnd' () =
                async {
                    let! result = socket.ReceiveAsync(buffer, cancellationToken) |> Async.AwaitTask
                    
                    match result with
                    | result when result.MessageType = WebSocketMessageType.Close || socket.State = WebSocketState.CloseReceived ->
                        do! socket.CloseOutputAsync (WebSocketCloseStatus.NormalClosure, "Close Received", cancellationToken) |> Async.AwaitTask
                    | result ->
                        if result.MessageType <> messageType then return ()
                        
                        do! writeableStream.AsyncWrite(buffer.Array,buffer.Offset,result.Count)
                        
                        if result.EndOfMessage then
                            return ()
                        else return! recieveTilEnd' ()

                }
            do! recieveTilEnd' ()
        }
    
     /// Receives a message as an UTF8 string from specified socket.
    let receieveMessageUTF8 (socket : ClientWebSocket) = 
        async {
            let stream = new IO.MemoryStream()
            do! receiveMessage CancellationToken.None defaultBufferSize WebSocketMessageType.Text stream socket
            return
                stream.ToArray()
                |> Text.Encoding.UTF8.GetString
                |> fun s -> s.TrimEnd(char 0)
        }
