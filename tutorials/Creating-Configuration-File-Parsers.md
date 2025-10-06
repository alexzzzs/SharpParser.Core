# Tutorial: Creating Configuration File Parsers with SharpParser

This tutorial demonstrates how to build parsers for configuration files like INI, properties, and other structured text formats using SharpParser.Core. We'll focus on INI files as they're commonly used for application configuration.

## Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- SharpParser.Core NuGet package
- Basic understanding of F#

## Step 1: Understanding INI Files

INI files have a simple structure:
```ini
[Section1]
key1=value1
key2=value2

[Section2]
key3=value3
; This is a comment
# This is also a comment
```

Key characteristics:
- **Sections**: `[SectionName]` groups related settings
- **Key-value pairs**: `key=value` stores individual settings
- **Comments**: Lines starting with `;` or `#`
- **Whitespace**: Usually ignored

## Step 2: Project Setup

Create a new F# console application:

```bash
dotnet new console -lang F# -n ConfigParserTutorial
cd ConfigParserTutorial
dotnet add package SharpParser.Core
```

## Step 3: Basic INI Structure Recognition

Let's start by recognizing the basic elements of INI files:

```fsharp
open SharpParser.Core

// Basic INI parser that recognizes sections and key-value pairs
let createBasicIniParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onError (fun ctx msg ->
        printfn "Parse error: %s" msg
        ctx)

    // Section headers: [SectionName]
    |> Parser.onPattern @"\[([^\]]+)\]" (fun ctx matched ->
        let sectionName = matched.Trim('[', ']')
        printfn "Found section: [%s]" sectionName
        ParserContextOps.enterMode sectionName ctx)

    // Key-value pairs: key=value
    |> Parser.onPattern @"^([^=]+)=(.*)$" (fun ctx matched ->
        let parts = matched.Split('=', 2)
        if parts.Length = 2 then
            let key = parts.[0].Trim()
            let value = parts.[1].Trim()
            printfn "Found setting: %s = %s" key value
        ctx)

    // Comments (lines starting with ; or #)
    |> Parser.onPattern @"^[;#].*" (fun ctx matched ->
        printfn "Found comment: %s" matched
        ctx)

    // Empty lines
    |> Parser.onPattern @"^\s*$" (fun ctx _ ->
        printfn "Empty line"
        ctx)
```

## Step 4: Testing Basic Recognition

Test with a simple INI file:

```fsharp
[<EntryPoint>]
let main argv =
    let parser = createBasicIniParser ()

    let iniContent = """[Database]
host=localhost
port=5432

[Application]
name=MyApp
debug=true"""

    printfn "Parsing INI content:\n%s\n" iniContent

    let context = Parser.runString iniContent parser

    printfn "Tokens: %d" (List.length (Parser.getTokens context))
    printfn "Errors: %d" (List.length (Parser.getErrors context))

    0
```

## Step 5: Building a Configuration Object

To create a useful configuration parser, we need to build a data structure:

```fsharp
type Configuration = Map<string, Map<string, string>>

let createIniParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onError (fun ctx msg ->
        printfn "Parse error: %s" msg
        ctx)

    // ... (include all handlers from createBasicIniParser)

    // Use user data to build configuration during parsing
    |> Parser.onPattern @"\[([^\]]+)\]" (fun ctx matched ->
        let sectionName = matched.Trim('[', ']')
        printfn "Entering section: [%s]" sectionName

        // Store current section in context
        let ctxWithSection = ParserContextOps.setUserData "currentSection" sectionName ctx
        ParserContextOps.enterMode sectionName ctxWithSection)

    |> Parser.onPattern @"^([^=]+)=(.*)$" (fun ctx matched ->
        let parts = matched.Split('=', 2)
        if parts.Length = 2 then
            let key = parts.[0].Trim()
            let value = parts.[1].Trim()

            // Get current section and build configuration
            match ParserContextOps.getUserData "currentSection" ctx with
            | Some sectionName ->
                // Here you would update a configuration map
                // For now, just print
                printfn "Setting [%s] %s = %s" sectionName key value
            | None ->
                printfn "Setting (no section) %s = %s" key value
        ctx)
```

## Step 6: Complete INI Parser with Data Structure

Here's a complete parser that builds a configuration object:

```fsharp
module ConfigParserTutorial

open SharpParser.Core

type IniConfig = Map<string, Map<string, string>>

let createIniParser () =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onError (fun ctx msg ->
        printfn "INI Parse error: %s" msg
        ctx)

    // Section headers
    |> Parser.onPattern @"\[([^\]]+)\]" (fun ctx matched ->
        let sectionName = matched.Trim('[', ']')
        printfn "Section: [%s]" sectionName
        ParserContextOps.enterMode sectionName ctx)

    // Key-value pairs
    |> Parser.onPattern @"^([^=]+)=(.*)$" (fun ctx matched ->
        let parts = matched.Split('=', 2)
        if parts.Length = 2 then
            let key = parts.[0].Trim()
            let value = parts.[1].Trim()
            printfn "Setting: %s = %s" key value
        ctx)

    // Comments
    |> Parser.onPattern @"^[;#].*" (fun ctx matched ->
        printfn "Comment: %s" matched
        ctx)

    // Empty lines and whitespace
    |> Parser.onPattern @"^\s*$" (fun ctx _ -> ctx)
    |> Parser.onPattern @"^\s+|\s+$" (fun ctx _ -> ctx)

[<EntryPoint>]
let main argv =
    let parser = createIniParser ()

    let iniData = """; Sample application configuration
[Database]
host = localhost
port = 5432
username = admin
password = secret123

[Application]
name = My Awesome App
version = 1.0.0
debug = true

; Feature flags
[Features]
logging = enabled
caching = disabled
experimental = false"""

    printfn "Parsing INI configuration:\n%s\n" iniData

    let context = Parser.runString iniData parser

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
    tokens |> List.take 15 |> List.iter (fun token ->
        printfn "  %A" token)

    0
```

## Step 7: Advanced Configuration Features

Add support for more advanced INI features:

```fsharp
// Support for quoted values
|> Parser.onPattern @"^([^=]+)=\s*""([^""]*)""" (fun ctx matched ->
    // Handle quoted values with spaces
    ctx)

// Support for multi-line values (non-standard but useful)
|> Parser.onPattern @"^([^=]+)=\s*<<(\w+)$" (fun ctx matched ->
    // Start multi-line value capture
    ctx)

// Support for environment variable substitution
|> Parser.onPattern @"\$\{([^}]+)\}" (fun ctx matched ->
    // Replace with environment variable
    ctx)

// Support for include directives
|> Parser.onPattern @"^include\s+(.+)$" (fun ctx matched ->
    // Load additional config file
    ctx)
```

## Step 8: Parsing Other Configuration Formats

The same approach works for other formats:

### Properties Files (Java-style)
```fsharp
let createPropertiesParser () =
    Parser.create ()
    // Similar to INI but no sections
    |> Parser.onPattern @"^([^=]+)=(.*)$" (fun ctx matched ->
        let parts = matched.Split('=', 2)
        let key = parts.[0].Trim()
        let value = parts.[1].Trim()
        printfn "Property: %s = %s" key value
        ctx)
```

### YAML-like Simple Format
```fsharp
let createYamlParser () =
    Parser.create ()
    // Basic YAML structure
    |> Parser.onPattern @"^(\s*)([^:]+):\s*(.*)$" (fun ctx matched ->
        // Handle indentation for nesting
        ctx)
```

### TOML-like Format
```fsharp
let createTomlParser () =
    Parser.create ()
    // TOML table headers
    |> Parser.onPattern @"^\[([^\]]+)\]$" (fun ctx matched ->
        let tableName = matched.Trim('[', ']')
        printfn "Table: [%s]" tableName
        ctx)
    // Key-value pairs
    |> Parser.onPattern @"^([^=]+)=(.*)$" (fun ctx matched ->
        ctx)
```

## Step 9: Running the Tutorial

```bash
dotnet run
```

You should see the parser correctly identifying sections, key-value pairs, and comments in the INI file.

## Step 10: Integration with Application Code

Here's how to use the parsed configuration in an application:

```fsharp
// Function to extract configuration values
let getConfigValue (config: IniConfig) section key defaultValue =
    config
    |> Map.tryFind section
    |> Option.bind (Map.tryFind key)
    |> Option.defaultValue defaultValue

// Usage example
let config = parseIniFile "app.config"
let dbHost = getConfigValue config "Database" "host" "localhost"
let appName = getConfigValue config "Application" "name" "MyApp"
let debugMode = getConfigValue config "Application" "debug" "false" = "true"
```

## Common Configuration Parsing Challenges Solved

1. **Section grouping**: `[Section]` headers group related settings
2. **Key-value parsing**: `key=value` with proper trimming
3. **Comments**: Ignore lines starting with `;` or `#`
4. **Whitespace handling**: Trim keys and values appropriately
5. **Empty lines**: Skip blank lines gracefully

## Next Steps

- Add support for array values: `key = [value1, value2]`
- Implement configuration validation against schemas
- Add support for environment variable substitution
- Create configuration watchers for hot reloading
- Add encryption support for sensitive values

## Related Examples

See `examples/SharpParser.Examples/IniExample.fs` for a complete working implementation with additional features.