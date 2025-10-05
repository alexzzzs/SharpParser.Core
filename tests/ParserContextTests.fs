namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module ParserContextTests =

    [<Fact>]
    let ``ParserContextOps.create initializes context correctly`` () =
        let context = ParserContextOps.create "test.fs" false
        context.Line |> should equal 1
        context.Col |> should equal 1
        context.FilePath |> should equal "test.fs"
        context.CurrentChar |> should equal None
        context.Buffer |> should equal ""
        context.ModeStack |> should be Empty
        context.State.Tokens |> should be Empty
        context.State.ASTNodes |> should be Empty
        context.State.Errors |> should be Empty
        context.State.TraceLog |> should be Empty
        context.State.UserData |> should be Empty

    [<Fact>]
    let ``ParserContextOps.enterMode pushes mode to stack`` () =
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.enterMode "function"
        context.ModeStack |> should equal ["function"]

    [<Fact>]
    let ``ParserContextOps.enterMode handles multiple modes`` () =
        let context =
            ParserContextOps.create "test.fs" false
            |> ParserContextOps.enterMode "global"
            |> ParserContextOps.enterMode "function"
            |> ParserContextOps.enterMode "block"
        context.ModeStack |> should equal ["block"; "function"; "global"]

    [<Fact>]
    let ``ParserContextOps.exitMode pops mode from stack`` () =
        let context =
            ParserContextOps.create "test.fs" false
            |> ParserContextOps.enterMode "function"
            |> ParserContextOps.exitMode
        context.ModeStack |> should be Empty

    [<Fact>]
    let ``ParserContextOps.exitMode handles empty stack`` () =
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.exitMode
        context.ModeStack |> should be Empty

    [<Fact>]
    let ``ParserContextOps.currentMode returns top of stack`` () =
        let context =
            ParserContextOps.create "test.fs" false
            |> ParserContextOps.enterMode "function"
        ParserContextOps.currentMode context |> should equal (Some "function")

    [<Fact>]
    let ``ParserContextOps.currentMode returns None for empty stack`` () =
        let context = ParserContextOps.create "test.fs" false
        ParserContextOps.currentMode context |> should equal None

    [<Fact>]
    let ``ParserContextOps.updatePosition changes line and column`` () =
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.updatePosition 5 10
        context.Line |> should equal 5
        context.Col |> should equal 10

    [<Fact>]
    let ``ParserContextOps.updateChar sets current character`` () =
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.updateChar (Some 'a')
        context.CurrentChar |> should equal (Some 'a')

    [<Fact>]
    let ``ParserContextOps.updateBuffer sets buffer string`` () =
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.updateBuffer "test"
        context.Buffer |> should equal "test"

    [<Fact>]
    let ``ParserContextOps.addToken appends token to state`` () =
        let token = { Type = TokenType.Keyword "if"; Line = 1; Col = 1; Mode = None }
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.addToken token
        context.State.Tokens |> should equal [token]

    [<Fact>]
    let ``ParserContextOps.addToken maintains order`` () =
        let token1 = { Type = TokenType.Keyword "if"; Line = 1; Col = 1; Mode = None }
        let token2 = { Type = TokenType.Symbol "{"; Line = 1; Col = 4; Mode = None }
        let context =
            ParserContextOps.create "test.fs" false
            |> ParserContextOps.addToken token1
            |> ParserContextOps.addToken token2
        context.State.Tokens |> should equal [token2; token1] // Internal storage is reversed

    [<Fact>]
    let ``ParserContextOps.addASTNode appends AST node`` () =
        let node = ASTNode.Literal "42"
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.addASTNode node
        context.State.ASTNodes |> should equal [node]

    [<Fact>]
    let ``ParserContextOps.addError appends error`` () =
        let errorInfo = {
            ErrorType = ParseError.GenericError "Syntax error"
            Line = 1
            Col = 1
            Mode = None
            Message = "Syntax error"
            Suggestion = None
        }
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.addError errorInfo
        context.State.Errors |> should equal [errorInfo]

    [<Fact>]
    let ``ParserContextOps.addTrace appends trace message`` () =
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.addTrace "Parsing started"
        context.State.TraceLog |> should equal ["Parsing started"]

    [<Fact>]
    let ``ParserContextOps.setUserData stores data`` () =
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.setUserData "key" "value"
        let retrieved = ParserContextOps.getUserData "key" context
        match retrieved with
        | Some (:? string as s) -> s |> should equal "value"
        | _ -> Assert.True(false, "Expected string value")

    [<Fact>]
    let ``ParserContextOps.getUserData returns None for missing key`` () =
        let context = ParserContextOps.create "test.fs" false
        ParserContextOps.getUserData "missing" context |> should equal None

    [<Fact>]
    let ``ParserContextOps.setUserData overwrites existing data`` () =
        let context =
            ParserContextOps.create "test.fs" false
            |> ParserContextOps.setUserData "key" "value1"
            |> ParserContextOps.setUserData "key" "value2"
        let retrieved = ParserContextOps.getUserData "key" context
        match retrieved with
        | Some (:? string as s) -> s |> should equal "value2"
        | _ -> Assert.True(false, "Expected string value")

    [<Fact>]
    let ``ParserContextOps.getState returns current state`` () =
        let context = ParserContextOps.create "test.fs" false
        let state = ParserContextOps.getState context
        state.Tokens |> should be Empty
        state.ASTNodes |> should be Empty
        state.Errors |> should be Empty
        state.TraceLog |> should be Empty
        state.UserData |> should be Empty

    [<Fact>]
    let ``ParserContextOps.setState updates state`` () =
        let errorInfo = {
            ErrorType = ParseError.GenericError "test error"
            Line = 1
            Col = 1
            Mode = None
            Message = "test error"
            Suggestion = None
        }
        let newState = {
            Tokens = [{ Type = TokenType.Number 42.0; Line = 1; Col = 1; Mode = None }]
            ASTNodes = [ASTNode.Literal "42"]
            Errors = [errorInfo]
            TraceLog = ["test trace"]
            UserData = Map.ofList [("key", "value")]
        }
        let context = ParserContextOps.create "test.fs" false |> ParserContextOps.setState newState
        context.State |> should equal newState