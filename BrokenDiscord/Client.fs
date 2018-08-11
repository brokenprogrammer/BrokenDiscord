module BrokenDiscord.Client

open BrokenDiscord.Gateway

open System

let obs handler = 
  { new System.IObserver<_> with 
      member __.OnError e = () 
      member __.OnNext x = handler x
      member __.OnCompleted () = () }

type Client () =
    let gw = new Gateway()
    
    let disposables = ResizeArray<_>()
    
    member __.OnReady
        with set (handler) = 
            disposables.Add (gw.ReadyEvent.Subscribe (obs handler))
    
    //TODO: Should take token in params.
    member this.login() = gw.con() |> Async.RunSynchronously

    interface System.IDisposable with
        member this.Dispose () = 
            for disposable in disposables do
                disposable.Dispose()
            (gw :> IDisposable).Dispose()