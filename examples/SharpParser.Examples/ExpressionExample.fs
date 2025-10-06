namespace SharpParser.Examples

open SharpParser.Core

/// Arithmetic expression parsing example demonstrating operator precedence and AST building
module ExpressionExample =
    /// Runs the expression parsing example
    let run () : unit =
        printfn "=== Arithmetic Expression Parsing Example ==="
        printfn "Demonstrating operator precedence and AST construction"
        printfn "This example parses expressions like '2 + 3 * 4 - (5 / 2)'"
        printfn ""

        // Create parser with AST building enabled
        let parser =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.enableAST ()
            |> Parser.onError (fun ctx msg ->
                printfn "Parse error: %s" msg
                ctx)

            // Handle numbers
            |> Parser.onPattern @"\d+(\.\d+)?" (fun ctx matched ->
                printfn "Found number: %s" matched
                ctx)

            // Handle identifiers (variables)
            |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx matched ->
                printfn "Found identifier: %s" matched
                ctx)

            // Handle parentheses for grouping
            |> Parser.onChar '(' (fun ctx ->
                printfn "Opening parenthesis"
                ParserContextOps.enterMode "group" ctx)
            |> Parser.onChar ')' (fun ctx ->
                printfn "Closing parenthesis"
                ParserContextOps.exitMode ctx)

            // Handle operators with precedence (higher precedence operators first)
            |> Parser.onChar '*' (fun ctx ->
                printfn "Multiplication operator"
                ctx)
            |> Parser.onChar '/' (fun ctx ->
                printfn "Division operator"
                ctx)
            |> Parser.onChar '+' (fun ctx ->
                printfn "Addition operator"
                ctx)
            |> Parser.onChar '-' (fun ctx ->
                printfn "Subtraction operator"
                ctx)

            // Skip whitespace
            |> Parser.onPattern @"\s+" (fun ctx _ -> ctx)

        // Sample expressions to parse
        let expressions = [
            "2 + 3"
            "x * y + 5"
            "2 + 3 * 4"
            "(2 + 3) * 4"
            "a + b * c - d / e"
            "x = 42 + (10 * 2)"
        ]

        expressions |> List.iteri (fun i expr ->
            printfn "Expression %d: %s" (i + 1) expr

            // Parse each expression
            let context = Parser.runString expr parser

            // Display results
            let tokens = Parser.getTokens context
            let ast = Parser.getAST context
            let errors = Parser.getErrors context

            printfn "  Tokens: %d" (List.length tokens)
            printfn "  AST nodes: %d" (List.length ast)
            printfn "  Errors: %d" (List.length errors)

            if not (List.isEmpty tokens) then
                printfn "  Sample tokens:"
                tokens |> List.take 5 |> List.iter (fun token ->
                    printfn "    %A" token)

            if not (List.isEmpty ast) then
                printfn "  AST:"
                ast |> List.iter (fun node ->
                    printfn "    %A" node)

            if not (List.isEmpty errors) then
                printfn "  Errors:"
                errors |> List.iter (fun error ->
                    printfn "    Line %d, Col %d: %s" error.Line error.Col error.Message)

            printfn ""
        )

        printfn "Note: This example demonstrates tokenization and basic AST building."
        printfn "A full expression parser would need proper precedence handling and tree construction."
        printfn ""