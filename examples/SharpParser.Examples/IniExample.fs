namespace SharpParser.Examples

open SharpParser.Core

/// INI file parsing example demonstrating section-based parsing with modes
module IniExample =
    /// Runs the INI parsing example
    let run () : unit =
        printfn "=== INI File Parsing Example ==="
        printfn "Demonstrating configuration file parsing with sections"
        printfn "This example parses INI format with [sections] and key=value pairs"
        printfn ""

        // Create parser for INI format
        let parser =
            Parser.create ()
            |> Parser.enableTokens ()
            |> Parser.onError (fun ctx msg ->
                printfn "Parse error: %s" msg
                ctx)

            // Handle section headers [SectionName]
            |> Parser.onPattern @"\[([^\]]+)\]" (fun ctx matched ->
                let sectionName = matched.Trim('[', ']')
                printfn "Found section: [%s]" sectionName
                ParserContextOps.enterMode sectionName ctx)

            // Handle key-value pairs
            |> Parser.onPattern @"^([^=]+)=(.*)$" (fun ctx matched ->
                let parts = matched.Split('=', 2)
                if parts.Length = 2 then
                    let key = parts.[0].Trim()
                    let value = parts.[1].Trim()
                    printfn "Found key-value: %s = %s" key value
                ctx)

            // Handle comments (lines starting with ; or #)
            |> Parser.onPattern @"^[;#].*" (fun ctx matched ->
                printfn "Found comment: %s" matched
                ctx)

            // Handle empty lines
            |> Parser.onPattern @"^\s*$" (fun ctx _ ->
                printfn "Empty line"
                ctx)

            // Skip leading/trailing whitespace on lines
            |> Parser.onPattern @"^\s+|\s+$" (fun ctx _ -> ctx)

        // Sample INI configuration
        let iniData = """; Sample configuration file
[Database]
host = localhost
port = 5432
username = admin
password = secret123

[Application]
name = MyApp
version = 1.0.0
debug = true

; Another comment
[Logging]
level = INFO
file = app.log
max_size = 10MB
"""

        printfn "Parsing INI data:"
        printfn "%s" iniData
        printfn ""

        // Parse the INI file
        let context = Parser.runString iniData parser

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