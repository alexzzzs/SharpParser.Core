namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module ParserConfigTests =

    [<Fact>]
    let ``ParserConfig.create returns valid configuration`` () =
        let config = ParserConfig.create ()
        config |> should not' (be Null)

    [<Fact>]
    let ``ParserConfig.withCharHandler adds character handler`` () =
        let config = ParserConfig.create () |> ParserConfig.withCharHandler 'a' (fun ctx -> ctx)
        config |> should not' (be Null)

    [<Fact>]
    let ``ParserConfig.withSequenceHandler adds sequence handler`` () =
        let config = ParserConfig.create () |> ParserConfig.withSequenceHandler "test" (fun ctx -> ctx)
        config |> should not' (be Null)

    [<Fact>]
    let ``ParserConfig.withPatternHandler adds pattern handler`` () =
        let config = ParserConfig.create () |> ParserConfig.withPatternHandler @"\d+" (fun ctx matched -> ctx)
        config |> should not' (be Null)

    [<Fact>]
    let ``ParserConfig.withTokens enables tokenization`` () =
        let config = ParserConfig.create () |> ParserConfig.withTokens true
        ParserConfig.isTokenizationEnabled config |> should be True

    [<Fact>]
    let ``ParserConfig.withAST enables AST building`` () =
        let config = ParserConfig.create () |> ParserConfig.withAST true
        ParserConfig.isASTBuildingEnabled config |> should be True

    [<Fact>]
    let ``ParserConfig.withTrace enables tracing`` () =
        let config = ParserConfig.create () |> ParserConfig.withTrace true
        config |> should not' (be Null)