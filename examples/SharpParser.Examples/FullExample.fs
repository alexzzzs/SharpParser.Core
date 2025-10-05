namespace SharpParser.Examples

open SharpParser.Core

/// Comprehensive example using all features: modes, tokenization, AST, error handling, and tracing
module FullExample =
    /// Runs the comprehensive example demonstrating all parser features
    let run () : unit =
        printfn "=== Full Example ==="
        printfn "Demonstrating all parser features: tokenization, AST building, error handling, tracing, and modes"
        printfn "This creates a mini programming language parser with functions, conditionals, and expressions"
        printfn ""

        // Define a custom AST builder for function calls
        // AST builders allow custom logic for creating AST nodes from matched text
        let customFunctionBuilder (mode: string option) (matchedText: string) (context: ParserContext) : ASTNode option =
            // Check if the matched text looks like a function call (contains parentheses)
            if matchedText.Contains("(") && matchedText.Contains(")") then
                // Extract function name from before the opening parenthesis
                let funcName = matchedText.Split('(').[0]
                // Create a function call AST node (simplified - no argument parsing in this example)
                Some (ASTNode.Call (funcName.Trim(), []))
            else
                // Not a function call, let default builders handle it
                None

        // Create a comprehensive parser with all features enabled
        let parser =
            Parser.create ()
            // Enable automatic token generation from matched text
            |> Parser.enableTokens ()
            // Enable automatic AST node construction
            |> Parser.enableAST ()
            // Enable detailed tracing for debugging
            |> Parser.enableTrace true
            // Register the custom AST builder for function calls
            |> Parser.onAST customFunctionBuilder
            // Register a global error handler to print errors as they occur
            |> Parser.onError (fun ctx msg ->
                printfn "ERROR: %s" msg
                ctx)
            // Sequence handlers for keywords - these have highest priority
            |> Parser.onSequence "function" (fun ctx ->
                printfn "Found function definition"
                ParserContextOps.enterMode "functionBody" ctx)  // Enter function context
            |> Parser.onSequence "if" (fun ctx ->
                printfn "Found if statement"
                ParserContextOps.enterMode "ifBody" ctx)  // Enter conditional context
            |> Parser.onSequence "else" (fun ctx ->
                printfn "Found else clause"
                ctx)  // Just acknowledge, no mode change
            |> Parser.onSequence "return" (fun ctx ->
                printfn "Found return statement"
                ctx)
            // Character handlers for operators and punctuation
            |> Parser.onChar '=' (fun ctx ->
                printfn "Found assignment operator"
                ctx)
            |> Parser.onChar '+' (fun ctx ->
                printfn "Found addition operator"
                ctx)
            |> Parser.onChar '-' (fun ctx ->
                printfn "Found subtraction operator"
                ctx)
            |> Parser.onChar '*' (fun ctx ->
                printfn "Found multiplication operator"
                ctx)
            |> Parser.onChar '/' (fun ctx ->
                printfn "Found division operator"
                ctx)
            |> Parser.onChar '{' (fun ctx ->
                printfn "Found opening brace - start of block"
                ctx)
            |> Parser.onChar '}' (fun ctx ->
                printfn "Found closing brace - end of block"
                // Exit mode based on current context (function or if block)
                let currentMode = ParserContextOps.currentMode ctx
                match currentMode with
                | Some "functionBody" -> ParserContextOps.exitMode ctx  // Exit function
                | Some "ifBody" -> ParserContextOps.exitMode ctx        // Exit if block
                | _ -> ctx)  // No mode to exit
            // Pattern handlers for numbers and identifiers
            |> Parser.onPattern @"\d+" (fun ctx matched ->  // One or more digits
                printfn "Found number: %s" matched
                ctx)
            |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx matched ->  // Valid identifier
                printfn "Found identifier: %s" matched
                ctx)
            // Additional punctuation handlers
            |> Parser.onChar '(' (fun ctx ->
                printfn "Found opening parenthesis"
                ctx)
            |> Parser.onChar ')' (fun ctx ->
                printfn "Found closing parenthesis"
                ctx)
            |> Parser.onChar ',' (fun ctx ->
                printfn "Found comma (parameter separator)"
                ctx)

        // Define a complete program in our mini language
        let input = """
function calculate(x, y) {
    result = x + y * 2
    if result > 10 {
        return result
    } else {
        return 0
    }
}

function main() {
    return calculate(5, 3)
}
"""
        printfn "Parsing a complete program with functions, conditionals, and function calls:"
        printfn "%s" input
        printfn "This demonstrates real-world parsing of a small programming language"
        printfn ""

        // Execute the parser on the input program
        let context = Parser.runString input parser

        // Display comprehensive parsing results
        printfn "=== Parsing Results ==="

        // Show tokens generated during parsing (if tokenization was enabled)
        let tokens = Parser.getTokens context
        printfn "Tokens generated: %d" (List.length tokens)
        if List.length tokens <= 20 then // Limit output for readability
            printfn "Token list:"
            tokens |> List.iter (fun token ->
                printfn "  Line %d, Col %d: %A" token.Line token.Col token.Type)
        else
            printfn "  (Too many tokens to display - showing first 20)"
            tokens |> List.take 20 |> List.iter (fun token ->
                printfn "  Line %d, Col %d: %A" token.Line token.Col token.Type)

        // Show AST nodes built during parsing (if AST building was enabled)
        let astNodes = Parser.getAST context
        printfn "AST nodes created: %d" (List.length astNodes)
        printfn "AST represents the syntactic structure of the parsed program"

        // Report any errors that occurred during parsing
        let errors = Parser.getErrors context
        printfn "Errors encountered: %d" (List.length errors)
        if not (List.isEmpty errors) then
            printfn "Error details:"
            errors |> List.iter (fun errorInfo ->
                printfn "  Line %d, Col %d: %s" errorInfo.Line errorInfo.Col errorInfo.Message
                match errorInfo.Suggestion with
                | Some suggestion -> printfn "    Suggestion: %s" suggestion
                | None -> ())

        // Show trace information (if tracing was enabled)
        let trace = Parser.getTrace context
        printfn "Trace messages logged: %d" (List.length trace)
        printfn "Tracing shows the step-by-step parsing process for debugging"

        // Final parsing state summary
        printfn ""
        printfn "Final parsing state:"
        printfn "  Position: Line %d, Col %d" context.Line context.Col
        printfn "  Current mode: %s" (ParserContextOps.currentMode context |> Option.defaultValue "none")
        printfn "  Mode stack depth: %d" (List.length context.ModeStack)

        // Success/failure assessment
        if List.isEmpty errors then
            printfn "✓ Parsing completed successfully - all input processed without errors"
        else
            printfn "✗ Parsing completed with errors - check error details above"

        printfn ""