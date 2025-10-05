namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module ASTBuilderTests =

    [<Fact>]
    let ``ASTBuilder can build nodes`` () =
        let context = ParserContextOps.create "test.fs" false
        let result = ASTBuilder.buildNode "global" "test" context
        result |> should not' (be Null)