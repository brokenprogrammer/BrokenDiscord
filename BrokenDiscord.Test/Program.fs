// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open BrokenDiscord.Gateway
open System

[<EntryPoint>]
let main argv = 
    use g = new Gateway()
    g.con() |> Async.RunSynchronously
    Console.ReadLine() |> ignore
    0 // return an integer exit code