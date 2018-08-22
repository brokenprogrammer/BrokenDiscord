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
    
    let mutable SessionId = 0

    member val GatewayVersion = 0 with get, set
    member val PrivateChannels = [] with get, set
    member val Guilds = [] with get,set

    member __.OnReady
        with set (handler) = 
            disposables.Add (gw.ReadyEvent.Subscribe (obs handler))
    
    member __.OnResumed
        with set (handler) =
            disposables.Add (gw.ResumedEvent.Subscribe (obs handler))
    
    member __.OnChannelCreated
        with set (handler) =
            disposables.Add (gw.ChannelCreatedEvent.Subscribe (obs handler))
    
    member __.OnChannelUpdated
        with set (handler) =
            disposables.Add (gw.ChannelUpdatedEvent.Subscribe (obs handler))
    
    member __.OnChannelDeleted
        with set (handler) =
            disposables.Add (gw.ChannelDeletedEvent.Subscribe (obs handler))

    member __.OnChannelPinsUpdated
        with set (handler) =
            disposables.Add (gw.ChannelPinsUpdatedEvent.Subscribe (obs handler))
    
    member __.OnGuildCreated
        with set (handler) =
            disposables.Add (gw.GuildCreatedEvent.Subscribe (obs handler))

    member __.OnGuildUpdated
        with set (handler) =
            disposables.Add (gw.GuildUpdatedEvent.Subscribe (obs handler))

    member __.OnGuildDeleted
        with set (handler) =
            disposables.Add (gw.GuildDeletedEvent.Subscribe (obs handler))
    
    member __.OnGuildBanAdd
        with set (handler) =
            disposables.Add (gw.GuildBanAddEvent.Subscribe (obs handler))

    member __.OnGuildBanRemove
        with set (handler) =
            disposables.Add (gw.GuildBanRemoveEvent.Subscribe (obs handler))

    member __.OnGuildEmojisUpdated
        with set (handler) =
            disposables.Add (gw.GuildEmojisUpdatedEvent.Subscribe (obs handler))

    member __.OnGuildIntegrationsUpdated
        with set (handler) =
            disposables.Add (gw.GuildIntegrationsUpdatedEvent.Subscribe (obs handler))

    member __.OnGuildMemberAdd
        with set (handler) =
            disposables.Add (gw.GuildMemberAddEvent.Subscribe (obs handler))

    member __.OnGuildMemberUpdate
        with set (handler) =
            disposables.Add (gw.GuildMemberUpdateEvent.Subscribe (obs handler))
    
    member __.OnGuildMemberRemove
        with set (handler) =
            disposables.Add (gw.GuildMemberRemoveEvent.Subscribe (obs handler))

    member __.OnGuildMembersChunk
        with set (handler) =
            disposables.Add (gw.GuildMembersChunkEvent.Subscribe (obs handler))

    member __.OnGuildRoleCreate
        with set (handler) =
            disposables.Add (gw.GuildRoleCreateEvent.Subscribe (obs handler))

    member __.OnGuildRoleUpdate
        with set (handler) =
            disposables.Add (gw.GuildRoleUpdateEvent.Subscribe (obs handler))
     
    member __.OnGuildRoleDelete
        with set (handler) =
            disposables.Add (gw.GuildRoleDeleteEvent.Subscribe (obs handler))

    member __.OnMessageCreate
        with set (handler) =
            disposables.Add (gw.MessageCreateEvent.Subscribe (obs handler))

    member __.OnMessageUpdate
        with set (handler) =
            disposables.Add (gw.MessageUpdateEvent.Subscribe (obs handler))

    member __.OnMessageDelete
        with set (handler) =
            disposables.Add (gw.MessageDeleteEvent.Subscribe (obs handler))

    member __.OnMessageDeleteBulk
        with set (handler) =
            disposables.Add (gw.MessageDeleteBulkEvent.Subscribe (obs handler))

    member __.OnMessageReactionAdd
        with set (handler) =
            disposables.Add (gw.MessageReactionAddEvent.Subscribe (obs handler))

    member __.OnMessageReactionRemove
        with set (handler) =
            disposables.Add (gw.MessageReactionRemoveEvent.Subscribe (obs handler))

    member __.OnMessageReactionCleared
        with set (handler) =
            disposables.Add (gw.MessageReactionClearedEvent.Subscribe (obs handler))

    member __.OnPresenceUpdate
        with set (handler) =
            disposables.Add (gw.PresenceUpdateEvent.Subscribe (obs handler))

    member __.OnTypingStart
        with set (handler) =
            disposables.Add (gw.TypingStartEvent.Subscribe (obs handler))

    member __.OnUserUpdate
        with set (handler) =
            disposables.Add (gw.UserUpdateEvent.Subscribe (obs handler))

    member __.OnVoiceStateUpdate
        with set (handler) =
            disposables.Add (gw.VoiceStateUpdateEvent.Subscribe (obs handler))

    member __.OnVoiceServerUpdate
        with set (handler) =
            disposables.Add (gw.VoiceServerUpdateEvent.Subscribe (obs handler))

    //TODO: Should take token in params.
    member this.login() = gw.con() |> Async.RunSynchronously

    interface System.IDisposable with
        member this.Dispose () = 
            for disposable in disposables do
                disposable.Dispose()
            (gw :> IDisposable).Dispose()