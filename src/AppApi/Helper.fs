namespace AppApi

open System
open FSharp.Core
open FSharp.Data.GraphQL.Types
open FSharp.Data.GraphQL.Relay

type 'a Cursor = 'a -> string

type MaybeBuilder() =

    member _.Bind(x, f) =
        match x with
        | Some x -> f x
        | None -> None

    member _.Return(x) = Some x
    member _.ReturnFrom(x) = x
    member _.Zero() = None

[<AutoOpen>]
module Helper =

    module Internal =
        let sliceToEdge (cursor: 'a Cursor) (value: 'a): 'a Edge = { Node = value; Cursor = cursor value }
        let hasNext (slice: 'a list) (all: 'a list) = slice.Tail <> (List.tail all)
        let hasPrevious (slice: 'a list) (all: 'a list) = slice.Head <> (List.head all)
        let startCursor (cursor: 'a Cursor) = List.head >> cursor >> Some
        let endCursor (cursor: 'a Cursor) = List.last >> cursor >> Some

        let findIndex (cursor: 'a Cursor) =
            function
            | Some x -> List.tryFindIndex (cursor >> (=) x)
            | _ -> (fun _ -> None)

        let findIndexBack (cursor: 'a Cursor) =
            function
            | Some x -> List.tryFindIndexBack (cursor >> (=) x)
            | _ -> (fun _ -> None)

    let toConnection (cursor: 'a Cursor) (slice: 'a list) (all: 'a list): 'a Connection =
        { Edges = slice |> Seq.map (Internal.sliceToEdge cursor)
          PageInfo =
              { HasNextPage = Internal.hasNext slice all
                HasPreviousPage = Internal.hasPrevious slice all
                StartCursor = Internal.startCursor cursor all
                EndCursor = Internal.endCursor cursor all }
          TotalCount = Some(slice.Length) }

    let resolveSlice (cursor: 'a Cursor) (values: 'a list) (slice: ResolveFieldContext) (): 'a Connection =
        match slice with
        | SliceInfo (Forward (first, after)) ->
            let idx =
                Internal.findIndex cursor after values
                |> Option.bind ((+) 1 >> Some)
                |> Option.defaultValue 0

            let slice =
                values
                |> List.splitAt idx
                |> snd
                |> List.truncate first

            toConnection cursor slice values
        | SliceInfo (Backward (last, before)) ->
            let idx =
                Internal.findIndexBack cursor before values
                |> Option.defaultValue (values.Length)

            let slice =
                values
                |> List.splitAt idx
                |> fst
                |> List.rev
                |> List.truncate last
                |> List.rev

            toConnection cursor slice values
        | _ -> toConnection cursor values values


    let maybe = MaybeBuilder()

    module Env =
        let defaultValue (key: string) (defaultValue: string): string =
            match Environment.GetEnvironmentVariable(key) with
            | value when isNull value || value = "" -> defaultValue
            | value -> value

    module Json =
        open Newtonsoft.Json

        let defaltSettings: JsonSerializerSettings =
            JsonSerializerSettings
                (ContractResolver = Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver())

        let serialize (o: obj): string =
            JsonConvert.SerializeObject(o, defaltSettings)

        let deserialize<'a> (json: string): 'a =
            JsonConvert.DeserializeObject<'a>(json, defaltSettings)
