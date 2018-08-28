module BrokenDiscord.Api

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Text

open BrokenDiscord.Types

let setHeaders (client : HttpClient) (token : string) (userAgent : string) = 
    client.DefaultRequestHeaders.Add("Authorization", String.Format("Bot {0}", token))
    client.DefaultRequestHeaders.Add("User-Agent", userAgent)

type Api (token : string) =
    let baseURL = "https://discordapp.com/api"
    let userAgent = String.Format("DiscordBot ($url, $versionNumber)", "https://github.com/brokenprogrammer/BrokenDiscord", "6")
    let token = token
    
    let client : HttpClient = new HttpClient()
    do setHeaders client token userAgent

    member this.GET (path : string) =
        async {
            let! res = client.GetAsync(baseURL + path) |> Async.AwaitTask
            do res.EnsureSuccessStatusCode |> ignore
            let! content = res.Content.ReadAsStringAsync() |> Async.AwaitTask
            return content
        }
    
    member this.POST (path : string, content : string) =
        async {
            let! res = client.PostAsync((baseURL + path), StringContent(content, Encoding.UTF8, "application/json")) |> Async.AwaitTask
            if res.IsSuccessStatusCode then
                let! content = res.Content.ReadAsStringAsync() |> Async.AwaitTask
                return Some content
            else
                return None
        }
    
    member this.PUT (path : string, content : string) =
        async {
            let! res = client.PutAsync((baseURL + path), StringContent(content, Encoding.UTF8, "appliation/json")) |> Async.AwaitTask
            if res.IsSuccessStatusCode then
                let! content = res.Content.ReadAsStringAsync() |> Async.AwaitTask
                return Some content
            else
                return None
        }
    
    member this.DELETE (path : string) =
        async {
            let! res = client.DeleteAsync(baseURL + path) |> Async.AwaitTask
            if res.IsSuccessStatusCode then
                let! content = res.Content.ReadAsStringAsync() |> Async.AwaitTask
                return Some content
            else
                return None
        }
    
    interface System.IDisposable with
        member this.Dispose () =
            client.Dispose()