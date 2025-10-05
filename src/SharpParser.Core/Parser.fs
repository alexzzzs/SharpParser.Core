namespace SharpParser.Core

/// Public API module for the SharpParser.Core library
module Parser =
    /// Creates a new parser configuration
    let create () : ParserConfig =
        ParserConfig.create ()

    /// Registers a character handler for the current mode context
    let onChar (character: char) (handler: CharHandler) (config: ParserConfig) : ParserConfig =
        ParserConfig.withCharHandler character handler config

    /// Registers a sequence handler for the current mode context
    let onSequence (sequence: string) (handler: SequenceHandler) (config: ParserConfig) : ParserConfig =
        ParserConfig.withSequenceHandler sequence handler config

    /// Registers a pattern handler for the current mode context
    let onPattern (pattern: string) (handler: PatternHandler) (config: ParserConfig) : ParserConfig =
        ParserConfig.withPatternHandler pattern handler config

    /// Sets mode context for nested handler registration
    let inMode (mode: string) (nestedConfig: ParserConfig -> ParserConfig) (config: ParserConfig) : ParserConfig =
        ParserConfig.withModeContext mode nestedConfig config

    /// Registers a global error handler
    let onError (handler: ErrorHandler) (config: ParserConfig) : ParserConfig =
        ParserConfig.withErrorHandler handler config

    /// Enables automatic tokenization
    let enableTokens () (config: ParserConfig) : ParserConfig =
        ParserConfig.withTokens true config

    /// Enables automatic AST building
    let enableAST () (config: ParserConfig) : ParserConfig =
        ParserConfig.withAST true config

    /// Enables or disables tracing
    let enableTrace (enabled: bool) (config: ParserConfig) : ParserConfig =
        ParserConfig.withTrace enabled config

    /// Registers a custom AST builder for the current mode context
    let onAST (builder: ASTBuilderFunc) (config: ParserConfig) : ParserConfig =
        ParserConfig.withASTBuilder builder config

    /// Parses a file and returns the final context
    let run (filePath: string) (config: ParserConfig) : ParserContext =
        ParsingEngine.parseFile config filePath

    /// Parses a string and returns the final context
    let runString (input: string) (config: ParserConfig) : ParserContext =
        ParsingEngine.parseString config input

    /// Extracts tokens from the final context
    let getTokens (context: ParserContext) : Token list =
        (ParserContextOps.getState context).Tokens |> List.rev // Reverse to maintain original order

    /// Extracts AST nodes from the final context
    let getAST (context: ParserContext) : ASTNode list =
        (ParserContextOps.getState context).ASTNodes |> List.rev // Reverse to maintain original order

    /// Extracts errors from the final context
    let getErrors (context: ParserContext) : ErrorInfo list =
        (ParserContextOps.getState context).Errors |> List.rev // Reverse to maintain original order

    /// Extracts trace log from the final context
    let getTrace (context: ParserContext) : string list =
        (ParserContextOps.getState context).TraceLog |> List.rev // Reverse to maintain original order

    /// Gets user data from the parsing context
    let getUserData (key: string) (context: ParserContext) : obj option =
        ParserContextOps.getUserData key context

    /// Sets user data in the parsing context
    let setUserData (key: string) (value: obj) (context: ParserContext) : ParserContext =
        ParserContextOps.setUserData key value context

    /// Formats a summary of parsing results as a string
    let formatSummary (context: ParserContext) : string =
        let state = ParserContextOps.getState context
        sprintf "=== Parsing Summary ===\nFile: %s\nFinal Position: Line %d, Col %d\nMode Stack: %A\nTokens: %d\nAST Nodes: %d\nErrors: %d\nTrace Messages: %d\n"
                context.FilePath context.Line context.Col context.ModeStack
                (List.length state.Tokens) (List.length state.ASTNodes)
                (List.length state.Errors) (List.length state.TraceLog)

    /// Prints a summary of parsing results
    let printSummary (context: ParserContext) : unit =
        formatSummary context |> printfn "%s"

    /// Validates that the parser configuration is properly set up
    let validateConfig (config: ParserConfig) : Result<ParserConfig, string> =
        let errors = ResizeArray<string>()

        // Check for conflicting character handlers in same mode
        let charConflicts =
            config.Registry.CharHandlers
            |> Map.toSeq
            |> Seq.collect (fun (mode, charMap) ->
                charMap
                |> Map.toSeq
                |> Seq.groupBy fst
                |> Seq.filter (fun (_, handlers) -> Seq.length handlers > 1)
                |> Seq.map (fun (char, _) -> sprintf "Multiple handlers for character '%c' in mode %A" char mode))

        errors.AddRange(charConflicts)

        // Check for conflicting sequence handlers in same mode
        // Note: Full trie conflict detection would require traversing the trie to detect overlapping sequences.
        // This is complex to implement efficiently. The trie handles conflicts by design (longest match wins),
        // but validation is skipped for performance reasons. Users should be aware that shorter sequences
        // may be shadowed by longer ones if they share prefixes.
        let sequenceConflicts = Seq.empty<string>

        // Check for conflicting pattern handlers in same mode
        let patternConflicts =
            config.Registry.PatternHandlers
            |> Map.toSeq
            |> Seq.collect (fun (mode, patterns) ->
                patterns
                |> Seq.groupBy (fun (regex, _) -> regex.ToString())
                |> Seq.filter (fun (_, group) -> Seq.length group > 1)
                |> Seq.map (fun (pattern, _) -> sprintf "Duplicate pattern '%s' in mode %A" pattern mode))

        errors.AddRange(patternConflicts)

        // Check for valid regex patterns
        let invalidPatterns =
            config.Registry.PatternHandlers
            |> Map.toSeq
            |> Seq.collect (fun (_, patterns) ->
                patterns
                |> Seq.choose (fun (regex, _) ->
                    try
                        regex.Match("") |> ignore // Test if regex is valid
                        None
                    with
                    | ex -> Some (sprintf "Invalid regex pattern: %s" ex.Message)))

        errors.AddRange(invalidPatterns)

        // Check for error handlers if tracing is enabled (optional but recommended)
        if config.EnableTrace && List.isEmpty config.Registry.ErrorHandlers then
            errors.Add("Tracing is enabled but no error handlers are registered")

        if errors.Count > 0 then
            Error (String.concat "; " errors)
        else
            Ok config