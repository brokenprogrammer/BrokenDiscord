module BrokenDiscord.Events

open Types

type ReadyEventArgs(readyEvent : Payload) = 
    inherit System.EventArgs()

    member this.ReadyEvent = readyEvent