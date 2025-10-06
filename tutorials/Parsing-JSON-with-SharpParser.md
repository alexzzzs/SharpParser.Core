# Tutorial: Parsing JSON with SharpParser

This tutorial will guide you through building a JSON parser using SharpParser.Core. We'll create a parser that can handle basic JSON structures including objects, arrays, strings, numbers, booleans, and null values.

## Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- SharpParser.Core NuGet package
- Basic understanding of F#

## Step 1: Project Setup

Create a new F# console application:

```bash
dotnet new console -lang F# -n JsonParserTutorial
cd JsonParserTutorial
dotnet add package SharpParser.Core
```

## Step 2: Understanding JSON Structure

JSON has these basic elements:
- **Objects**: `{"key": "value", "key2": 123}`
- **Arrays**: `[1, 2, 3, "hello"]`
- **Strings**: `"Hello World"`
- **Numbers**: `123`, `45.67`
- **Booleans**: `true`, `false`
- **Null**: `null`

## Step 3: Basic Parser Setup

Let's start with a basic parser that can recognize JSON values:

```fsharp
open SharpParser.Core

// Create a parser for basic JSON values
let createBasicJsonParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onError (fun ctx msg ->
        printfn "Parse error: %s" msg
        ctx)
```

## Step 4: Handling Primitive Values

Add handlers for strings, numbers, booleans, and null:

```fsharp
let createBasicJsonParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onError (fun ctx msg ->
        printfn "Parse error: %s" msg
        ctx)

    // Handle string literals (simplified - no escape sequences)
    |> Parser.onPattern @"""[^""]*""" (fun ctx matched ->
        printfn "Found string: %s" matched
        ctx)

    // Handle numbers (integers and floats)
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
```

## Step 5: Handling Objects and Arrays

JSON objects and arrays require context-sensitive parsing. We'll use SharpParser's mode system:

```fsharp
let createJsonParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onError (fun ctx msg ->
        printfn "Parse error: %s" msg
        ctx)

    // Primitive value handlers (same as above)
    |> Parser.onPattern @"""[^""]*""" (fun ctx matched ->
        printfn "Found string: %s" matched
        ctx)
    |> Parser.onPattern @"-?\d+(\.\d+)?" (fun ctx matched ->
        printfn "Found number: %s" matched
        ctx)
    |> Parser.onSequence "true" (fun ctx ->
        printfn "Found boolean: true"
        ctx)
    |> Parser.onSequence "false" (fun ctx ->
        printfn "Found boolean: false"
        ctx)
    |> Parser.onSequence "null" (fun ctx ->
        printfn "Found null"
        ctx)

    // Object parsing with modes
    |> Parser.onChar '{' (fun ctx ->
        printfn "Starting object"
        ParserContextOps.enterMode "object" ctx)
    |> Parser.onChar '}' (fun ctx ->
        printfn "Ending object"
        ParserContextOps.exitMode ctx)

    // Array parsing with modes
    |> Parser.onChar '[' (fun ctx ->
        printfn "Starting array"
        ParserContextOps.enterMode "array" ctx)
    |> Parser.onChar ']' (fun ctx ->
        printfn "Ending array"
        ParserContextOps.exitMode ctx)

    // Structural characters
    |> Parser.onChar ',' (fun ctx ->
        printfn "Found comma"
        ctx)
    |> Parser.onChar ':' (fun ctx ->
        printfn "Found colon"
        ctx)

    // Skip whitespace
    |> Parser.onPattern @"\s+" (fun ctx _ -> ctx)
```

## Step 6: Testing the Parser

Create a simple test program:

```fsharp
[<EntryPoint>]
let main argv =
    let parser = createJsonParser ()

    // Test with simple JSON
    let simpleJson = """{"name": "John", "age": 30, "active": true}"""

    printfn "Parsing: %s" simpleJson
    printfn ""

    let context = Parser.runString simpleJson parser

    let tokens = Parser.getTokens context
    let errors = Parser.getErrors context

    printfn "Tokens found: %d" (List.length tokens)
    printfn "Errors: %d" (List.length errors)

    if not (List.isEmpty errors) then
        printfn "Errors:"
        errors |> List.iter (fun error ->
            printfn "  Line %d, Col %d: %s" error.Line error.Col error.Message)

    0
```

## Step 7: Handling Complex JSON

Test with more complex JSON structures:

```fsharp
// Test with nested objects and arrays
let complexJson = """
{
    "user": {
        "name": "Alice",
        "age": 25,
        "hobbies": ["reading", "coding", "gaming"],
        "address": {
            "street": "123 Main St",
            "city": "Anytown",
            "coordinates": [40.7128, -74.0060]
        }
    },
    "active": true,
    "score": null
}
"""

printfn "Parsing complex JSON..."
let context = Parser.runString complexJson parser
// ... handle results
```

## Step 8: Adding AST Building

To build a proper JSON AST, enable AST building and add custom builders:

```fsharp
let createAstJsonParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.enableAST ()
    |> Parser.onError (fun ctx msg ->
        printfn "Parse error: %s" msg
        ctx)

    // ... (include all the handlers from createJsonParser)

    // Add custom AST builders for JSON structures
    |> Parser.onAST (fun mode matched ctx ->
        match matched with
        | "{" -> Some (ASTNode.Object [])
        | "[" -> Some (ASTNode.Array [])
        | str when str.StartsWith("\"") && str.EndsWith("\"") ->
            Some (ASTNode.String (str.Trim('"')))
        | num when System.Double.TryParse(num, ref 0.0) ->
            Some (ASTNode.Number (System.Double.Parse(num)))
        | "true" -> Some (ASTNode.Boolean true)
        | "false" -> Some (ASTNode.Boolean false)
        | "null" -> Some (ASTNode.Null)
        | _ -> None)
```

## Step 9: Complete Example

Here's a complete working JSON parser:

```fsharp
module JsonParserTutorial

open SharpParser.Core

let createJsonParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.enableAST ()
    |> Parser.onError (fun ctx msg -> printfn "Error: %s" msg; ctx)

    // String literals
    |> Parser.onPattern @"""[^""]*""" (fun ctx matched ->
        printfn "String: %s" matched
        ctx)

    // Numbers
    |> Parser.onPattern @"-?\d+(\.\d+)?" (fun ctx matched ->
        printfn "Number: %s" matched
        ctx)

    // Booleans and null
    |> Parser.onSequence "true" (fun ctx -> printfn "Boolean: true"; ctx)
    |> Parser.onSequence "false" (fun ctx -> printfn "Boolean: false"; ctx)
    |> Parser.onSequence "null" (fun ctx -> printfn "Null"; ctx)

    // Objects
    |> Parser.onChar '{' (fun ctx ->
        printfn "Start object"
        ParserContextOps.enterMode "object" ctx)
    |> Parser.onChar '}' (fun ctx ->
        printfn "End object"
        ParserContextOps.exitMode ctx)

    // Arrays
    |> Parser.onChar '[' (fun ctx ->
        printfn "Start array"
        ParserContextOps.enterMode "array" ctx)
    |> Parser.onChar ']' (fun ctx ->
        printfn "End array"
        ParserContextOps.exitMode ctx)

    // Structural elements
    |> Parser.onChar ',' (fun ctx -> ctx)
    |> Parser.onChar ':' (fun ctx -> ctx)

    // Whitespace
    |> Parser.onPattern @"\s+" (fun ctx _ -> ctx)

[<EntryPoint>]
let main argv =
    let parser = createJsonParser ()

    let json = """{"name": "John", "items": [1, 2, {"nested": true}]}"""

    printfn "Parsing JSON: %s\n" json

    let context = Parser.runString json parser

    printfn "Tokens: %d" (List.length (Parser.getTokens context))
    printfn "AST nodes: %d" (List.length (Parser.getAST context))
    printfn "Errors: %d" (List.length (Parser.getErrors context))

    0
```

## Step 10: Running and Testing

```bash
dotnet run
```

You should see output showing the parser recognizing different JSON elements as it processes the input.

## Next Steps

- Add proper escape sequence handling for strings
- Implement JSON schema validation
- Add support for comments (non-standard but useful)
- Create a JSON-to-F# type converter

## Related Examples

Check out `examples/SharpParser.Examples/JsonExample.fs` for a more complete implementation with additional features.