namespace Surge.Core.Models

/// Contains details about the server as a whole
type ServerStats = { DownloadSpeed : int64
                     UploadSpeed : int64
                     ServerVersion : int
                     DefaultDownloadLocation : string
                     SpaceRemaining : int64 }

type StateType = ForceStart | Start | Stop | Verify

/// An abstract class representing base data that both files and folders share in common.
[<AbstractClass>]
type Item(torrentId : int, name : string, parent : Folder option) =
    let mutable parent = parent
    member internal this.MutableParent with set(value) = parent <- Some value
    member this.TorrentID = torrentId
    member this.Name = name
    member this.HasParent = parent.IsSome
    /// Throws an exception if HasParent is false
    member this.Parent =
        match parent with
        | Some value -> value
        | None -> failwith "Item has no parent"

/// Represents a folder and contains the appropriate data. Is only sent when providing
/// a new torrent or details about a magnet torrent.
and Folder(torrentId, name, parent : Folder option) =
    inherit Item(torrentId, name, parent)
    let mutable children : Item list = []
    member internal this.MutableChildren with get() = children and set(value) = children <- value
    member this.Children with get() = List.toSeq children

/// Represents a file and contains the appropriate data.
and File(torrId, id : int, priority : int, isWanted : bool, bytesCompleted : int64,
         name, fileSize : int64, parent : Folder option) =
    inherit Item(torrId, name, parent)
    member this.IsWanted = isWanted
    member this.Priority = priority
    member this.FileSize = fileSize
    member this.BytesCompleted = bytesCompleted
    member this.Id = id

/// Represents a torrent and contains the relevant data.
type Torrent = { Files : Item seq
                 ID : int
                 IsFinished : bool
                 IsPaused : bool
                 IsVerifying : bool
                 IsMagnetResolving : bool
                 Comment : string
                 Error : string
                 Hash : string
                 Name : string
                 Location : string
                 MagnetResolvedPercent : float
                 VerifiedPercent : float
                 Percent : float
                 Ratio : float
                 Availability : int64
                 Desired : int64
                 Downloaded : int64
                 DownloadSpeed : int64
                 QueuePosition : int
                 Size : int64
                 Uploaded : int64
                 UploadSpeed : int64
                 LastActivity : int
                 RemainingTime : int
                 RunningTime : int  }