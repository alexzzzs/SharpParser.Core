namespace SharpParser.Examples

open SharpParser.Core

/// Example demonstrating parsing modes for context-sensitive parsing
module ModeExample =
    /// Runs the mode example
    let run () : unit =
        printfn "=== Mode Example ==="
        printfn "Demonstrating context-sensitive parsing with modes"
        printfn ""

        // Create a parser with mode-based handlers
        let parser =
            Parser.create ()
            |> Parser.onSequence "function" (fun ctx ->
                printfn "Found function keyword at line %d, col %d" ctx.Line ctx.Col
                ParserContextOps.enterMode "functionBody" ctx)
            |> Parser.inMode "functionBody" (fun config ->
                config
                |> Parser.onChar '{' (fun ctx ->
                    printfn "Start function block at line %d, col %d" ctx.Line ctx.Col
                    ctx)
                |> Parser.onChar '}' (fun ctx ->
                    printfn "End function block at line %d, col %d" ctx.Line ctx.Col
                    ParserContextOps.exitMode ctx)
                |> Parser.onSequence "if" (fun ctx ->
                    printfn "Found if statement at line %d, col %d" ctx.Line ctx.Col
                    ParserContextOps.enterMode "ifBody" ctx)
                |> Parser.inMode "ifBody" (fun ifConfig ->
                    ifConfig
                    |> Parser.onChar '{' (fun ctx ->
                        printfn "Start if block at line %d, col %d" ctx.Line ctx.Col
                        ctx)
                    |> Parser.onChar '}' (fun ctx ->
                        printfn "End if block at line %d, col %d" ctx.Line ctx.Col
                        ParserContextOps.exitMode ctx)))

        // Parse a sample program with nested function and if blocks
        let input = """
function calculate(x, y) {
    result = x + y * 2
    if result > 10 {
        return result
    } else {
        return 0
    }
}
"""
        printfn "Parsing:"
        printfn "%s" input
        printfn ""

        let context = Parser.runString input parser

        // Print results
        printfn "Parsing completed!"
        printfn "Final position: Line %d, Col %d" context.Line context.Col
        printfn "Final mode: %s" (ParserContextOps.currentMode context |> Option.defaultValue "none")
        printfn "Mode stack depth: %d" (List.length context.ModeStack)
        let errors = Parser.getErrors context
        printfn "Errors: %d" (List.length errors)

        if List.isEmpty errors then
            printfn "✓ No errors found"
        else
            printfn "✗ Errors found:"
            errors |> List.iter (fun errorInfo ->
                printfn "  Line %d, Col %d: %s" errorInfo.Line errorInfo.Col errorInfo.Message)

        printfn ""