namespace Surge.Core.Transmission

module internal Implementation =
    open Surge.Core.Models
    open Surge.Core.Network
    open Surge.Core.Transmission.Deserialise
    open Surge.Core.Transmission.Generators
    open System
    open Surge.Core
    open System.Collections.Generic

    type TransmissionClient(server : Server, torrentClient : TorrentNetworkClient, onError, onException) =
        let minSupportedServerVersion = 14
        let maxSupportedServerVersion = 15

        let generateTorrentData n (u : Dictionary<int, Torrent>) (m : Dictionary<int, Torrent>) =
            let n' = Seq.map(fun e -> { Id = e.ID; HasFiles = not (e.Files |> Seq.length = 0)}) n
            
            let u' =
                (List.ofSeq u)
                |> Seq.map(fun e -> { Id = e.Key; HasFiles = true})

            let m' =
                (List.ofSeq m)
                |> Seq.map(fun e -> { Id = e.Key; HasFiles = Seq.isEmpty e.Value.Files |> not})

            (List.ofSeq m)
            |> List.iter(fun e -> u.Add(e.Key, e.Value))
            
            Seq.concat [n'; u'; m']

        let processTorrentData speedBytes sizeBytes (torrentData : TorrentDataTracker) torrentResult =
            let torrents = DeserialiseTorrents(torrentResult) torrentData speedBytes sizeBytes
            let currentIDs = Set.ofSeq(query { for torrent in torrentData do select torrent.Id })
            let serverIDs = Set.ofSeq(query { for torrent in torrents do select torrent.ID})

            let deletedTorrents = currentIDs - serverIDs

            let updatedIDs = Set.intersect (Set.ofSeq(query {
                    for torrent in torrentData do
                    where torrent.HasFiles
                    select torrent.Id })) serverIDs

            let magnetIDs = Set.intersect (Set.ofSeq(query {
                    for torrent in torrentData do
                    where (not torrent.HasFiles)
                    select torrent.Id })) serverIDs

            let newIDs = (serverIDs - updatedIDs) - magnetIDs

            let newTorrents =
                query { for torrent in torrents do
                        where (newIDs.Contains(torrent.ID))
                        select torrent }

            let updatedTorrents =
                query { for torrent in torrents do
                        where (updatedIDs.Contains(torrent.ID))
                        select torrent }
                |> TorrentListToDictionary

            let magnetTorrents =
                query { for torrent in torrents do
                        where (magnetIDs.Contains(torrent.ID))
                        select torrent }
                |> TorrentListToDictionary

            (newTorrents, updatedTorrents, magnetTorrents, deletedTorrents)

        let getFromServer data processResult = async {
            let! result = torrentClient.SendWithResultAsync processResult data
            match result with
            | Error(value) -> do! onError value
            | _ -> ()
            return result  }

        let getTorrentUpdate = getFromServer TorrentDetails
        let getStats = getFromServer SessionStats DeserialiseStats
        let getGet = getFromServer SessionGet DeserializeGet

        let getUpdate torrentData = async {
            try
                let! stat = getStats
                let! get = getGet

                match (stat, get) with
                | (Error e, _) | (_, Error e) ->
                    return None
                | (Success statData, Success getData) ->
                    let! torr = getTorrentUpdate (processTorrentData (int64 getData.SpeedUnits.Bytes) (int64 getData.SizeUnits.Bytes) torrentData)
                    match torr with
                    | Success (n, u, m, d) ->
                        let server = GenerateServer statData getData
                        if server.ServerVersion < minSupportedServerVersion || server.ServerVersion > maxSupportedServerVersion then
                            do! onError InternalError.Version
                            return None
                        else
                            let updateData = {
                                NewTorrents = n
                                UpdatedTorrents = u
                                DeletedTorrents = d
                                ServerStats = server }

                            let torrentData' = generateTorrentData n u m
                            return Some { UpdateData = updateData ; TorrentData = torrentData' }
                    | Error e ->
                        return None
            with
            | ex ->
                if ex.Message.StartsWith "Invalid JSON" then
                    do! onError NonJSON
                else
                    do! onException ex
                return None }
                             
        interface IInternalTorrentClient with
            member this.Dispose() = 
                (torrentClient :> IDisposable).Dispose()
            
            member this.GetUpdateAsync torrentData = async {
                    return! getUpdate torrentData }
            
            member this.ChangeLocation items location moveFiles = async {
                do! ChangeLocation items location moveFiles |> torrentClient.SendNoResultAsync } |> Async.Start
            
            member this.CheckConnectionAsync() = async {
                let! result = torrentClient.SendWithResultAsync (fun x -> x) SessionStats
                return ServerState.FromResponse result }
            
            member this.AddFile file location = async {
                do! AddTorrent file location false |> torrentClient.SendNoResultAsync }  |> Async.Start
            
            member this.AddURL url location = async {
                do! AddTorrent url location true |> torrentClient.SendNoResultAsync } |> Async.Start

            member this.ChangeFilePriority torrentid files priority = async {
                do! ChangeFilePriority torrentid files priority |> torrentClient.SendNoResultAsync } |> Async.Start

            member this.ChangeFileWanted torrentid files wanted = async {
                do! ChangeFileWanted torrentid files wanted |> torrentClient.SendNoResultAsync } |> Async.Start

            member this.Delete torrents keepFiles = async {
                do! RemoveTorrents torrents keepFiles |> torrentClient.SendNoResultAsync } |> Async.Start

            member this.ChangeState state torrents = async {
                do! ChangeTorrentState torrents state |> torrentClient.SendNoResultAsync } |> Async.Start
