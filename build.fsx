#r "paket:
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target //"

#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

Target.initEnvironment ()

Target.create "Clean" (fun _ -> !! "src/**/bin" ++ "src/**/obj" |> Shell.cleanDirs)

Target.create "Restore" (fun _ ->
    !! "src/**/*.fsproj"
    |> Seq.iter (fun proj -> DotNet.restore id proj))

Target.create "Build" (fun _ -> !! "src/**/*.*proj" |> Seq.iter (DotNet.build id))

Target.create "All" ignore

"Format"
==> "Clean"
==> "Restore"
==> "Build"
==> "All"

Target.runOrDefault "All"
