namespace AppApi

module Config =

    type ServerConfig = { Host: string; Port: int }

    type Config = { Server: ServerConfig }

    module ServerConfig =
        let load () =
            { Host = Env.defaultValue "SERVER_HOST" "127.0.0.1"
              Port = Env.defaultValue "SERVER_PORT" "4000" |> int }

    let load () = { Server = ServerConfig.load () }
