open System.Net.Http
open NBomber
open NBomber.Contracts
open NBomber.Http
open NBomber.Http.FSharp
open NBomber.FSharp

printfn "Stress Test Pusher"

let client = new HttpClient()

Scenario.create (
    "pusher_http",
    fun _ ->
        task {
            let! response =
                Http.createRequest "POST" "http://localhost:7166/notification?channel=pusher&event=pusher&key=key"
                |> Http.withHeader "Content-Type" "application/json"
                |> Http.withBody (new StringContent("{'message':'Hello, World!'}"))
                |> Http.send client

            return response
        }
)
|> Scenario.withoutWarmUp
|> Scenario.withLoadSimulations [ Inject(rate = 100, interval = seconds 1, during = minutes 1) ]
|> NBomberRunner.registerScenario
|> NBomberRunner.withWorkerPlugins [ new HttpMetricsPlugin() ]
|> NBomberRunner.run
|> ignore
