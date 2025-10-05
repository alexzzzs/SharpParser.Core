namespace SharpParser.Examples

open SharpParser.Core

/// Basic example demonstrating simple character and sequence handlers
module BasicExample =
    /// Runs the basic example demonstrating fundamental parser concepts
    let run () : unit =
        printfn "=== Basic Example ==="
        printfn "Demonstrating simple arithmetic expression parsing"
        printfn "This example shows how to handle individual characters and patterns"
        printfn ""

        // Create a basic parser configuration starting from scratch
        // Parser.create() returns a minimal configuration with no handlers
        let parser =
            Parser.create ()
            // Register character handlers for arithmetic operators
            // Each handler receives the current parsing context and returns an updated context
            // Here we just print information and return the context unchanged
            |> Parser.onChar '+' (fun ctx ->
                printfn "Found addition operator at line %d, col %d" ctx.Line ctx.Col
                ctx)  // Return context unchanged
            |> Parser.onChar '-' (fun ctx ->
                printfn "Found subtraction operator at line %d, col %d" ctx.Line ctx.Col
                ctx)
            |> Parser.onChar '*' (fun ctx ->
                printfn "Found multiplication operator at line %d, col %d" ctx.Line ctx.Col
                ctx)
            |> Parser.onChar '/' (fun ctx ->
                printfn "Found division operator at line %d, col %d" ctx.Line ctx.Col
                ctx)
            // Register pattern handlers for more complex matches
            // Pattern handlers receive both context and the matched text
            |> Parser.onPattern @"\d+" (fun ctx matched ->  // Regex for one or more digits
                printfn "Found number '%s' at line %d, col %d" matched ctx.Line ctx.Col
                ctx)
            |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx matched ->  // Regex for identifiers
                printfn "Found identifier '%s' at line %d, col %d" matched ctx.Line ctx.Col
                ctx)

        // Define input to parse - a simple arithmetic expression
        let input = "x + 42 * y - 10 / z"
        printfn "Parsing input: %s" input
        printfn "Expected matches: identifier 'x', operator '+', number '42', operator '*', etc."
        printfn ""

        // Run the parser on the input string
        // This processes the entire string and returns the final parsing context
        let context = Parser.runString input parser

        // Examine the parsing results
        printfn "Parsing completed!"
        printfn "Final position: Line %d, Col %d" context.Line context.Col
        let errors = Parser.getErrors context
        printfn "Errors encountered: %d" (List.length errors)

        // Report success or failure
        if List.isEmpty errors then
            printfn "✓ No errors found - all input was successfully processed"
        else
            printfn "✗ Errors found during parsing:"
            errors |> List.iter (fun errorInfo ->
                printfn "  Line %d, Col %d: %s" errorInfo.Line errorInfo.Col errorInfo.Message)

        printfn ""