# SharpParser.Core Quick Start Guide

## What is SharpParser.Core?

SharpParser.Core is a beginner-friendly, event-driven F# parsing library that makes it easy to build parsers for custom languages, configuration files, or domain-specific formats.

## Your First Parser

Let's create a simple parser that recognizes basic arithmetic expressions:

```fsharp
open SharpParser.Core

// Create a parser configuration
let parser =
    Parser.create ()
    |> Parser.onChar '+' (fun ctx ->
        printfn "Found addition operator at line %d, column %d" ctx.Line ctx.Col
        ctx)
    |> Parser.onChar '-' (fun ctx ->
        printfn "Found subtraction operator at line %d, column %d" ctx.Line ctx.Col
        ctx)
    |> Parser.onPattern @"\d+" (fun ctx number ->
        printfn "Found number: %s" number
        ctx)
    |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx identifier ->
        printfn "Found identifier: %s" identifier
        ctx)

// Parse some input
let context = Parser.runString "x + 42 * y - 10" parser

printfn "Parsing completed!"
```

## Adding Context Sensitivity with Modes

Modes allow your parser to behave differently in different contexts:

```fsharp
let parser =
    Parser.create ()
    |> Parser.onSequence "function" (fun ctx ->
        printfn "Found function definition"
        ParserContextOps.enterMode "functionBody" ctx)
    |> Parser.inMode "functionBody" (fun config ->
        config
        |> Parser.onChar '{' (fun ctx ->
            printfn "Function body starts"
            ctx)
        |> Parser.onChar '}' (fun ctx ->
            printfn "Function body ends"
            ParserContextOps.exitMode ctx)
        |> Parser.onSequence "if" (fun ctx ->
            printfn "Found if statement in function"
            ParserContextOps.enterMode "ifBody" ctx)
        |> Parser.inMode "ifBody" (fun ifConfig ->
            ifConfig
            |> Parser.onChar '{' (fun ctx -> printfn "If block starts"; ctx)
            |> Parser.onChar '}' (fun ctx -> printfn "If block ends"; ParserContextOps.exitMode ctx)))

let context = Parser.runString """
function calculate(x, y) {
    if x > 0 {
        return x + y
    }
}
""" parser
```

## Enabling Advanced Features

SharpParser.Core provides optional features for more advanced parsing:

```fsharp
let parser =
    Parser.create ()
    |> Parser.enableTokens ()  // Collect tokens automatically
    |> Parser.enableAST ()     // Build AST nodes automatically
    |> Parser.enableTrace true // Log parsing steps for debugging
    |> Parser.onError (fun ctx msg ->
        printfn "ERROR: %s" msg
        ctx)

// Parse and get results
let context = Parser.runString "function test() { return 42 }" parser

// Extract different types of results
let tokens = Parser.getTokens context    // List of Token
let astNodes = Parser.getAST context     // List of ASTNode
let errors = Parser.getErrors context    // List of (line, col, message)
let traces = Parser.getTrace context     // List of trace messages
```

## Handler Types Explained

### Character Handlers
Respond to individual characters:

```fsharp
Parser.onChar '+' (fun ctx -> printfn "Plus sign!"; ctx)
Parser.onChar '(' (fun ctx -> printfn "Open parenthesis"; ctx)
```

### Sequence Handlers
Respond to multi-character sequences:

```fsharp
Parser.onSequence "function" (fun ctx -> printfn "Function keyword!"; ctx)
Parser.onSequence "==" (fun ctx -> printfn "Equals operator!"; ctx)
```

### Pattern Handlers
Respond to regular expression patterns:

```fsharp
Parser.onPattern @"\d+" (fun ctx number -> printfn "Number: %s" number; ctx)
Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx id -> printfn "Identifier: %s" id; ctx)
```

## Common Patterns

### Numbers
```fsharp
Parser.onPattern @"-?\d+(\.\d+)?" (fun ctx number ->
    match System.Double.TryParse number with
    | true, value -> printfn "Number: %f" value; ctx
    | false, _ -> printfn "Invalid number: %s" number; ctx)
```

### Strings
```fsharp
Parser.onPattern "\"([^\"\\\\]|\\\\.)*\"" (fun ctx str ->
    let content = str.Substring(1, str.Length - 2)
    printfn "String: %s" content
    ctx)
```

### Identifiers
```fsharp
Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx id ->
    printfn "Identifier: %s" id
    ctx)
```

## Error Handling

```fsharp
let parser =
    Parser.create ()
    |> Parser.onError (fun ctx msg ->
        printfn "Parse error at line %d, col %d: %s" ctx.Line ctx.Col msg
        ctx)

// Check for errors after parsing
let context = Parser.runString "invalid input +++" parser
if not (List.isEmpty (Parser.getErrors context)) then
    printfn "Parsing failed!"
else
    printfn "Parsing successful!"
```

## Debugging with Tracing

```fsharp
let parser =
    Parser.create ()
    |> Parser.enableTrace true

let context = Parser.runString "function test() { return 42 }" parser

// Print trace log
Parser.getTrace context
|> List.iter (fun traceMsg -> printfn "TRACE: %s" traceMsg)
```

## Real-World Example: Simple Language Parser

Here's a more complete example parsing a simple programming language:

```fsharp
let parser =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.enableAST ()
    |> Parser.onError (fun ctx msg -> printfn "ERROR: %s" msg; ctx)

    // Keywords
    |> Parser.onSequence "function" (fun ctx ->
        printfn "Function definition"
        ParserContextOps.enterMode "functionBody" ctx)
    |> Parser.onSequence "if" (fun ctx ->
        printfn "If statement"
        ParserContextOps.enterMode "ifBody" ctx)
    |> Parser.onSequence "return" (fun ctx ->
        printfn "Return statement"
        ctx)

    // Operators and punctuation
    |> Parser.onChar '=' (fun ctx -> printfn "Assignment"; ctx)
    |> Parser.onChar '+' (fun ctx -> printfn "Addition"; ctx)
    |> Parser.onChar '{' (fun ctx -> printfn "Block start"; ctx)
    |> Parser.onChar '}' (fun ctx ->
        printfn "Block end"
        let currentMode = ParserContextOps.currentMode ctx
        match currentMode with
        | Some "functionBody" -> ParserContextOps.exitMode ctx
        | Some "ifBody" -> ParserContextOps.exitMode ctx
        | _ -> ctx)

    // Literals and identifiers
    |> Parser.onPattern @"\d+" (fun ctx num -> printfn "Number: %s" num; ctx)
    |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx id -> printfn "Identifier: %s" id; ctx)

let context = Parser.runString """
function calculate(x, y) {
    if x > 0 {
        return x + y
    }
    return 0
}
""" parser

// Use the results
let tokens = Parser.getTokens context
let ast = Parser.getAST context
let errors = Parser.getErrors context
```

## Next Steps

1. **Explore Examples**: Check out the `examples/` directory for more comprehensive examples
2. **Read API Docs**: See `docs/API.md` for detailed API reference
3. **Study Architecture**: Read `docs/Architecture.md` to understand the design
4. **Experiment**: Try modifying the examples to parse your own formats
5. **Extend**: Add custom token types or AST nodes for your domain

## Getting Help

- **API Documentation**: `docs/API.md`
- **Architecture Guide**: `docs/Architecture.md`
- **Examples**: `examples/` directory
- **Issues**: Check the project repository for known issues and solutions

Happy parsing! 🚀