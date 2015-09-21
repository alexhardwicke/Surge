namespace Surge.Core

open Surge.Core.Network
open System.Threading
open System
open Surge.Core.Models

type PublicTorrentClient internal (generateClient, fnUpdate : Action<UpdateData>, fnError : Action<InternalError>, fnException : Action<Exception>) =
    let context = SynchronizationContext.Current

    let sendData (func : Action<'t>) args = async {
        do! Async.SwitchToContext(context)
        func.Invoke args
        do! Async.SwitchToThreadPool() }

    let sendUpdate args = async { do! sendData fnUpdate args }
    let sendError args = async { do! sendData fnError args }
    let sendException args = async { do! sendData fnException args }

    let implementation : IInternalTorrentClient = generateClient sendError sendException

    let rec getUpdate token torrentData = async {
        try
            let! internalData = implementation.GetUpdateAsync torrentData
            match internalData with
            | None ->
                do! Async.Sleep 2000
                do! getUpdate token torrentData
            | Some data ->
                do! sendUpdate data.UpdateData
                do! Async.Sleep 2000
                do! getUpdate token data.TorrentData
        with
        | ex ->
            do! sendException ex
            do! Async.Sleep 2000
            do! getUpdate token torrentData }

    member this.StartUpdate() =
        let cancelTokenSource = new CancellationTokenSource()
        Async.Start <| getUpdate cancelTokenSource []
        cancelTokenSource

    /// Pass a state type and item list to update the passed items
    member this.ChangeState state torrents =
        let list = Seq.toList torrents
        implementation.ChangeState state list

    /// Pass a base64 encoded torrent file's contents to have the client add it
    member this.AddFile file location =
        implementation.AddFile file location

    /// Pass a url to a torrent file or a magnet link to have the client add it
    member this.AddURL url location =
        implementation.AddURL url location

    /// Pass a torrent id, file id and priority value to change the file's priority
    member this.ChangeFilePriority torrent files priority =
        let list = Seq.toList files
        implementation.ChangeFilePriority torrent list priority

    /// Pass a torrent id, file id and wanted boolean value to change the file's wanted status
    member this.ChangeFileWanted torrent files wanted =
        let list = Seq.toList files
        implementation.ChangeFileWanted torrent list wanted

    /// Pass a torrent id, file id and priority value to change the file's priority
    member this.Delete torrents keepFiles =
        let list = Seq.toList torrents
        implementation.Delete list keepFiles

    /// Move the provided torrents to the provided location
    member this.ChangeLocation torrents location moveFiles =
        let list = Seq.toList torrents
        implementation.ChangeLocation list location moveFiles

    /// Check if you have a working connection to the server
    member this.CheckConnectionAsync() =
        implementation.CheckConnectionAsync() |> Async.StartAsTask

    interface IDisposable with
        member this.Dispose() =
            (implementation :> IDisposable).Dispose()

and internal IInternalTorrentClient =
    abstract AddFile: string -> string -> unit
    abstract AddURL: string -> string -> unit
    abstract ChangeFilePriority: int -> int list -> int -> unit
    abstract ChangeFileWanted: int -> int list -> bool -> unit
    abstract ChangeLocation: int list -> string -> bool -> unit
    abstract ChangeState: StateType -> int list -> unit
    abstract CheckConnectionAsync: unit -> Async<ServerState>
    abstract Delete: int list -> bool -> unit
    abstract GetUpdateAsync: TorrentDataTracker -> Async<InternalUpdate option>
    inherit IDisposable
