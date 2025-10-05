namespace SharpParser.Examples

open SharpParser.Core

/// Basic example demonstrating simple character and sequence handlers
module BasicExample =
    /// Runs the basic example
    let run () : unit =
        printfn "=== Basic Example ==="
        printfn "Demonstrating simple arithmetic expression parsing"
        printfn ""

        // Create a parser with basic handlers
        let parser =
            Parser.create ()
            |> Parser.onChar '+' (fun ctx ->
                printfn "Found addition operator at line %d, col %d" ctx.Line ctx.Col
                ctx)
            |> Parser.onChar '-' (fun ctx ->
                printfn "Found subtraction operator at line %d, col %d" ctx.Line ctx.Col
                ctx)
            |> Parser.onChar '*' (fun ctx ->
                printfn "Found multiplication operator at line %d, col %d" ctx.Line ctx.Col
                ctx)
            |> Parser.onChar '/' (fun ctx ->
                printfn "Found division operator at line %d, col %d" ctx.Line ctx.Col
                ctx)
            |> Parser.onPattern @"\d+" (fun ctx matched ->
                printfn "Found number '%s' at line %d, col %d" matched ctx.Line ctx.Col
                ctx)
            |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx matched ->
                printfn "Found identifier '%s' at line %d, col %d" matched ctx.Line ctx.Col
                ctx)

        // Parse a simple arithmetic expression
        let input = "x + 42 * y - 10 / z"
        printfn "Parsing: %s" input
        printfn ""

        let context = Parser.runString input parser

        // Print results
        printfn "Parsing completed!"
        printfn "Final position: Line %d, Col %d" context.Line context.Col
        let errors = Parser.getErrors context
        printfn "Errors: %d" (List.length errors)

        if List.isEmpty errors then
            printfn "✓ No errors found"
        else
            printfn "✗ Errors found:"
            errors |> List.iter (fun errorInfo ->
                printfn "  Line %d, Col %d: %s" errorInfo.Line errorInfo.Col errorInfo.Message)

        printfn ""