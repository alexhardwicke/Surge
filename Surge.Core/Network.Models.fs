namespace Surge.Core.Network

open Surge.Core.Models
open System
open System.Collections.Generic

/// Contains the data from the most recent update
type UpdateData = {
    /// Contains all new torrents, as fully populated as possible
    NewTorrents : Torrent seq
    /// Contains all updated torrents in a dictionary, with their id as the key.
    /// Note that some torrents in this list may be for torrents that have just
    /// resolved a magnet link, and as such contain more data that will have changed,
    /// such as the torrent's title and file structure
    UpdatedTorrents : IDictionary<int, Torrent> // Should I use something other than a Dictionary?
    /// Contains all deleted torrent IDs
    DeletedTorrents : int seq
    /// Contains the server statistics
    ServerStats : ServerStats }

/// Represents the different servers Surge supports
type ServerType = Transmission | UTorrent | Deluge

type InternalError =
    | Connection
    | Credential
    | Other
    | Version
    | BadRequest
    | NonJSON
    | NotFound

    static member FromString str =
        let getNextCase case =
            match case with
            | Connection -> Credential
            | Credential -> Other
            | Other -> Version
            | Version -> BadRequest
            | BadRequest -> NonJSON
            | NonJSON -> NotFound
            | NotFound -> failwith (str + " doesn't match any member of InternalError")

        let rec getErrorFromString str case =
            if str = case.ToString() then case
            else getErrorFromString str (getNextCase case)

        getErrorFromString str Connection

    override self.ToString() =
        match self with
        | Connection -> "Connection"
        | Credential -> "Credential"
        | Other -> "Other"
        | Version -> "Version"
        | BadRequest -> "BadRequest"
        | NonJSON -> "NonJSON"
        | NotFound -> "NotFound"

/// Represents the server
type Server(url : string, port : string, username : string, password : string, serverType : ServerType) =
    member this.URL = url
    member this.Port = port
    member this.Username = username
    member this.Password = password
    member this.ServerType = serverType
    member this.URI = let uri = match url.IndexOf("://") = -1 with
                                | true -> "http://" + url + ":" + port
                                | false -> url + ":" + port
                      Uri(uri)
    member this.HasCredentials =
        match String.IsNullOrEmpty username && String.IsNullOrEmpty password with
            | true -> false
            | false -> true

type internal TorrentData = { Id : int; HasFiles : bool }

type internal TorrentDataTracker = TorrentData seq

type internal InternalUpdate = { UpdateData : UpdateData; TorrentData : TorrentDataTracker }

type internal Response<'a> =
    | Success of value : 'a
    | Error of value: InternalError

type ServerState =
    | Working
    | Unconfigured
    | ServerError of value : InternalError

    static member internal FromResponse response =
        match response with
        | Success _ -> Working
        | Error value -> ServerError value

    static member FromString str =
        match str with
        | "Working" -> Working
        | "Unconfigured" -> Unconfigured
        | _ -> ServerError (InternalError.FromString str)

    member x.GetError =
        match x with
        | ServerError value -> value
        | _ -> failwith "Can't get error when no error"

    override x.ToString() =
        match x with
        | ServerError value -> value.ToString()
        | Working -> "Working"
        | Unconfigured -> "Unconfigured"