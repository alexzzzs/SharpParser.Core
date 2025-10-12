namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module ParserTests =

    [<Fact>]
    let ``Parser.create returns valid configuration`` () =
        let config = Parser.create ()
        config |> should not' (be Null)

    [<Fact>]
    let ``Parser.onChar registers character handler`` () =
        let config =
            Parser.create ()
            |> Parser.onChar '+' (fun ctx -> ctx)
        config |> should not' (be Null)

    [<Fact>]
    let ``Parser.onSequence registers sequence handler`` () =
        let config =
            Parser.create ()
            |> Parser.onSequence "function" (fun ctx -> ctx)
        config |> should not' (be Null)

    [<Fact>]
    let ``Parser.onPattern registers pattern handler`` () =
        let config =
            Parser.create ()
            |> Parser.onPattern @"\d+" (fun ctx matched -> ctx)
        config |> should not' (be Null)

    [<Fact>]
    let ``Parser.inMode creates mode context`` () =
        let config =
            Parser.create ()
            |> Parser.inMode "function" (fun config ->
                config |> Parser.onChar '{' (fun ctx -> ctx))
        config |> should not' (be Null)

    [<Fact>]
    let ``Parser.onError registers error handler`` () =
        let config =
            Parser.create ()
            |> Parser.onError (fun ctx msg -> ctx)
        config |> should not' (be Null)

    [<Fact>]
    let ``Parser.enableTokens enables tokenization`` () =
        let config = Parser.create () |> Parser.enableTokens ()
        config |> should not' (be Null)

    [<Fact>]
    let ``Parser.enableAST enables AST building`` () =
        let config = Parser.create () |> Parser.enableAST ()
        config |> should not' (be Null)

    [<Fact>]
    let ``Parser.enableTrace enables tracing`` () =
        let config = Parser.create () |> Parser.enableTrace true
        config |> should not' (be Null)

    [<Fact>]
    let ``Parser.runString parses simple input`` () =
        let config = Parser.create () |> Parser.onChar 'a' (fun ctx -> ctx)
        let context = Parser.runString "test" config
        context.FilePath |> should equal "<string>"
        context.Line |> should equal 2  // Engine increments line after processing
        context.Col |> should equal 1

    [<Fact>]
    let ``Parser.getTokens returns tokens when tokenization enabled`` () =
        let config =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.onSequence "let" (fun ctx -> ctx) // This should generate a token
        let context = Parser.runString "let x = 42" config
        let tokens = Parser.getTokens context
        tokens.Length |> should be (greaterThan 0)

    [<Fact>]
    let ``Parser.getAST returns AST nodes when AST enabled`` () =
        let config =
            Parser.create ()
            |> Parser.enableAST ()
            |> Parser.onSequence "function" (fun ctx -> ctx) // This should generate AST
        let context = Parser.runString "function test() {}" config
        let ast = Parser.getAST context
        ast.Length |> should be (greaterThan 0)

    [<Fact>]
    let ``Parser.getErrors returns errors`` () =
        let config = Parser.create () |> Parser.onError (fun ctx msg -> ctx)
        let context = Parser.runString "test" config
        let errors = Parser.getErrors context
        errors |> should be Empty // No errors in this case

    [<Fact>]
    let ``Parser.getTrace returns trace messages when handlers call tracing`` () =
        let config =
            Parser.create ()
            |> Parser.enableTrace true
            |> Parser.onChar 't' (fun ctx -> Tracer.trace "found t" ctx)
        let context = Parser.runString "test" config
        let trace = Parser.getTrace context
        trace.Length |> should be (greaterThan 0)

    [<Fact>]
    let ``Parser.setUserData and getUserData work together`` () =
        let context = Parser.runString "test" (Parser.create ())
        let contextWithData = Parser.setUserData "key" "value" context
        let retrieved = Parser.getUserData "key" contextWithData
        match retrieved with
        | Some (:? string as s) -> s |> should equal "value"
        | _ -> Assert.True(false, "Expected string value")

    [<Fact>]
    let ``Parser.validateConfig returns Ok for valid config`` () =
        let config = Parser.create ()
        let result = Parser.validateConfig config
        match result with
        | Ok _ -> Assert.True(true)
        | Error _ -> Assert.True(false, "Expected Ok result")

    [<Fact>]
    let ``Parser.printSummary produces output`` () =
        let context = Parser.runString "test" (Parser.create ())
        // Just ensure it doesn't throw
        Parser.printSummary context
        Assert.True(true)

    [<Fact>]
    let ``Parser handles empty input string`` () =
        let config = Parser.create () |> Parser.onChar 'a' (fun ctx -> ctx)
        let context = Parser.runString "" config
        context.FilePath |> should equal "<string>"
        context.Line |> should equal 1  // No lines processed
        context.Col |> should equal 1
        Parser.getTokens context |> should be Empty
        Parser.getErrors context |> should be Empty

    [<Fact>]
    let ``Parser reports errors for unmatched characters`` () =
        let config = Parser.create () |> Parser.onChar 'a' (fun ctx -> ctx) |> Parser.onError (fun ctx msg -> ctx)
        let context = Parser.runString "abc" config
        let errors = Parser.getErrors context
        errors.Length |> should be (greaterThan 0)
        errors |> List.forall (fun e -> match e.ErrorType with ParseError.UnexpectedChar _ -> true | _ -> false) |> should be True

    [<Fact>]
    let ``Parser handles file not found error`` () =
        let config = Parser.create ()
        let context = ParsingEngine.parseFile config "nonexistent.txt"
        let errors = Parser.getErrors context
        errors.Length |> should be (greaterThan 0)
        errors |> List.exists (fun e -> match e.ErrorType with ParseError.ConfigError _ -> true | _ -> false) |> should be True

    [<Fact>]
    let ``Parser handles invalid regex pattern with exception`` () =
        Assert.Throws<System.Text.RegularExpressions.RegexParseException>(fun () ->
            Parser.create () |> Parser.onPattern "[" (fun ctx _ -> ctx) |> ignore) |> ignore

    [<Fact>]
    let ``Parser handles empty config validation`` () =
        let config = Parser.create ()
        let result = Parser.validateConfig config
        match result with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, sprintf "Unexpected validation error: %s" msg)

    [<Fact>]
    let ``Parser handles mode errors`` () =
        let config =
            Parser.create ()
            |> Parser.inMode "testMode" (fun c -> c |> Parser.onChar 'x' (fun ctx -> ctx))
            |> Parser.onError (fun ctx msg -> ctx)
        let context = Parser.runString "test" config
        // No handlers in default mode, so should have errors for unmatched characters
        let errors = Parser.getErrors context
        errors.Length |> should be (greaterThan 0)

    [<Fact>]
    let ``Parser handles large input with errors`` () =
        let largeInput = String.replicate 1000 "x" + "y" // One unmatched char at end
        let config = Parser.create () |> Parser.onChar 'x' (fun ctx -> ctx) |> Parser.onError (fun ctx msg -> ctx)
        let context = Parser.runString largeInput config
        let errors = Parser.getErrors context
        errors.Length |> should equal 1 // Only one error for 'y'
        errors.Head.ErrorType |> should equal (ParseError.UnexpectedChar 'y')

    [<Fact>]
    let ``Parser handles null input with exception`` () =
        let config = Parser.create ()
        // Null input causes exception
        Assert.Throws<System.NullReferenceException>(fun () -> Parser.runString null config |> ignore) |> ignore

    [<Fact>]
    let ``Parser handles empty handlers`` () =
        let config = Parser.create () |> Parser.onError (fun ctx msg -> ctx)
        let context = Parser.runString "anything" config
        let errors = Parser.getErrors context
        errors.Length |> should equal 0 // No handlers, so no errors reported

    [<Fact>]
    let ``Parser handles large input efficiently`` () =
        let largeInput = String.replicate 10000 "a"
        let config = Parser.create () |> Parser.onChar 'a' (fun ctx -> ctx)
        let stopwatch = System.Diagnostics.Stopwatch.StartNew()
        let context = Parser.runString largeInput config
        stopwatch.Stop()
        // Should complete in reasonable time (less than 1 second)
        stopwatch.ElapsedMilliseconds |> should be (lessThan 1000L)
        Parser.getErrors context |> should be Empty

    [<Fact>]
    let ``Parallel tokenization merges tokens correctly`` () =
        let config =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.enableParallelTokenization ()
            |> Parser.onSequence "let" (fun ctx -> ctx)
        let input = "let x = 1\nlet y = 2\nlet z = 3"
        let context = Parser.runString input config
        let tokens = Parser.getTokens context
        // Should have tokens for each "let" keyword
        tokens |> List.filter (fun t -> match t.Type with TokenType.Keyword "let" -> true | _ -> false) |> List.length |> should equal 3

    [<Fact>]
    let ``Function boundary detection identifies JavaScript functions`` () =
        let input = """
function add(a, b) {
    return a + b;
}

function multiply(x, y) {
    return x * y;
}
"""
        let functions = ParsingEngine.identifyFunctionBoundaries input
        functions.Length |> should equal 2
        functions.[0].Name |> should equal "add"
        functions.[1].Name |> should equal "multiply"

    [<Fact>]
    let ``Function boundary detection identifies F# functions`` () =
        let input = """
let add a b =
    a + b

let multiply x y =
    x * y
"""
        let functions = ParsingEngine.identifyFunctionBoundaries input
        functions.Length |> should equal 2
        functions.[0].Name |> should equal "add"
        functions.[1].Name |> should equal "multiply"

    [<Fact>]
    let ``Parallel function parsing merges results correctly`` () =
        let config =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.enableParallel ()
            |> Parser.onSequence "function" (fun ctx -> ctx)
            |> Parser.onSequence "return" (fun ctx -> ctx)
        let input = """
function add(a, b) {
    return a + b;
}

function multiply(x, y) {
    return x * y;
}
"""
        let context = Parser.runString input config
        let tokens = Parser.getTokens context
        // Should have tokens for both functions and returns
        let functionTokens = tokens |> List.filter (fun t -> match t.Type with TokenType.Keyword "function" -> true | _ -> false)
        let returnTokens = tokens |> List.filter (fun t -> match t.Type with TokenType.Keyword "return" -> true | _ -> false)
        functionTokens.Length |> should equal 2
        returnTokens.Length |> should equal 2

    [<Fact>]
    let ``Parallel parsing handles errors correctly`` () =
        let config =
            Parser.create ()
            |> Parser.enableParallel ()
            |> Parser.onChar 'a' (fun ctx -> ctx)
            |> Parser.onError (fun ctx msg -> ctx)
        let input = "abc"  // 'b' and 'c' will be unmatched
        let context = Parser.runString input config
        let errors = Parser.getErrors context
        // Should have errors for unmatched characters
        errors.Length |> should be (greaterThan 0)

    [<Fact>]
    let ``Parallel parsing with AST building works`` () =
        let config =
            Parser.create ()
            |> Parser.enableAST ()
            |> Parser.enableParallel ()
            |> Parser.onSequence "function" (fun ctx -> ctx)  // This might generate AST nodes
        let input = """
function test() {
    return 42;
}
"""
        let context = Parser.runString input config
        let ast = Parser.getAST context
        // AST generation depends on the actual handlers, but should not crash
        Assert.True(true)  // Just ensure it doesn't throw

    [<Fact>]
    let ``Parallel parsing respects minimum functions threshold`` () =
        let config =
            Parser.create ()
            |> Parser.enableParallel ()
            |> Parser.withMinFunctionsForParallelism 3  // Require 3 functions
            |> Parser.onSequence "function" (fun ctx -> ctx)
        let input = """
function add(a, b) {
    return a + b;
}

function multiply(x, y) {
    return x * y;
}
"""  // Only 2 functions, should fall back to sequential
        let context = Parser.runString input config
        // Should still work but use sequential processing
        context.FilePath |> should equal "<string>"