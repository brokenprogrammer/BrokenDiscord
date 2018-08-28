module BrokenDiscord.Api

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Text

open BrokenDiscord.Types
open BrokenDiscord.Json.Json

let setHeaders (client : HttpClient) (token : string) (userAgent : string) = 
    client.DefaultRequestHeaders.Add("Authorization", String.Format("Bot {0}", token))
    client.DefaultRequestHeaders.Add("User-Agent", userAgent)

type Api (token : string) =
    let baseURL = "https://discordapp.com/api"
    let userAgent = String.Format("DiscordBot ($url, $versionNumber)", "https://github.com/brokenprogrammer/BrokenDiscord", "6")
    let token = token
    
    let client : HttpClient = new HttpClient()
    do setHeaders client token userAgent

    member this.GET<'T> (path : string) =
        async {
            let! res = client.GetAsync(baseURL + path) |> Async.AwaitTask
            if res.IsSuccessStatusCode then
                let! content = res.Content.ReadAsStringAsync() |> Async.AwaitTask
                return content |> ofJson<'T> |> Some
            else
                return None
        }
    
    member this.POST<'T> (path : string, content : string) =
        async {
            let! res = client.PostAsync((baseURL + path), StringContent(content, Encoding.UTF8, "application/json")) |> Async.AwaitTask
            if res.IsSuccessStatusCode then
                let! content = res.Content.ReadAsStringAsync() |> Async.AwaitTask
                return content |> ofJson<'T> |> Some
            else
                return None
        }
    
    member this.PUT<'T> (path : string, content : string) =
        async {
            let! res = client.PutAsync((baseURL + path), StringContent(content, Encoding.UTF8, "appliation/json")) |> Async.AwaitTask
            if res.IsSuccessStatusCode then
                let! content = res.Content.ReadAsStringAsync() |> Async.AwaitTask
                return content |> ofJson<'T> |> Some
            else
                return None
        }
    
    member this.DELETE<'T> (path : string) =
        async {
            let! res = client.DeleteAsync(baseURL + path) |> Async.AwaitTask
            if res.IsSuccessStatusCode then
                let! content = res.Content.ReadAsStringAsync() |> Async.AwaitTask
                return content |> ofJson<'T> |> Some
            else
                return None
        }
    
    interface System.IDisposable with
        member this.Dispose () =
            client.Dispose()