namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module TokenizerTests =

    [<Fact>]
    let ``Tokenizer can emit tokens`` () =
        let context = ParserContextOps.create "test.fs" false
        let tokenType = TokenType.Keyword "test"
        let updated = Tokenizer.emitToken tokenType context
        updated.State.Tokens.Length |> should equal 1
        updated.State.Tokens.Head.Type |> should equal tokenType