namespace Surge.Core.Common

/// This is used to generate the correct torrent client for the server type
module Initialiser =
    open Surge.Core
    open Surge.Core.Network
    open Surge.Core.Transmission.Implementation
    open System

    let private emptyAction<'t> : Action<'t> =
        new Action<'t>(fun x -> ())

    let private delugeClient server onError onException =
        failwith "Not Implemented"

    let private transmissionClient server onError onException =
        let torrentClient = new TorrentNetworkClient(server, "transmission/rpc", "X-Transmission-Session-Id", onException)
        new TransmissionClient(server, torrentClient, onError, onException)

    let private uTorrentClient server onError onException =
        failwith "Not Implemented"

    let private genServer (server : Server) onError onException : IInternalTorrentClient =
        match server.ServerType with
        | Deluge -> delugeClient server onError onException
        | Transmission -> upcast transmissionClient server onError onException
        | UTorrent -> uTorrentClient server onError onException

    let GetTestTorrentClient (server : Server) =
        new PublicTorrentClient((genServer server), emptyAction<UpdateData>, emptyAction<InternalError>, emptyAction<Exception>)

    /// Returns an initialised torrent client for the passed server object
    let GetTorrentClient (server : Server, fnUpdate, fnError, fnException) =
        new PublicTorrentClient((genServer server), fnUpdate, fnError, fnException)