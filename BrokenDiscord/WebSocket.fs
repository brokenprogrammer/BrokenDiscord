namespace BrokenDiscord.WebSockets

open System.Net.WebSockets

module WebSocket =
    open System
    open System.Threading
    open System.IO

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

    let close status message (socket : ClientWebSocket) =
        async {
            do! socket.CloseAsync(status, message, CancellationToken.None) |> Async.AwaitTask
        }
    
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

    let sendMessageUTF8 (text : string) (socket : ClientWebSocket) = 
        async {
            let stream = new IO.MemoryStream(Text.Encoding.UTF8.GetBytes text)
            do! sendMessage defaultBufferSize WebSocketMessageType.Text stream socket
        }

    let receieveMessage cancellationToken bufferSize messageType (writeableStream : IO.Stream) (socket : ClientWebSocket) =
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

    let receieveMessageUTF8 (socket : ClientWebSocket) = 
        async {
            let stream = new IO.MemoryStream()
            do! receieveMessage CancellationToken.None defaultBufferSize WebSocketMessageType.Text stream socket
            return
                stream.ToArray()
                |> Text.Encoding.UTF8.GetString
                |> fun s -> s.TrimEnd(char 0)
        }
