namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module ParsingEngineTests =

    [<Fact>]
    let ``ParsingEngine can parse simple string`` () =
        let config = Parser.create () |> Parser.onChar 'a' (fun ctx -> ctx)
        let context = ParsingEngine.parseString config "test"
        context.FilePath |> should equal "<string>"
        context.Line |> should equal 2  // Engine increments line after processing
        context.Col |> should equal 1

    [<Fact>]
    let ``ParsingEngine handles file not found`` () =
        let config = Parser.create ()
        let context = ParsingEngine.parseFile config "nonexistent.txt"
        let errors = ErrorHandling.getErrors context
        errors.Length |> should be (greaterThan 0)
        match errors.Head.ErrorType with
        | ParseError.ConfigError _ -> Assert.True(true)
        | _ -> Assert.True(false, "Expected ConfigError")

    [<Fact>]
    let ``ParsingEngine handles empty input`` () =
        let config = Parser.create ()
        let context = ParsingEngine.parseString config ""
        context.Line |> should equal 1
        context.Col |> should equal 1

    [<Fact>]
    let ``ParsingEngine handles null input with exception`` () =
        let config = Parser.create ()
        Assert.Throws<System.NullReferenceException>(fun () -> ParsingEngine.parseString config null |> ignore) |> ignore

    [<Fact>]
    let ``ParsingEngine handles very long input`` () =
        let longInput = String.replicate 10000 "a"
        let config = Parser.create () |> Parser.onChar 'a' (fun ctx -> ctx)
        let stopwatch = System.Diagnostics.Stopwatch.StartNew()
        let context = ParsingEngine.parseString config longInput
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> should be (lessThan 2000L) // Should be reasonably fast

    [<Fact>]
    let ``ParsingEngine handles multiline input`` () =
        let multiline = "line1\nline2\nline3"
        let config = Parser.create () |> Parser.onChar 'l' (fun ctx -> ctx)
        let context = ParsingEngine.parseString config multiline
        context.Line |> should equal 4 // After 3 lines + increment
        context.Col |> should equal 1

    [<Fact>]
    let ``ParsingEngine handles input with only newlines`` () =
        let newlines = "\n\n\n"
        let config = Parser.create ()
        let context = ParsingEngine.parseString config newlines
        context.Line |> should equal 1 // Empty lines don't increment line number
        context.Col |> should equal 1