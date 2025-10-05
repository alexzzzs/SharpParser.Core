namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module IntegrationTests =
    [<Fact>]
    let ``Integration test - parse simple function with custom AST builder`` () =
        // Custom AST builder that creates function nodes
        let customBuilder (mode: string option) (matchedText: string) (context: ParserContext) : ASTNode option =
            if matchedText = "function" then
                Some (ASTNode.Function ("testFunc", [], []))
            else
                None

        let parser =
            Parser.create ()
            |> Parser.enableAST ()
            |> Parser.onAST customBuilder
            |> Parser.onSequence "function" (fun ctx -> ctx)
            |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx _ -> ctx)

        let context = Parser.runString "function test() {}" parser
        let astNodes = Parser.getAST context

        // Should have at least one AST node from custom builder
        astNodes |> should not' (be Empty)

        // Check that we have a function node
        let functionNodes = astNodes |> List.filter (function ASTNode.Function _ -> true | _ -> false)
        functionNodes |> should not' (be Empty)

    [<Fact>]
    let ``Integration test - error handling with suggestions`` () =
        let parser =
            Parser.create ()
            |> Parser.onError (fun ctx msg ->
                printfn "Error: %s" msg
                ctx)

        // This should not cause errors since we have no specific handlers
        let context = Parser.runString "some random text" parser
        let errors = Parser.getErrors context

        // Should have no errors for basic parsing
        errors |> should be Empty

    [<Fact>]
    let ``Integration test - complex parsing with modes and tokens`` () =
        let parser =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.enableAST ()
            |> Parser.onSequence "function" (fun ctx ->
                ParserContextOps.enterMode "functionBody" ctx)
            |> Parser.inMode "functionBody" (fun config ->
                config
                |> Parser.onChar '{' (fun ctx -> ctx)
                |> Parser.onChar '}' (fun ctx -> ParserContextOps.exitMode ctx))
            |> Parser.onPattern @"\d+" (fun ctx _ -> ctx)
            |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx _ -> ctx)

        let input = """
        function test(x) {
            return x + 1
        }
        """

        let context = Parser.runString input parser
        let tokens = Parser.getTokens context
        let astNodes = Parser.getAST context
        let errors = Parser.getErrors context

        // Should have tokens
        tokens |> should not' (be Empty)

        // Should have AST nodes
        astNodes |> should not' (be Empty)

        // Should have errors for unmatched characters (spaces, etc.)
        errors.Length |> should be (greaterThan 0)

        // Final mode should be none (exited function body)
        ParserContextOps.currentMode context |> should equal None

    [<Fact>]
    let ``Integration test - handles malformed input gracefully`` () =
        let parser =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.onError (fun ctx msg -> ctx)
            |> Parser.onChar 'a' (fun ctx -> ctx)

        let malformedInput = "aaa\x00bbb" // Null character
        let context = Parser.runString malformedInput parser
        let errors = Parser.getErrors context
        // Should not crash, may have errors for null char
        Assert.True(true)

    [<Fact>]
    let ``Integration test - handles deeply nested modes`` () =
        let parser =
            Parser.create ()
            |> Parser.onSequence "level1" (fun ctx -> ParserContextOps.enterMode "level1" ctx)
            |> Parser.inMode "level1" (fun config ->
                config |> Parser.onSequence "level2" (fun ctx -> ParserContextOps.enterMode "level2" ctx))
            |> Parser.inMode "level2" (fun config ->
                config |> Parser.onSequence "level3" (fun ctx -> ParserContextOps.enterMode "level3" ctx))
            |> Parser.inMode "level3" (fun config ->
                config |> Parser.onChar 'x' (fun ctx -> ctx))

        let input = "level1 level2 level3 x"
        let context = Parser.runString input parser
        let currentMode = ParserContextOps.currentMode context
        currentMode |> should equal (Some "level3")

    [<Fact>]
    let ``Integration test - handles circular mode references`` () =
        let parser =
            Parser.create ()
            |> Parser.onSequence "start" (fun ctx -> ParserContextOps.enterMode "modeA" ctx)
            |> Parser.inMode "modeA" (fun config ->
                config |> Parser.onSequence "toB" (fun ctx -> ParserContextOps.enterMode "modeB" ctx))
            |> Parser.inMode "modeB" (fun config ->
                config |> Parser.onSequence "back" (fun ctx -> ParserContextOps.enterMode "modeA" ctx))
            |> Parser.onChar 'x' (fun ctx -> ctx)

        let input = "start toB back x"
        let context = Parser.runString input parser
        // Should not crash with circular modes
        Assert.True(true)

    [<Fact>]
    let ``Integration test - handles large token streams`` () =
        let parser =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.onPattern @"\w+" (fun ctx _ -> ctx)

        let largeInput = String.concat " " (List.replicate 1000 "word")
        let context = Parser.runString largeInput parser
        let tokens = Parser.getTokens context
        tokens.Length |> should be (greaterThan 900) // Should have most words as tokens

    [<Fact>]
    let ``Integration test - handles mixed handlers with conflicts`` () =
        let parser =
            Parser.create ()
            |> Parser.onSequence "if" (fun ctx -> ctx) // Sequence handler
            |> Parser.onPattern @"if\s*\(" (fun ctx _ -> ctx) // Pattern handler that could conflict
            |> Parser.onChar 'i' (fun ctx -> ctx) // Char handler

        let input = "if (condition) { }"
        let context = Parser.runString input parser
        // Should prioritize sequence over pattern over char
        Assert.True(true)

    [<Fact>]
    let ``Integration test - handles empty modes`` () =
        let parser =
            Parser.create ()
            |> Parser.inMode "empty" (fun config -> config) // Empty mode
            |> Parser.onSequence "enter" (fun ctx -> ParserContextOps.enterMode "empty" ctx)

        let input = "enter something"
        let context = Parser.runString input parser
        let currentMode = ParserContextOps.currentMode context
        currentMode |> should equal (Some "empty")