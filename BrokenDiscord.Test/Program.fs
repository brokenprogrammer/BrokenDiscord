// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open BrokenDiscord.Gateway
open System

[<EntryPoint>]
let main argv = 
    Gateway().con() |> Async.RunSynchronously
    printfn "%A" argv
    Console.ReadLine() |> ignore
    0 // return an integer exit code