namespace SharpParser.Core

/// Module for the core parsing engine that processes input and dispatches to handlers
module ParsingEngine =
    /// Helper function to handle token emission and AST building after a match
    let private handleMatch (config: ParserConfig) (matchedText: string) (contextAfterPosition: ParserContext) : ParserContext =
        // Auto-emit token if tokenization enabled
        let contextAfterToken =
            if ParserConfig.isTokenizationEnabled config then
                Tokenizer.autoEmit config.Keywords matchedText contextAfterPosition
            else
                contextAfterPosition

        // Auto-add AST node if AST building enabled
        if ParserConfig.isASTBuildingEnabled config then
            ASTBuilder.autoAddNode config matchedText contextAfterToken
        else
            contextAfterToken

    /// Processes input starting at the given position and dispatches to appropriate handlers
    /// Returns the updated context and the number of characters consumed
    let processPosition (config: ParserConfig) (context: ParserContext) (line: string) (pos: int) : ParserContext * int =
        // Get current mode for handler lookup
        let currentMode = ParserContextOps.currentMode context

        // Try sequence match first (longest match from current position)
        let sequenceMatch = HandlerRegistry.matchSequence currentMode line pos config.Registry

        match sequenceMatch with
        | Some (length, handler) ->
            // Sequence matched, invoke handler
            let contextAfterHandler = handler context

            // Extract the matched sequence
            let matchedText = line.Substring(pos, length)

            // Update position to after the match
            let contextAfterPosition = ParserContextOps.updatePosition context.Line (context.Col + length) contextAfterHandler

            // Handle token emission and AST building
            let finalContext = handleMatch config matchedText contextAfterPosition

            (finalContext, length)

        | None ->
            // No sequence match, try pattern match
            let patternMatch = HandlerRegistry.matchPattern currentMode line pos config.Registry

            match patternMatch with
            | Some (length, matchedText, handler) ->
                // Pattern matched, invoke handler
                let contextAfterHandler = handler context matchedText

                // Update position to after the match
                let contextAfterPosition = ParserContextOps.updatePosition context.Line (context.Col + length) contextAfterHandler
    
                // Handle token emission and AST building
                let finalContext = handleMatch config matchedText contextAfterPosition
    
                (finalContext, length)

            | None ->
                // No pattern match, check if we're at end of line
                if pos >= line.Length then
                    (context, 0) // Nothing consumed
                else
                    // Try character handlers for current character
                    let currentChar = line.[pos]
                    let charHandlers = HandlerRegistry.getCharHandlers currentMode currentChar config.Registry

                    match charHandlers with
                    | [] ->
                        // No handlers matched - check if any handlers are registered at all
                        let hasAnyHandlers =
                            not (Map.isEmpty config.Registry.CharHandlers) ||
                            not (Map.isEmpty config.Registry.SequenceTrie) ||
                            not (Map.isEmpty config.Registry.PatternHandlers)
                        if hasAnyHandlers then
                            // Only report error if handlers are registered but none matched
                            let suggestion = Some (sprintf "Consider adding a handler for character '%c' or using a pattern handler" currentChar)
                            let errorContext = ErrorHandling.triggerError (ParseError.UnexpectedChar currentChar) config context suggestion
                            let contextAfterPosition = ParserContextOps.updatePosition context.Line (context.Col + 1) errorContext
                            (contextAfterPosition, 1)
                        else
                            // No handlers registered at all - just consume the character
                            let contextAfterPosition = ParserContextOps.updatePosition context.Line (context.Col + 1) context
                            (contextAfterPosition, 1)
                    | handlers ->
                        // Invoke all character handlers
                        let contextAfterHandlers = handlers |> List.fold (fun ctx handler -> handler ctx) context
    
                        // Update position to after the character
                        let contextAfterPosition = ParserContextOps.updatePosition context.Line (context.Col + 1) contextAfterHandlers

                        // Convert character to string for auto functions
                        let charAsString = string currentChar

                        // Handle token emission and AST building
                        let finalContext = handleMatch config charAsString contextAfterPosition

                        (finalContext, 1)

    /// Processes an entire line of input
    let processLine (config: ParserConfig) (context: ParserContext) (line: string) : ParserContext =
        // Update context buffer with line
        let context = ParserContextOps.updateBuffer line context

        // Process each character position in the line
        let rec processPositions (pos: int) (ctx: ParserContext) : ParserContext =
            if pos >= line.Length then
                ctx
            else
                let (updatedContext, consumedChars) = processPosition config ctx line pos

                // Skip the consumed characters
                processPositions (pos + consumedChars) updatedContext

        // Start processing from position 0
        let finalContext = processPositions 0 context

        // Update line number for next line (only if line has content)
        if line.Length > 0 then
            ParserContextOps.updatePosition (context.Line + 1) 1 finalContext
        else
            finalContext

    /// Processes a sequence of input lines
    let processInput (config: ParserConfig) (context: ParserContext) (lines: string seq) : ParserContext =
        // Fold over lines with processLine, tracking line numbers
        lines |> Seq.fold (fun ctx line -> processLine config ctx line) context

    /// Parses a file and returns the final context
    let parseFile (config: ParserConfig) (filePath: string) : ParserContext =
        try
            // Create initial context with file path
            let initialContext = ParserContextOps.create filePath config.EnableTrace

            // Read file lines
            let lines = System.IO.File.ReadLines(filePath)

            // Process input
            processInput config initialContext lines

        with
        | ex ->
            // File reading error - add error to context
            let errorContext = ParserContextOps.create filePath false
            let errorInfo = {
                ErrorType = ParseError.ConfigError (sprintf "Failed to read file: %s" ex.Message)
                Line = 1
                Col = 1
                Mode = None
                Message = sprintf "Failed to read file: %s" ex.Message
                Suggestion = Some "Check that the file exists and is readable"
            }
            ParserContextOps.addError errorInfo errorContext

    /// Parses a string input and returns the final context
    let parseString (config: ParserConfig) (input: string) : ParserContext =
        // Create initial context for string input
        let initialContext = ParserContextOps.create "<string>" config.EnableTrace

        // Split string into lines
        let lines = input.Split([|'\r'; '\n'|], System.StringSplitOptions.None)

        // Process input
        processInput config initialContext lines