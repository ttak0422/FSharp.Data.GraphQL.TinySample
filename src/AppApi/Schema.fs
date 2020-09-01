namespace AppApi


open FSharp.Data.GraphQL.Types
open FSharp.Data.GraphQL.Relay


#nowarn "40"



module Schema =

    // dummy
    let users: User list =
        [ { Id = "1"; Name = "foo" }
          { Id = "2"; Name = "bar" } ]

    let todos =
        [ { Id = "1"
            Owner = users.[0]
            Title = "title1"
            Description = Some "description" }
          { Id = "2"
            Owner = users.[0]
            Title = "title2"
            Description = None }
          { Id = "3"
            Owner = users.[1]
            Title = "title3"
            Description = Some "description" } ]


    let rec User: User ObjectDef =
        Define.Object<User>
            (name = "User",
             description = "user",
             interfaces = [ Node ],
             fieldsFn =
                 fun () ->
                     [ Define.GlobalIdField(fun _ u -> u.Id)
                       Define.Field("name", String, (fun _ u -> u.Name))
                       Define.Field
                           (name = "todolist",
                            typedef = ConnectionOf Todo,
                            description = "the list of todo",
                            args = Connection.allArgs,
                            resolve =
                                fun ctx user ->
                                    let slice =
                                        todos |> List.where (fun t -> t.Owner = user)

                                    resolveSlice (fun t -> t.Id) slice ctx ()) ])

    and Todo: Todo ObjectDef =
        Define.Object<Todo>
            (name = "todo",
             description = "todo",
             fieldsFn =
                 fun () ->
                     [ Define.GlobalIdField(fun _ t -> t.Id)
                       Define.Field("owner", User, (fun _ t -> t.Owner))
                       Define.Field("title", String, (fun _ t -> t.Title))
                       Define.Field("description", Nullable String, (fun _ t -> t.Description)) ])

    and Node = Define.Node<obj>(fun () -> [ User ])
