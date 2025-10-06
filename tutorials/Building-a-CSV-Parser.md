# Tutorial: Building a CSV Parser with SharpParser

This tutorial will teach you how to build a robust CSV parser using SharpParser.Core. We'll handle quoted fields, escaped quotes, and various edge cases that make CSV parsing challenging.

## Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- SharpParser.Core NuGet package
- Basic understanding of F#

## Step 1: Understanding CSV Challenges

CSV (Comma-Separated Values) seems simple but has tricky edge cases:
- **Quoted fields**: `"New York, NY"` contains a comma but is one field
- **Escaped quotes**: `"She said ""Hello"""` contains a quote character
- **Newlines in fields**: Multi-line content within quotes
- **Empty fields**: `,,` represents empty values between commas

## Step 2: Project Setup

Create a new F# console application:

```bash
dotnet new console -lang F# -n CsvParserTutorial
cd CsvParserTutorial
dotnet add package SharpParser.Core
```

## Step 3: Basic CSV Structure

A CSV file consists of:
- **Records**: Lines separated by newlines
- **Fields**: Values separated by commas
- **Quoted fields**: Fields containing special characters wrapped in quotes
- **Escaped quotes**: Double quotes within quoted fields

## Step 4: Basic Field Parsing

Let's start with parsing individual fields:

```fsharp
open SharpParser.Core

// Basic CSV field parser
let createBasicCsvParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onError (fun ctx msg ->
        printfn "Parse error: %s" msg
        ctx)

    // Handle quoted fields (fields containing commas or quotes)
    // Pattern: " followed by any chars except ", or escaped quotes "", followed by "
    |> Parser.onPattern @"""(?:[^""]|"""")*""" (fun ctx matched ->
        // Remove outer quotes and unescape inner quotes
        let unescaped = matched.Trim('"').Replace("\"\"", "\"")
        printfn "Quoted field: '%s'" unescaped
        ctx)

    // Handle unquoted fields (no commas or quotes)
    |> Parser.onPattern @"[^,\r\n]+" (fun ctx matched ->
        printfn "Field: '%s'" matched
        ctx)

    // Handle field separators
    |> Parser.onChar ',' (fun ctx ->
        printfn "Field separator"
        ctx)

    // Handle record separators (newlines)
    |> Parser.onChar '\n' (fun ctx ->
        printfn "Record separator"
        ctx)
    |> Parser.onChar '\r' (fun ctx -> ctx) // Handle \r\n sequences
```

## Step 5: Testing Basic Parsing

Test with simple CSV data:

```fsharp
[<EntryPoint>]
let main argv =
    let parser = createBasicCsvParser ()

    let simpleCsv = "Name,Age,City\nJohn,30,New York\nJane,25,Los Angeles"

    printfn "Parsing CSV:\n%s\n" simpleCsv

    let context = Parser.runString simpleCsv parser

    printfn "Tokens: %d" (List.length (Parser.getTokens context))
    printfn "Errors: %d" (List.length (Parser.getErrors context))

    0
```

## Step 6: Handling Quoted Fields

The real challenge is quoted fields. Let's improve our parser:

```fsharp
let createCsvParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onError (fun ctx msg ->
        printfn "Parse error: %s" msg
        ctx)

    // Quoted fields with proper escaping
    // This regex handles: "field", "field with ""quotes""", "field,with,commas"
    |> Parser.onPattern @"""(?:[^""]|"""")*""" (fun ctx matched ->
        let fieldContent = matched.Trim('"').Replace("\"\"", "\"")
        printfn "Quoted field: '%s'" fieldContent
        ctx)

    // Unquoted fields (cannot contain commas, quotes, or newlines)
    |> Parser.onPattern @"[^,\r\n]+" (fun ctx matched ->
        printfn "Unquoted field: '%s'" matched
        ctx)

    // Field separator
    |> Parser.onChar ',' (fun ctx ->
        printfn "Field separator"
        ctx)

    // Record separator
    |> Parser.onChar '\n' (fun ctx ->
        printfn "New record"
        ctx)
    |> Parser.onChar '\r' (fun ctx -> ctx) // Handle Windows line endings
```

## Step 7: Testing with Complex CSV

Test with challenging CSV data:

```fsharp
let complexCsv = """Name,Age,City,Occupation
"John Doe",30,"New York, NY",Software Engineer
"Jane ""Smith""",25,Los Angeles,"Data Scientist"
Bob,35,Chicago,Designer
,28,"Austin, TX",Manager"""

printfn "Parsing complex CSV with quotes and commas:\n%s\n" complexCsv

let context = Parser.runString complexCsv parser

printfn "Tokens: %d" (List.length (Parser.getTokens context))
printfn "Errors: %d" (List.length (Parser.getErrors context))

// Show all tokens
Parser.getTokens context |> List.iter (fun token ->
    printfn "  %A" token)
```

## Step 8: Building CSV Records

To build actual CSV records from tokens, we need to group fields into records:

```fsharp
let createRecordBuilderParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onError (fun ctx msg ->
        printfn "Parse error: %s" msg
        ctx)

    // ... (include field parsing handlers from above)

    // Custom logic to build records
    |> Parser.onChar '\n' (fun ctx ->
        printfn "Processing complete record"
        // Here you could collect fields into records
        ctx)
```

## Step 9: Complete CSV Parser with Record Building

Here's a more complete implementation:

```fsharp
module CsvParserTutorial

open SharpParser.Core

type CsvRecord = {
    Fields: string list
}

type CsvDocument = {
    Headers: string list option
    Records: CsvRecord list
}

let createCsvParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onError (fun ctx msg ->
        printfn "CSV Parse error: %s" msg
        ctx)

    // Quoted fields with escaping
    |> Parser.onPattern @"""(?:[^""]|"""")*""" (fun ctx matched ->
        let field = matched.Trim('"').Replace("\"\"", "\"")
        printfn "Field: %s" field
        ctx)

    // Unquoted fields
    |> Parser.onPattern @"[^,\r\n]+" (fun ctx matched ->
        printfn "Field: %s" matched
        ctx)

    // Field separator
    |> Parser.onChar ',' (fun ctx -> ctx)

    // Record separator
    |> Parser.onChar '\n' (fun ctx ->
        printfn "--- End of record ---"
        ctx)
    |> Parser.onChar '\r' (fun ctx -> ctx)

[<EntryPoint>]
let main argv =
    let parser = createCsvParser ()

    // Test data with various edge cases
    let csvData = """Name,Age,City,Occupation
"John ""The Great"" Doe",30,"New York, NY",Software Engineer
Jane,25,Los Angeles,"Data Scientist"
Bob,,Chicago,Designer
,28,"Austin, TX",Manager"""

    printfn "Parsing CSV data:\n%s\n" csvData

    let context = Parser.runString csvData parser

    let tokens = Parser.getTokens context
    let errors = Parser.getErrors context

    printfn "\nResults:"
    printfn "Tokens found: %d" (List.length tokens)
    printfn "Errors: %d" (List.length errors)

    if not (List.isEmpty errors) then
        printfn "\nErrors:"
        errors |> List.iter (fun error ->
            printfn "  Line %d, Col %d: %s" error.Line error.Col error.Message)

    printfn "\nSample tokens:"
    tokens |> List.take 10 |> List.iter (fun token ->
        printfn "  %A" token)

    0
```

## Step 10: Advanced Features

For a production CSV parser, consider adding:

```fsharp
// Handle different delimiters (not just commas)
let createFlexibleCsvParser delimiter =
    Parser.create ()
    // ... use delimiter parameter instead of hardcoded ','

// Handle headers automatically
let parseWithHeaders csvText =
    let lines = csvText.Split('\n')
    let headers = parseHeaderLine lines.[0]
    let records = lines.[1..] |> Array.map parseRecordLine
    { Headers = Some headers; Records = records }

// Support for different quote characters
// Support for custom escape characters
// Handle malformed CSV gracefully
```

## Step 11: Running the Tutorial

```bash
dotnet run
```

You should see the parser correctly identifying quoted and unquoted fields, handling escaped quotes, and managing commas within quoted fields.

## Common CSV Parsing Challenges Solved

1. **Quoted fields with commas**: `"New York, NY"` stays as one field
2. **Escaped quotes**: `"She said ""Hello"""` becomes `She said "Hello"`
3. **Empty fields**: `,,` creates empty strings between commas
4. **Mixed quoted/unquoted**: Handles both types in the same file

## Next Steps

- Add support for custom delimiters (tabs, pipes, etc.)
- Implement CSV writing functionality
- Add schema validation for CSV columns
- Support for multi-line fields
- Performance optimizations for large CSV files

## Related Examples

See `examples/SharpParser.Examples/CsvExample.fs` for a complete working implementation with additional features.