// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open BrokenDiscord.Client
open BrokenDiscord.Gateway
open System

[<EntryPoint>]
let main argv = 
    let c = new Client(OnReady = 
        (fun args -> printf "%s" "\n\n This is the client responding to the OnReady Event \n\n"))
    c.login()
    Console.ReadLine() |> ignore
    0 // return an integer exit code