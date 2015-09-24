namespace Surge.Core.Network

open System
open System.Linq
open System.Net
open System.Net.Http
open System.Net.Http.Headers

type TorrentNetworkClient internal (server : Server, uriSuffix, csrfHeader, onException : Exception -> Async<unit>) =
    let handler = new HttpClientHandler(Credentials = NetworkCredential(server.Username, server.Password))
    let client = new HttpClient(handler)
    let mutable csrfToken = ""

    let rec sendToClientAsync (processResult : string -> 'a) data = async {
        let uri = server.URI.ToString() + uriSuffix
        try
            use content = new StringContent(data)
            content.Headers.ContentType <- MediaTypeHeaderValue("application/json")
            content.Headers.Add(csrfHeader, csrfToken)
            use! response = Async.AwaitTask <| client.PostAsync(uri, content)
            if response.StatusCode = HttpStatusCode.Conflict then
                csrfToken <- response.Headers.GetValues(csrfHeader).FirstOrDefault()
                return! sendToClientAsync processResult data
            else
                match response.StatusCode with
                | HttpStatusCode.OK ->
                    let! result = Async.AwaitTask <| response.Content.ReadAsStringAsync()
                    return Success (processResult result)
                | HttpStatusCode.Unauthorized ->
                    return Error Credential
                | HttpStatusCode.BadRequest ->
                    do! onException (Exception("Bad Request"))
                    return Error BadRequest
                | HttpStatusCode.NotFound ->
                    return Error NotFound
                | code when code.ToString() = "520" ->
                    // Error 520: Generic catch-all used by Microsoft amongst others.
                    return Error Other
                | code when code.ToString() = "522" ->
                    // Error 522: Timeout (used by CloudFlare)
                    return Error Connection
                | code ->
                    do! onException (Exception("Unsupported status: " + code.ToString()))
                    return Error Other
                
        with
            | :? HttpRequestException as ex ->
                return Error Connection
            | :? AggregateException as ex -> match ex.InnerException with
                                                | :? HttpRequestException ->
                                                    return Error Connection
                                                | _ ->
                                                    do! onException ex
                                                    return Error Other
            | ex ->
                do! onException ex
                return Error Other }

    let sendAsync processResult json = async {
        return! sendToClientAsync processResult json }
        
    member internal this.SendWithResultAsync processResult json = async {
        return! sendAsync processResult json }
        
    member internal this.SendNoResultAsync json = async {
        do! sendAsync ignore json |> Async.Ignore }

    interface IDisposable with
        member this.Dispose(): unit =
            if handler <> null then
                handler.Dispose()
            else ()
            if client <> null then
                client.Dispose()
            else ()
