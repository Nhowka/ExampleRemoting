module Client

open Elmish
open Elmish.React
open Elmish.Remoting

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Shared

open Fulma


type Model = Counter option

module Server =

    open Shared
    open Fable.Remoting.Client

    /// A proxy you can use to talk to server directly
    let api : ICounterProtocol =
        Proxy.remoting<ICounterProtocol> {
            use_route_builder Route.builder
        }

let init () : Model * Cmd<Msg<ServerMsg,ClientMsg>> =
    let model = None
    let cmd =
        Cmd.ofAsync
            Server.api.getInitCounter
            ()
            (Ok >> Init >> C)
            (Error >> Init >> C)
    model, cmd

let update (msg : ClientMsg) (model : Model) : Model * Cmd<Msg<ServerMsg,ClientMsg>> =
    match model,  msg with
    | model, Sync -> model, Cmd.ofMsg (S (Reset model))
    | _, Result x -> Some x, Cmd.none
    | None, Init (Ok x) -> Some x, Cmd.ofMsg (C Sync)
    | _ -> model, Cmd.none


let safeComponents =
    let intersperse sep ls =
        List.foldBack (fun x -> function
            | [] -> [x]
            | xs -> x::sep::xs) ls []

    let components =
        [
            "Saturn", "https://saturnframework.github.io/docs/"
            "Fable", "http://fable.io"
            "Elmish", "https://elmish.github.io/elmish/"
            "Fulma", "https://mangelmaxime.github.io/Fulma"
            "Fable.Remoting", "https://zaid-ajaj.github.io/Fable.Remoting/"
        ]
        |> List.map (fun (desc,link) -> a [ Href link ] [ str desc ] )
        |> intersperse (str ", ")
        |> span [ ]

    p [ ]
        [ strong [] [ str "SAFE Template" ]
          str " powered by: "
          components ]

let show = function
| Some x -> string x
| None -> "Loading..."

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let view (model : Model) (dispatch : Msg<ServerMsg,ClientMsg> -> unit) =
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "SAFE Template" ] ] ]

          Container.container []
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ Heading.h3 [] [ str ("Press buttons to manipulate counter: " + show model) ] ]
                Columns.columns []
                    [ Column.column [] [ button "-" (fun _ -> dispatch (S Decrement)) ]
                      Column.column [] [ button "+" (fun _ -> dispatch (S Increment)) ] ] ]

          Footer.footer [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ safeComponents ] ] ]


#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

RemoteProgram.mkProgram init update view
#if DEBUG
|> RemoteProgram.programBridge Program.withConsoleTrace
|> RemoteProgram.programBridgeWithMap Program.UserMsg Program.withHMR
#endif
|> RemoteProgram.programBridge (Program.withReact "elmish-app")
#if DEBUG
|> RemoteProgram.programBridge Program.withDebugger
#endif
|> RemoteProgram.onConnectionOpen Sync
|> RemoteProgram.runAt Route.Socket
