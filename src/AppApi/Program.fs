module Program

open AppApi
open AppApi.Config
open Suave
open Suave.Operators
open Suave.Filters
open System
open System.Text
open FSharpPlus.Lens
open FSharp.Data.GraphQL
open FSharp.Data.GraphQL.Execution
open Root

let _tryGetQuery (data: byte ReadOnlySpan): string option =
    let data = Encoding.UTF8.GetString data
    if isNull data || data = "" then
        None
    else
        Json.deserialize<Map<string, obj>> (data)
        |> Map.tryFind ("query")
        |> Option.map (function
            | :? string as q -> Some q
            | _ -> None)
        |> Option.defaultValue None

let tryGetQuery (data: byte ReadOnlySpan): string option =
    let data = Encoding.UTF8.GetString data
    maybe {
        if not <| isNull data && data <> "" then
            let data =
                Json.deserialize<Map<string, obj>> (data)

            let! query = Map.tryFind "query" data

            return! query
                    |> (function
                    | :? string as q -> Some q
                    | _ -> None)
    }

let graphql (schema: _ Schema): WebPart =
    fun http ->
        async {
            match tryGetQuery (ReadOnlySpan http.request.rawForm) with
            | Some query ->
                let! result = Executor(schema).AsyncExecute(query)

                let result =
                    match result with
                    | Direct (data, []) -> Json.serialize data
                    | Direct (_, errs) ->
                        printfn
                            "Error: %A"
                            (errs
                             |> List.map string
                             |> List.reduce (fun acc x -> acc + ",\n" + x))
                        Json.serialize "{}"
                    | _ -> failwith "Not Implemented!"

                return! Successful.OK result http
            | _ -> return! Successful.no_content http
        }

let config = Config.load ()

let suaveConfig =
    { Web.defaultConfig with
          bindings = [ HttpBinding.createSimple HTTP config.Server.Host config.Server.Port ] }

let setCorsHeaders: WebPart =
    Writers.setHeader "Access-Control-Allow-Origin" "*"
    >=> Writers.setHeader "Access-Control-Allow-Headers" "Content-Type"
    >=> Writers.addHeader "Access-Control-Allow-Headers" "X-Apollo-Tracing"

let schemaConfig = SchemaConfig.Default
let schema = Schema(Query)

let api =
    choose [ path "/" >=> Successful.OK "use playground"

             path "/graphql"
             >=> setCorsHeaders
             >=> graphql schema
             >=> Writers.setMimeType "application/json" ]


startWebServer suaveConfig api
