namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module TracerTests =

    [<Fact>]
    let ``Tracer can add trace messages`` () =
        let context = ParserContextOps.create "test.fs" true
        let updated = Tracer.trace "test message" context
        updated.State.TraceLog |> should equal ["test message"]