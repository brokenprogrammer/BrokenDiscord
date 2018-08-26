module BrokenDiscord.Api

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open BrokenDiscord.Types

type Api (token : string) =
    let client : HttpClient = new HttpClient()

    let baseURL = "https://discordapp.com/api"
    let userAgent = String.Format("DiscordBot ($url, $versionNumber)", "https://github.com/brokenprogrammer/BrokenDiscord", "6") //TODO FORMAT
    let token = token 

    //TODO: Abstract GetAsync function
    member this.getChannel (id : Snowflake) = 
        async {
            let! res = client.GetAsync(baseURL + String.Format("/channels/{0}", id)) |> Async.AwaitTask
            if res.IsSuccessStatusCode <> true then
                printf "Error"
            
            let! channel = res.Content.ReadAsStringAsync() |> Async.AwaitTask
            printf "CHANNEL FROM API: %s" channel
            return channel
        }

    //TODO: This should be set on type created and hidden from api user.
    member this.setheaders = 
        client.DefaultRequestHeaders.Accept.Clear()
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"))
        client.DefaultRequestHeaders.Add("Authorization", String.Format("Bot {0}", token))
        client.DefaultRequestHeaders.Add("User-Agent", userAgent)