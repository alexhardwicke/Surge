namespace Surge.Core.Transmission

open System
open Surge.Core.Models

module internal Generators =
    let private addCommas list =
        let rec add list acc =
            match list with
            | [] -> acc
            | head::tail when tail |> List.isEmpty -> add list.Tail (acc + head.ToString())
            | head::tail -> add list.Tail (acc + head.ToString() + ",")

        add list ""

    let AddTorrent data location isURL =
        let addType = match isURL with
                      | true -> "filename"
                      | false -> "metainfo"
        "{ \"arguments\": {\"" + addType + "\": \"" + data + "\", \"download-dir\": \"" + location + "\"},"
        + "\"method\": \"torrent-add\"}"

    let ChangeFilePriority torrentid fileIDs priority=
        let priorityString = match priority with
                             | 0 -> "low"
                             | 1 -> "normal"
                             | 2 -> "high"
                             | _ -> failwith "invalid priority"
        let data = "\"priority-" + priorityString + "\":[" + addCommas fileIDs + "]"
        "{\"arguments\":{\"ids\":[" + torrentid.ToString() + "]," + data + "},\"method\":\"torrent-set\"}"
    
    let ChangeFileWanted torrentid fileIDs isWanted =
        let wantedString = match isWanted with
                           | true -> "wanted"
                           | false -> "unwanted"
        let data = "\"files-" + wantedString + "\":[" + addCommas fileIDs + "]"
        "{\"arguments\":{\"ids\":[" + torrentid.ToString() + "]," + data + "},\"method\":\"torrent-set\"}"

    let ChangeTorrentState torrentIDs stateType =
        let generateString stateType =
            match stateType with
            | ForceStart -> "torrent-start-now"
            | Start -> "torrent-start"
            | Stop -> "torrent-stop"
            | Verify -> "torrent-verify"
        "{\"arguments\":{ \"ids\": [ " + addCommas torrentIDs + " ]}, \"method\": \""
        + generateString stateType + "\"}"

    let SessionGet =
        "{\"method\": \"session-get\"}"

    let SessionStats =
        "{\"method\": \"session-stats\"}"

    let TorrentDetails =
        let typeString = "\"activityDate\", \"comment\", \"desiredAvailable\", \"downloadDir\", \"downloadedEver\","
                         + "\"errorString\", \"eta\", \"files\", \"fileStats\", \"hashString\", \"haveUnchecked\","
                         + "\"haveValid\", \"id\", \"isFinished\", \"leftUntilDone\", \"metadataPercentComplete\","
                         + "\"name\", \"percentDone\", \"queuePosition\", \"rateDownload\", \"rateUpload\","
                         + "\"recheckProgress\", \"secondsDownloading\", \"secondsSeeding\", \"sizeWhenDone\","
                         + "\"status\", \"uploadedEver\", \"uploadRatio\""
        "{ \"arguments\": { \"fields\": [ " + typeString + "]}, \"method\": \"torrent-get\"}"

    let RemoveTorrents ids keepFiles =
        match keepFiles with
        | true -> "{\"arguments\":{ \"ids\": [ " + addCommas ids + " ]}, \"method\": \"torrent-remove\"}"
        | false -> "{\"arguments\":{ \"ids\": [ " + addCommas ids + " ], \"delete-local-data\": true},"
                   + "\"method\": \"torrent-remove\"}"
    
    let ChangeLocation items (location : string) (moveFiles : bool) =
        let escapedLocation = location.Replace("\\", "\\\\")
        "{\"arguments\":{ \"ids\": [" + addCommas items + "], \"location\": \"" + escapedLocation + "\", "
        + "\"move\": \"" + moveFiles.ToString().ToLower() + "\"}, \"method\": \"torrent-set-location\"}"