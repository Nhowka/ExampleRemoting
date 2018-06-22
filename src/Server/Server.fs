open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Saturn
open Shared

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Elmish.Remoting
open Elmish

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us

type Model = Counter option
let init () = None, Cmd.ofMsg (C Sync)
let update msg model =
    match model, msg with
    |_,Reset model -> model, Cmd.none
    |Some x, Increment ->
        let n = x + 1
        Some n, Cmd.ofMsg (C (Result n))
    |Some x, Decrement ->
        let n = x - 1
        Some n, Cmd.ofMsg (C (Result n))

    |_ -> model, Cmd.none

let getInitCounter () : Task<Counter> = task { return 42 }



let webApp =
    let server =
        { getInitCounter = getInitCounter >> Async.AwaitTask }
    let remotingAPI =
        remoting server {
            use_route_builder Route.builder
        }
    let elmishAPI =
        ServerProgram.mkProgram init update
        |> ServerProgram.runServerAt Giraffe.server Route.Socket
    choose [
        remotingAPI
        elmishAPI
    ]

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    router webApp
    memory_cache
    use_static publicPath
    app_config Giraffe.useWebSockets
    // sitemap diagnostic data cannot be inferred when using Fable.Remoting
    // Saturn issue at https://github.com/SaturnFramework/Saturn/issues/64
    disable_diagnostics
    use_gzip
}

run app
