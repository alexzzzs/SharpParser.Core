namespace SharpParser.Examples

open SharpParser.Core

/// Comprehensive example using all features: modes, tokenization, AST, error handling, and tracing
module FullExample =
    /// Runs the full example
    let run () : unit =
        printfn "=== Full Example ==="
        printfn "Demonstrating all parser features with a simple programming language"
        printfn ""

        // Custom AST builder for function calls
        let customFunctionBuilder (mode: string option) (matchedText: string) (context: ParserContext) : ASTNode option =
            if matchedText.Contains("(") && matchedText.Contains(")") then
                // Parse function call like "calculate(x, y)"
                let funcName = matchedText.Split('(').[0]
                Some (ASTNode.Call (funcName.Trim(), [])) // Simplified - no argument parsing
            else
                None

        // Create a parser with all features enabled
        let parser =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.enableAST ()
            |> Parser.enableTrace true
            |> Parser.onAST customFunctionBuilder  // Register custom AST builder
            |> Parser.onError (fun ctx msg ->
                printfn "ERROR: %s" msg
                ctx)
            |> Parser.onSequence "function" (fun ctx ->
                printfn "Found function definition"
                ParserContextOps.enterMode "functionBody" ctx)
            |> Parser.onSequence "if" (fun ctx ->
                printfn "Found if statement"
                ParserContextOps.enterMode "ifBody" ctx)
            |> Parser.onSequence "else" (fun ctx ->
                printfn "Found else clause"
                ctx)
            |> Parser.onSequence "return" (fun ctx ->
                printfn "Found return statement"
                ctx)
            |> Parser.onChar '=' (fun ctx ->
                printfn "Found assignment"
                ctx)
            |> Parser.onChar '+' (fun ctx ->
                printfn "Found addition"
                ctx)
            |> Parser.onChar '-' (fun ctx ->
                printfn "Found subtraction"
                ctx)
            |> Parser.onChar '*' (fun ctx ->
                printfn "Found multiplication"
                ctx)
            |> Parser.onChar '/' (fun ctx ->
                printfn "Found division"
                ctx)
            |> Parser.onChar '{' (fun ctx ->
                printfn "Found opening brace"
                ctx)
            |> Parser.onChar '}' (fun ctx ->
                printfn "Found closing brace"
                let currentMode = ParserContextOps.currentMode ctx
                match currentMode with
                | Some "functionBody" -> ParserContextOps.exitMode ctx
                | Some "ifBody" -> ParserContextOps.exitMode ctx
                | _ -> ctx)
            |> Parser.onPattern @"\d+" (fun ctx matched ->
                printfn "Found number: %s" matched
                ctx)
            |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx matched ->
                printfn "Found identifier: %s" matched
                ctx)
            |> Parser.onChar '(' (fun ctx ->
                printfn "Found opening parenthesis"
                ctx)
            |> Parser.onChar ')' (fun ctx ->
                printfn "Found closing parenthesis"
                ctx)
            |> Parser.onChar ',' (fun ctx ->
                printfn "Found comma"
                ctx)

        // Parse a complete sample program
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
        printfn "Parsing program:"
        printfn "%s" input
        printfn ""

        let context = Parser.runString input parser

        // Print comprehensive results
        printfn "=== Results ==="

        // Tokens
        let tokens = Parser.getTokens context
        printfn "Tokens found: %d" (List.length tokens)
        if List.length tokens <= 20 then // Only show if not too many
            tokens |> List.iter (fun token ->
                printfn "  %d:%d %A" token.Line token.Col token.Type)

        // AST Nodes
        let astNodes = Parser.getAST context
        printfn "AST nodes: %d" (List.length astNodes)

        // Errors
        let errors = Parser.getErrors context
        printfn "Errors: %d" (List.length errors)
        if not (List.isEmpty errors) then
            errors |> List.iter (fun errorInfo ->
                printfn "  Line %d, Col %d: %s" errorInfo.Line errorInfo.Col errorInfo.Message
                match errorInfo.Suggestion with
                | Some suggestion -> printfn "    Suggestion: %s" suggestion
                | None -> ())

        // Trace
        let trace = Parser.getTrace context
        printfn "Trace messages: %d" (List.length trace)

        // Summary
        printfn ""
        printfn "Final position: Line %d, Col %d" context.Line context.Col
        printfn "Final mode: %s" (ParserContextOps.currentMode context |> Option.defaultValue "none")

        if List.isEmpty errors then
            printfn "✓ Parsing successful!"
        else
            printfn "✗ Parsing had errors"

        printfn ""