namespace SharpParser.Examples

open SharpParser.Core

/// Example demonstrating parsing modes for context-sensitive parsing
module ModeExample =
    /// Runs the mode example demonstrating context-sensitive parsing
    let run () : unit =
        printfn "=== Mode Example ==="
        printfn "Demonstrating context-sensitive parsing with modes"
        printfn "Modes allow different parsing rules in different contexts (e.g., inside functions vs global scope)"
        printfn ""

        // Create a parser that uses modes to handle nested structures
        // Modes are pushed/popped from a stack to track parsing context
        let parser =
            Parser.create ()
            // Global handler: when "function" is encountered, enter function body mode
            |> Parser.onSequence "function" (fun ctx ->
                printfn "Found function keyword at line %d, col %d" ctx.Line ctx.Col
                ParserContextOps.enterMode "functionBody" ctx)  // Push "functionBody" mode onto stack
            // Define handlers that only apply when in "functionBody" mode
            |> Parser.inMode "functionBody" (fun config ->
                config
                // In function body, '{' starts a block
                |> Parser.onChar '{' (fun ctx ->
                    printfn "Start function block at line %d, col %d" ctx.Line ctx.Col
                    ctx)
                // In function body, '}' ends the function and exits the mode
                |> Parser.onChar '}' (fun ctx ->
                    printfn "End function block at line %d, col %d" ctx.Line ctx.Col
                    ParserContextOps.exitMode ctx)  // Pop mode from stack
                // Nested mode: "if" statements inside functions enter "ifBody" mode
                |> Parser.onSequence "if" (fun ctx ->
                    printfn "Found if statement at line %d, col %d" ctx.Line ctx.Col
                    ParserContextOps.enterMode "ifBody" ctx)  // Push another mode onto stack
                // Define handlers for "ifBody" mode (nested inside "functionBody")
                |> Parser.inMode "ifBody" (fun ifConfig ->
                    ifConfig
                    |> Parser.onChar '{' (fun ctx ->
                        printfn "Start if block at line %d, col %d" ctx.Line ctx.Col
                        ctx)
                    |> Parser.onChar '}' (fun ctx ->
                        printfn "End if block at line %d, col %d" ctx.Line ctx.Col
                        ParserContextOps.exitMode ctx)))  // Pop "ifBody" mode

        // Sample input with nested structures: function containing if statement
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
        printfn "Parsing input with nested function and if blocks:"
        printfn "%s" input
        printfn "Notice how modes change the parsing behavior in different contexts"
        printfn ""

        let context = Parser.runString input parser

        // Examine the results, including mode information
        printfn "Parsing completed!"
        printfn "Final position: Line %d, Col %d" context.Line context.Col
        printfn "Final mode: %s" (ParserContextOps.currentMode context |> Option.defaultValue "none")
        printfn "Mode stack depth: %d (should be 0 if all blocks closed properly)" (List.length context.ModeStack)
        let errors = Parser.getErrors context
        printfn "Errors: %d" (List.length errors)

        if List.isEmpty errors then
            printfn "✓ No errors found - all structures properly nested"
        else
            printfn "✗ Errors found:"
            errors |> List.iter (fun errorInfo ->
                printfn "  Line %d, Col %d: %s" errorInfo.Line errorInfo.Col errorInfo.Message)

        printfn ""