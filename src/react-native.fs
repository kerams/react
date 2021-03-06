namespace Elmish.ReactNative

open System
open Fable.Import.React
open Fable.Core
open Elmish

module Components =
    type [<Pojo>] AppState = { 
        render : unit -> ReactElement
        setState : AppState -> unit
    }

    let mutable appState = None

    type App(props) as this =
        inherit Component<obj,AppState>(props)
        do
            match appState with
            | Some state ->
                appState <- Some { state with AppState.setState = this.setInitState }
                this.setInitState state
            | _ -> failwith "was Elmish.ReactNative.Program.withReactNative called?"

        member this.componentDidMount() =
            appState <- Some { appState.Value with setState = this.setState }

        member this.componentWillUnmount() =
            appState <- Some { appState.Value with setState = ignore; render = this.state.render }

        member this.render () = 
            this.state.render()

[<Import("AppRegistry","react-native")>] 
type AppRegistry =
    static member registerComponent(appKey:string, getComponentFunc:unit->ComponentClass<_>) : unit = 
        failwith "JS only"

[<RequireQualifiedAccess>]
module Program =
    open Fable.Core.JsInterop
    open Elmish.React
    open Components

    /// Setup rendering of root ReactNative component
    let withReactNative appKey (program:Program<_,_,_,_>) =
        AppRegistry.registerComponent(appKey, fun () -> unbox typeof<App>)
        let render m d =
             match appState with
             | Some state -> 
                state.setState { state with render = fun () -> program.view m d }
             | _ -> 
                appState <- Some { render = fun () -> program.view m d 
                                   setState = ignore }
        { program with setState = render }
