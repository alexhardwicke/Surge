namespace Surge.Core.Transmission

open Surge.Core.Models

type TransmissionServerStats = { DownloadSpeed : int64
                                 UploadSpeed : int64 }

type TransmissionServerGet = { ServerVersion : int
                               DefaultDownloadLocation : string
                               SpaceRemaining : int64
                               MemoryUnits : ServerUnits
                               SizeUnits : ServerUnits
                               SpeedUnits : ServerUnits }

module internal Deserialise =
    open FSharp.Data.JsonExtensions
    open Surge.Core.Models
    open Surge.Core.Network
    open Surge.Core.Transmission
    open Surge.Core.Transmission.Files
    open System
    open System.Collections.Generic
    open FSharp.Data
        
    let DeserialiseStats (stats : string) : TransmissionServerStats =
        let stats' = (SessionStats.Parse stats).Arguments
        { DownloadSpeed = stats'.DownloadSpeed
          UploadSpeed = stats'.UploadSpeed }

    let DeserializeGet (get : string) : TransmissionServerGet =
        let get' = (SessionGet.Parse get)
        let args = get'.Arguments
        { ServerVersion = args.RpcVersion
          DefaultDownloadLocation = args.DownloadDir
          SpaceRemaining =
            match (args.FreeSpace, args.DownloadDirFreeSpace) with
            | (Some(value), None) | (None, Some(value)) -> value
            | _ -> (int64)0
          MemoryUnits =
            { Bytes = args.Units.MemoryBytes
              Units = args.Units.MemoryUnits }
          SizeUnits =
            { Bytes = args.Units.SizeBytes
              Units = args.Units.SizeUnits }
          SpeedUnits =
            { Bytes = args.Units.SpeedBytes
              Units = args.Units.SpeedUnits }
        }

    let GenerateServer (stats : TransmissionServerStats) (get : TransmissionServerGet) =
        { DownloadSpeed = stats.DownloadSpeed
          UploadSpeed = stats.UploadSpeed
          ServerVersion = get.ServerVersion
          DefaultDownloadLocation = get.DefaultDownloadLocation
          SpaceRemaining = get.SpaceRemaining
          MemoryUnits = get.MemoryUnits
          SizeUnits = get.SizeUnits
          SpeedUnits = get.SpeedUnits
        }

    let TorrentListToDictionary seq =
        let dictionary = Dictionary<int, Torrent>()
        seq |> Seq.iter(fun t -> dictionary.Add(t.ID, t))
        dictionary

    let DeserialiseTorrents torrentData torrentDataTracker =
        let jsonDeserialise (torrentData : string) =
            (TorrentGet.Parse torrentData).Arguments.Torrents

        let unixTime = int (DateTime.UtcNow - DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds
        
        let generateTorrent(jsonTorrent : TorrentGet.Torrent) =
            let generateFiles = match Seq.length torrentDataTracker with
                                | 0 -> true
                                | _ ->
                                    let result = torrentDataTracker |> Seq.filter (fun e -> e.Id = jsonTorrent.Id)
                                    match Seq.isEmpty result with
                                    | true -> true
                                    | false -> not (Seq.head result).HasFiles

            let files =
                Array.zip jsonTorrent.Files jsonTorrent.FileStats
                |>  deserialiseFiles jsonTorrent.Id generateFiles

            // Should require queuePosition in the future
            let queuePosition =
                match jsonTorrent.QueuePosition with
                | Some(value) -> value
                | None -> jsonTorrent.Id

            { Files = files
              ID = jsonTorrent.Id
              IsFinished = jsonTorrent.IsFinished
              IsPaused = (jsonTorrent.IsFinished || jsonTorrent.Status = 0)
              IsVerifying = (jsonTorrent.Status = 1 || jsonTorrent.Status = 2)
              IsMagnetResolving = jsonTorrent.MetadataPercentComplete < 1.0M
              Comment = jsonTorrent.Comment
              Error = jsonTorrent.ErrorString.JsonValue.ToString()
              Hash = jsonTorrent.HashString
              Name = jsonTorrent.Name
              Location = jsonTorrent.DownloadDir
              MagnetResolvedPercent = float jsonTorrent.MetadataPercentComplete
              VerifiedPercent = float jsonTorrent.RecheckProgress
              Percent = float jsonTorrent.PercentDone
              Ratio = if jsonTorrent.UploadRatio > 0.0M then float jsonTorrent.UploadRatio else 0.0
              Availability = jsonTorrent.DesiredAvailable + jsonTorrent.HaveValid + jsonTorrent.HaveUnchecked
              Desired = jsonTorrent.DesiredAvailable
              Downloaded = jsonTorrent.HaveValid + jsonTorrent.HaveUnchecked
              DownloadSpeed = jsonTorrent.RateDownload
              QueuePosition = queuePosition
              Size = jsonTorrent.SizeWhenDone
              Uploaded = jsonTorrent.UploadedEver
              UploadSpeed = jsonTorrent.RateUpload
              LastActivity = unixTime - jsonTorrent.ActivityDate
              RemainingTime = jsonTorrent.Eta
              RunningTime = jsonTorrent.SecondsDownloading + jsonTorrent.SecondsSeeding }
              
        torrentData
        |> jsonDeserialise 
        |> Array.toList
        |> List.map generateTorrent