namespace SharpParser.Examples

open SharpParser.Core

/// JSON parsing example demonstrating mode-based parsing for structured data
module JsonExample =
    /// Runs the JSON parsing example
    let run () : unit =
        printfn "=== JSON Parsing Example ==="
        printfn "Demonstrating structured data parsing with modes"
        printfn "This example parses a simplified JSON-like format"
        printfn ""

        // Create parser with tokenization and AST building enabled
        let parser =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.enableAST ()
            |> Parser.onError (fun ctx msg ->
                printfn "Parse error: %s" msg
                ctx)

            // Handle string literals (simplified - no escape sequences)
            |> Parser.onPattern @"""[^""]*""" (fun ctx matched ->
                printfn "Found string: %s" matched
                ctx)

            // Handle numbers
            |> Parser.onPattern @"-?\d+(\.\d+)?" (fun ctx matched ->
                printfn "Found number: %s" matched
                ctx)

            // Handle boolean literals
            |> Parser.onSequence "true" (fun ctx ->
                printfn "Found boolean: true"
                ctx)
            |> Parser.onSequence "false" (fun ctx ->
                printfn "Found boolean: false"
                ctx)

            // Handle null
            |> Parser.onSequence "null" (fun ctx ->
                printfn "Found null"
                ctx)

            // Handle object start
            |> Parser.onChar '{' (fun ctx ->
                printfn "Starting object"
                ParserContextOps.enterMode "object" ctx)

            // Handle object end
            |> Parser.onChar '}' (fun ctx ->
                printfn "Ending object"
                ParserContextOps.exitMode ctx)

            // Handle array start
            |> Parser.onChar '[' (fun ctx ->
                printfn "Starting array"
                ParserContextOps.enterMode "array" ctx)

            // Handle array end
            |> Parser.onChar ']' (fun ctx ->
                printfn "Ending array"
                ParserContextOps.exitMode ctx)

            // Handle commas and colons (structural characters)
            |> Parser.onChar ',' (fun ctx ->
                printfn "Found comma"
                ctx)
            |> Parser.onChar ':' (fun ctx ->
                printfn "Found colon"
                ctx)

            // Skip whitespace
            |> Parser.onPattern @"\s+" (fun ctx _ -> ctx)

        // Sample JSON input
        let jsonInput = """
        {
            "name": "John Doe",
            "age": 30,
            "isStudent": false,
            "grades": [85, 92, 78],
            "address": {
                "street": "123 Main St",
                "city": "Anytown",
                "zipCode": "12345"
            },
            "metadata": null
        }
        """

        printfn "Parsing JSON input:"
        printfn "%s" jsonInput
        printfn ""

        // Parse the JSON
        let context = Parser.runString jsonInput parser

        // Display results
        let tokens = Parser.getTokens context
        let ast = Parser.getAST context
        let errors = Parser.getErrors context

        printfn "Parsing completed!"
        printfn "Tokens found: %d" (List.length tokens)
        printfn "AST nodes: %d" (List.length ast)
        printfn "Errors: %d" (List.length errors)

        if not (List.isEmpty tokens) then
            printfn ""
            printfn "Sample tokens:"
            tokens |> List.take 10 |> List.iter (fun token ->
                printfn "  %A" token)

        if not (List.isEmpty errors) then
            printfn ""
            printfn "Errors:"
            errors |> List.iter (fun error ->
                printfn "  Line %d, Col %d: %s" error.Line error.Col error.Message)

        printfn ""