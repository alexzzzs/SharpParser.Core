namespace SharpParser.Examples

open SharpParser.Core

/// CSV parsing example demonstrating pattern matching and state management
module CsvExample =
    /// Runs the CSV parsing example
    let run () : unit =
        printfn "=== CSV Parsing Example ==="
        printfn "Demonstrating delimited data parsing with proper escaping"
        printfn "This example parses CSV data with quoted fields"
        printfn ""

        // Create parser for CSV format
        let parser =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.onError (fun ctx msg ->
                printfn "Parse error: %s" msg
                ctx)

            // Handle quoted fields (fields containing commas or quotes)
            |> Parser.onPattern @"""(?:[^""]|"""")*""" (fun ctx matched ->
                let unescaped = matched.Trim('"').Replace("\"\"", "\"")
                printfn "Found quoted field: '%s'" unescaped
                ctx)

            // Handle unquoted fields (no commas or quotes)
            |> Parser.onPattern @"[^,\r\n]+" (fun ctx matched ->
                printfn "Found field: '%s'" matched
                ctx)

            // Handle field separators
            |> Parser.onChar ',' (fun ctx ->
                printfn "Field separator"
                ctx)

            // Handle newlines (end of record)
            |> Parser.onChar '\n' (fun ctx ->
                printfn "End of record"
                ctx)
            |> Parser.onChar '\r' (fun ctx ->
                // Handle \r\n sequences
                ctx)

        // Sample CSV data with various edge cases
        let csvData = "Name,Age,City,Occupation\n" +
                      "John Doe,30,\"New York, NY\",Software Engineer\n" +
                      "\"Jane \"\"Smith\"\"\",25,Los Angeles,\"Data Scientist\"\n" +
                      "Bob Johnson,35,Chicago,Designer\n" +
                      "Alice Brown,28,\"Austin, TX\",Manager\n"

        printfn "Parsing CSV data:"
        printfn "%s" csvData
        printfn ""

        // Parse the CSV
        let context = Parser.runString csvData parser

        // Display results
        let tokens = Parser.getTokens context
        let errors = Parser.getErrors context

        printfn "Parsing completed!"
        printfn "Tokens found: %d" (List.length tokens)
        printfn "Errors: %d" (List.length errors)

        if not (List.isEmpty tokens) then
            printfn ""
            printfn "All tokens:"
            tokens |> List.iter (fun token ->
                printfn "  %A" token)

        if not (List.isEmpty errors) then
            printfn ""
            printfn "Errors:"
            errors |> List.iter (fun error ->
                printfn "  Line %d, Col %d: %s" error.Line error.Col error.Message)

        printfn ""