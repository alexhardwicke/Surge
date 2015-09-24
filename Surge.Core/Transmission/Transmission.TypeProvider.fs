namespace Surge.Core.Transmission
open FSharp.Data

type SessionGet = JsonProvider<"Transmission/Json/session-get.json", true, EmbeddedResource="Surge.Core.dll, session-get.json">
type SessionStats = JsonProvider<"Transmission/Json/session-stats.json", EmbeddedResource="Surge.Core.dll, session-stats.json">
type TorrentGet = JsonProvider<"Transmission/Json/torrent-get.json", EmbeddedResource="Surge.Core.dll, torrent-get.json">