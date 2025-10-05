namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module TypesTests =

    [<Fact>]
    let ``TokenType.Keyword creates keyword token`` () =
        let tokenType = TokenType.Keyword "function"
        tokenType |> should equal (TokenType.Keyword "function")

    [<Fact>]
    let ``TokenType.Identifier creates identifier token`` () =
        let tokenType = TokenType.Identifier "myVar"
        tokenType |> should equal (TokenType.Identifier "myVar")

    [<Fact>]
    let ``TokenType.Symbol creates symbol token`` () =
        let tokenType = TokenType.Symbol "+"
        tokenType |> should equal (TokenType.Symbol "+")

    [<Fact>]
    let ``TokenType.Number creates number token`` () =
        let tokenType = TokenType.Number 42.5
        tokenType |> should equal (TokenType.Number 42.5)

    [<Fact>]
    let ``TokenType.StringLiteral creates string literal token`` () =
        let tokenType = TokenType.StringLiteral "hello"
        tokenType |> should equal (TokenType.StringLiteral "hello")

    [<Fact>]
    let ``TokenType.EOF represents end of file`` () =
        TokenType.EOF |> should equal TokenType.EOF

    [<Fact>]
    let ``Token record stores position and type information`` () =
        let token = {
            Type = TokenType.Keyword "if"
            Line = 5
            Col = 10
            Mode = Some "global"
        }
        token.Type |> should equal (TokenType.Keyword "if")
        token.Line |> should equal 5
        token.Col |> should equal 10
        token.Mode |> should equal (Some "global")

    [<Fact>]
    let ``Token can have None mode`` () =
        let token = {
            Type = TokenType.Identifier "x"
            Line = 1
            Col = 1
            Mode = None
        }
        token.Mode |> should equal None

    [<Fact>]
    let ``ASTNode.Function represents function definition`` () =
        let funcNode = ASTNode.Function ("calculate", [], [ASTNode.Literal "body"])
        funcNode |> should equal (ASTNode.Function ("calculate", [], [ASTNode.Literal "body"]))

    [<Fact>]
    let ``ASTNode.If represents conditional statement`` () =
        let ifNode = ASTNode.If (ASTNode.Literal "condition", [ASTNode.Literal "body"], None)
        ifNode |> should equal (ASTNode.If (ASTNode.Literal "condition", [ASTNode.Literal "body"], None))

    [<Fact>]
    let ``ASTNode.Assignment represents variable assignment`` () =
        let assignNode = ASTNode.Assignment ("x", ASTNode.Literal "42")
        assignNode |> should equal (ASTNode.Assignment ("x", ASTNode.Literal "42"))

    [<Fact>]
    let ``ASTNode.Expression represents complex expression`` () =
        let exprNode = ASTNode.Expression "x + y * 2"
        exprNode |> should equal (ASTNode.Expression "x + y * 2")

    [<Fact>]
    let ``ASTNode.Literal represents literal value`` () =
        let litNode = ASTNode.Literal "42"
        litNode |> should equal (ASTNode.Literal "42")

    [<Fact>]
    let ``ParserState initializes with empty collections`` () =
        let state = {
            Tokens = []
            ASTNodes = []
            Errors = []
            TraceLog = []
            UserData = Map.empty
        }
        state.Tokens |> should be Empty
        state.ASTNodes |> should be Empty
        state.Errors |> should be Empty
        state.TraceLog |> should be Empty
        state.UserData |> should be Empty

    [<Fact>]
    let ``ParserContext initializes with default values`` () =
        let context = {
            Line = 1
            Col = 1
            FilePath = "test.fs"
            CurrentChar = None
            Buffer = ""
            ModeStack = []
            State = {
                Tokens = []
                ASTNodes = []
                Errors = []
                TraceLog = []
                UserData = Map.empty
            }
            EnableTrace = false
        }
        context.Line |> should equal 1
        context.Col |> should equal 1
        context.FilePath |> should equal "test.fs"
        context.CurrentChar |> should equal None
        context.Buffer |> should equal ""
        context.ModeStack |> should be Empty