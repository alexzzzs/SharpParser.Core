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

    /// Processes input starting at the given position and dispatches to appropriate handlers.
    /// This is the core parsing function that implements the parsing priority order:
    /// 1. Sequence handlers (longest match wins)
    /// 2. Pattern handlers (regex-based)
    /// 3. Character handlers (single character)
    /// Returns the updated context and the number of characters consumed
    let processPosition (config: ParserConfig) (context: ParserContext) (line: string) (pos: int) : ParserContext * int =
        // Determine the current parsing mode to look up appropriate handlers
        let currentMode = ParserContextOps.currentMode context

        // FIRST: Try to match sequences (keywords, operators) - these have highest priority
        // Sequence matching uses a trie for efficient longest-match lookup
        let sequenceMatch = HandlerRegistry.matchSequence currentMode line pos config.Registry

        match sequenceMatch with
        | Some (length, handler) ->
            // A sequence was matched - invoke the associated handler
            let contextAfterHandler = handler context

            // Extract the actual text that was matched for tokenization/AST building
            let matchedText = line.Substring(pos, length)

            // Advance the parsing position by the length of the matched sequence
            let contextAfterPosition = ParserContextOps.updatePosition context.Line (context.Col + length) contextAfterHandler

            // If tokenization or AST building is enabled, automatically create tokens/nodes
            let finalContext = handleMatch config matchedText contextAfterPosition

            (finalContext, length)

        | None ->
            // SECOND: No sequence match, try pattern matching (regex-based handlers)
            // Patterns can match identifiers, numbers, strings, etc.
            let patternMatch = HandlerRegistry.matchPattern currentMode line pos config.Registry

            match patternMatch with
            | Some (length, matchedText, handler) ->
                // A pattern was matched - invoke the handler with the matched text
                let contextAfterHandler = handler context matchedText

                // Advance position by the length of the matched pattern
                let contextAfterPosition = ParserContextOps.updatePosition context.Line (context.Col + length) contextAfterHandler

                // Handle automatic tokenization and AST building
                let finalContext = handleMatch config matchedText contextAfterPosition

                (finalContext, length)

            | None ->
                // THIRD: No pattern match either, check if we've reached end of line
                if pos >= line.Length then
                    (context, 0) // End of line reached, nothing consumed
                else
                    // Try character-level handlers for the current character
                    let currentChar = line.[pos]
                    let charHandlers = HandlerRegistry.getCharHandlers currentMode currentChar config.Registry

                    match charHandlers with
                    | [] ->
                        // No character handlers found for this character
                        // Check if the parser has any handlers registered at all
                        let hasAnyHandlers =
                            not (Map.isEmpty config.Registry.CharHandlers) ||
                            not (Map.isEmpty config.Registry.SequenceTrie) ||
                            not (Map.isEmpty config.Registry.PatternHandlers)

                        if hasAnyHandlers then
                            // Handlers exist but none matched - this is an unexpected character
                            // Generate an error with a helpful suggestion
                            let suggestion = Some (sprintf "Consider adding a handler for character '%c' or using a pattern handler" currentChar)
                            let errorContext = ErrorHandling.triggerError (ParseError.UnexpectedChar currentChar) config context suggestion
                            let contextAfterPosition = ParserContextOps.updatePosition context.Line (context.Col + 1) errorContext
                            (contextAfterPosition, 1)
                        else
                            // No handlers registered at all - silently consume the character
                            // This allows the parser to work as a simple character-by-character processor
                            let contextAfterPosition = ParserContextOps.updatePosition context.Line (context.Col + 1) context
                            (contextAfterPosition, 1)
                    | handlers ->
                        // Character handlers found - invoke all of them in sequence
                        // Multiple handlers can be registered for the same character
                        let contextAfterHandlers = handlers |> List.fold (fun ctx handler -> handler ctx) context

                        // Advance position by one character
                        let contextAfterPosition = ParserContextOps.updatePosition context.Line (context.Col + 1) contextAfterHandlers

                        // Convert the character to string for tokenization/AST purposes
                        let charAsString = string currentChar

                        // Handle automatic token emission and AST node creation
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

    /// Parallel parsing functions for improved performance on multi-core systems

    /// Processes lines in parallel when tokenization is enabled
    /// This is safe because tokenization doesn't depend on sequential state
    let processInputParallel (config: ParserConfig) (context: ParserContext) (lines: string seq) : ParserContext =
        if ParserConfig.isParallelTokenizationEnabled config && ParserConfig.isTokenizationEnabled config then
            // Parallel tokenization path
            let linesArray = lines |> Seq.toArray
            let maxParallelism = min config.ParallelConfig.MaxParallelism linesArray.Length

            // Process lines in parallel, collecting tokenized results with line indices
            let tokenizedResults =
                linesArray
                |> Array.Parallel.mapi (fun lineIndex line ->
                    // Create a temporary context for this line's tokenization
                    let lineContext = ParserContextOps.create "<parallel-line>" false
                    let lineContext = ParserContextOps.updateBuffer line lineContext
                    let lineContext = ParserContextOps.updatePosition (context.Line + lineIndex) 1 lineContext
                    let processedContext = processLine config lineContext line
                    (lineIndex, processedContext)
                )

            // Merge tokenized results back into main context
            // This preserves the sequential nature while parallelizing tokenization
            let finalContext =
                tokenizedResults
                |> Array.fold (fun ctx (lineIndex, lineResult) ->
                    // Merge tokens from this line result with corrected line numbers
                    let lineTokens = (ParserContextOps.getState lineResult).Tokens
                    let correctedTokens = lineTokens |> List.map (fun token ->
                        { token with Line = context.Line + lineIndex })
                    let currentState = ParserContextOps.getState ctx
                    let mergedState = { currentState with Tokens = currentState.Tokens @ correctedTokens }
                    ParserContextOps.setState mergedState ctx
                ) context

            // Update final line count
            ParserContextOps.updatePosition (context.Line + linesArray.Length) 1 finalContext
        else
            // Fall back to sequential processing
            processInput config context lines

    /// Configuration for function boundary detection patterns
    type BoundaryPattern = {
        /// Regex pattern to match function/class declarations
        DeclarationPattern: string
        /// Name capture group index in the regex (1-based)
        NameGroupIndex: int
        /// Type of declaration (function, class, method, etc.)
        DeclarationType: string
        /// Opening delimiter character
        OpenDelimiter: char
        /// Closing delimiter character
        CloseDelimiter: char
    }

    /// Default boundary patterns for common languages
    let defaultBoundaryPatterns = [
        // JavaScript/TypeScript functions
        {
            DeclarationPattern = @"^\s*(?:export\s+)?(?:async\s+)?function\s+(\w+)"
            NameGroupIndex = 1
            DeclarationType = "function"
            OpenDelimiter = '{'
            CloseDelimiter = '}'
        }
        // JavaScript/TypeScript arrow functions (limited)
        {
            DeclarationPattern = @"^\s*(?:const|let|var)\s+(\w+)\s*=\s*(?:\([^)]*\)\s*)?=>\s*\{"
            NameGroupIndex = 1
            DeclarationType = "arrow-function"
            OpenDelimiter = '{'
            CloseDelimiter = '}'
        }
        // Class declarations
        {
            DeclarationPattern = @"^\s*(?:export\s+)?class\s+(\w+)"
            NameGroupIndex = 1
            DeclarationType = "class"
            OpenDelimiter = '{'
            CloseDelimiter = '}'
        }
        // C# methods
        {
            DeclarationPattern = @"^\s*(?:public|private|protected|internal)?\s*(?:static|virtual|override|abstract)?\s*\w+\s+(\w+)\s*\("
            NameGroupIndex = 1
            DeclarationType = "method"
            OpenDelimiter = '{'
            CloseDelimiter = '}'
        }
        // F# functions and members
        {
            DeclarationPattern = @"^\s*(?:let|member)\s+(?:rec\s+)?(\w+)"
            NameGroupIndex = 1
            DeclarationType = "function"
            OpenDelimiter = '='
            CloseDelimiter = '\n'  // F# uses indentation, but we'll use line-based for simplicity
        }
    ]

    /// Identifies function/class boundaries in source code for parallel parsing
    let identifyFunctionBoundaries (input: string) : FunctionBoundary list =
        let lines = input.Split([|'\r'; '\n'|], System.StringSplitOptions.None)
        let functions = ResizeArray<FunctionBoundary>()

        let compiledPatterns = defaultBoundaryPatterns |> List.map (fun pattern ->
            (System.Text.RegularExpressions.Regex(pattern.DeclarationPattern), pattern))

        let mutable currentFunction: FunctionBoundary option = None
        let mutable braceDepth = 0

        for i in 0 .. lines.Length - 1 do
            let line = lines.[i]
            let trimmed = line.Trim()

            // Check for function/class declarations using regex patterns
            let declarationMatch = compiledPatterns |> List.tryPick (fun (regex, pattern) ->
                let matchResult = regex.Match(line)
                if matchResult.Success && matchResult.Groups.Count > pattern.NameGroupIndex then
                    Some (matchResult.Groups.[pattern.NameGroupIndex].Value, pattern)
                else None)

            match declarationMatch with
            | Some (name, pattern) ->
                // If we were tracking a previous function, complete it
                match currentFunction with
                | Some func ->
                    let contentLines = lines.[func.StartLine-1..i-1]
                    functions.Add({ func with EndLine = i; Content = String.concat "\n" contentLines })
                | None -> ()

                // Start new function
                currentFunction <- Some {
                    Name = name
                    StartLine = i + 1
                    EndLine = i + 1
                    Content = line
                    DeclarationType = pattern.DeclarationType
                }
                braceDepth <- 0

            | None ->
                // Track delimiter depth for function boundaries
                match currentFunction with
                | Some func ->
                    let pattern = compiledPatterns |> List.find (fun (_, p) -> p.DeclarationType = func.DeclarationType) |> snd
                    let openDelims = line |> Seq.filter (fun c -> c = pattern.OpenDelimiter) |> Seq.length
                    let closeDelims = line |> Seq.filter (fun c -> c = pattern.CloseDelimiter) |> Seq.length
                    braceDepth <- braceDepth + openDelims - closeDelims

                    if braceDepth <= 0 && pattern.CloseDelimiter <> '\n' then
                        // Function ended
                        let contentLines = lines.[func.StartLine-1..i]
                        functions.Add({ func with EndLine = i + 1; Content = String.concat "\n" contentLines })
                        currentFunction <- None
                | None -> ()

        // Handle any remaining function
        match currentFunction with
        | Some func ->
            let contentLines = lines.[func.StartLine-1..]
            functions.Add({ func with EndLine = lines.Length; Content = String.concat "\n" contentLines })
        | None -> ()

        functions |> Seq.toList

    /// Merges parallel parsing results from multiple contexts
    let mergeParallelResults (baseContext: ParserContext) (functionResults: (FunctionBoundary * ParserContext) array) : ParserContext =
        let allTokens = ResizeArray<Token>()
        let allASTNodes = ResizeArray<ASTNode>()
        let allErrors = ResizeArray<ErrorInfo>()
        let allTrace = ResizeArray<string>()

        // Track line offset for each function
        let mutable currentLineOffset = 0

        for (func, funcContext) in functionResults do
            let funcState = ParserContextOps.getState funcContext

            // Adjust line numbers for tokens
            let adjustedTokens = funcState.Tokens |> List.map (fun token ->
                { token with Line = token.Line + currentLineOffset })
            allTokens.AddRange(adjustedTokens)

            // Adjust line numbers for errors
            let adjustedErrors = funcState.Errors |> List.map (fun error ->
                { error with Line = error.Line + currentLineOffset })
            allErrors.AddRange(adjustedErrors)

            // AST nodes and trace can be added as-is (they're function-scoped)
            allASTNodes.AddRange(funcState.ASTNodes)
            allTrace.AddRange(funcState.TraceLog)

            // Update line offset for next function
            currentLineOffset <- currentLineOffset + func.Content.Split([|'\r'; '\n'|], System.StringSplitOptions.None).Length

        // Create merged state
        let mergedState = {
            Tokens = allTokens |> Seq.toList
            ASTNodes = allASTNodes |> Seq.toList
            Errors = allErrors |> Seq.toList
            TraceLog = allTrace |> Seq.toList
            UserData = (ParserContextOps.getState baseContext).UserData  // Keep base user data
        }

        // Update final context
        let finalContext = ParserContextOps.setState mergedState baseContext
        ParserContextOps.updatePosition (baseContext.Line + currentLineOffset) 1 finalContext

    /// Parses functions in parallel when there are enough independent functions
    let parseStringParallel (config: ParserConfig) (input: string) : ParserContext =
        if not config.ParallelConfig.EnableParallelParsing then
            // Call sequential version directly to avoid circular dependency
            let initialContext = ParserContextOps.create "<string>" config.EnableTrace
            let lines = input.Split([|'\r'; '\n'|], System.StringSplitOptions.None)
            processInputParallel config initialContext lines
        else
            let functions = identifyFunctionBoundaries input

            if functions.Length < config.ParallelConfig.MinFunctionsForParallelism then
                // Not enough functions for parallel benefit - use sequential
                let initialContext = ParserContextOps.create "<string>" config.EnableTrace
                let lines = input.Split([|'\r'; '\n'|], System.StringSplitOptions.None)
                processInputParallel config initialContext lines
            else
                // Parse functions in parallel
                let functionResults =
                    functions
                    |> Array.ofList
                    |> Array.Parallel.map (fun func ->
                        // Create isolated config for this function (disable parallel to avoid recursion)
                        let isolatedConfig = { config with ParallelConfig = { config.ParallelConfig with EnableParallelParsing = false } }
                        // Parse function content sequentially
                        let funcContext = ParserContextOps.create "<function>" config.EnableTrace
                        let funcLines = func.Content.Split([|'\r'; '\n'|], System.StringSplitOptions.None)
                        let processedContext = processInputParallel isolatedConfig funcContext funcLines
                        (func, processedContext)
                    )

                // Create base context for merging
                let baseContext = ParserContextOps.create "<string>" config.EnableTrace

                // Merge all parallel results
                mergeParallelResults baseContext functionResults

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

        // Use parallel processing if enabled and beneficial
        if config.ParallelConfig.EnableParallelParsing then
            parseStringParallel config input
        else
            // Process input (with optional parallel tokenization)
            processInputParallel config initialContext lines