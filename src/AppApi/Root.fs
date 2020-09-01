namespace AppApi


open FSharp.Data.GraphQL.Types
open Schema


type Root = obj

module Root =


    let Query =
        Define.Object<Root>
            (name = "Query",
             fields =
                 [ Define.Field(name = "allUser", description = "", typedef = ListOf User, resolve = fun _ _ -> users)
                   Define.Field
                       (name = "allTodo", description = "all of todo", typedef = ListOf Todo, resolve = fun _ _ -> todos) ])
