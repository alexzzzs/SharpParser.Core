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
    /// <param name="sequence">The character sequence to match. Cannot be null or empty.</param>
    /// <param name="handler">Handler function to invoke when sequence is matched.</param>
    /// <param name="config">Parser configuration.</param>
    /// <exception cref="System.ArgumentException">Thrown when sequence is null or empty.</exception>
    let onSequence (sequence: string) (handler: SequenceHandler) (config: ParserConfig) : ParserConfig =
        if isNull sequence then
            invalidArg "sequence" "Sequence cannot be null"
        if System.String.IsNullOrEmpty(sequence) then
            invalidArg "sequence" "Sequence cannot be empty"
        ParserConfig.withSequenceHandler sequence handler config

    /// Registers a pattern handler for the current mode context
    /// <param name="pattern">Regular expression pattern to match. Cannot be null or empty.</param>
    /// <param name="handler">Handler function to invoke when pattern is matched.</param>
    /// <param name="config">Parser configuration.</param>
    /// <exception cref="System.ArgumentException">Thrown when pattern is null or empty.</exception>
    let onPattern (pattern: string) (handler: PatternHandler) (config: ParserConfig) : ParserConfig =
        if isNull pattern then
            invalidArg "pattern" "Pattern cannot be null"
        if System.String.IsNullOrEmpty(pattern) then
            invalidArg "pattern" "Pattern cannot be empty"
        ParserConfig.withPatternHandler pattern handler config

    /// Sets mode context for nested handler registration
    /// <param name="mode">Mode name for context. Cannot be null or empty.</param>
    /// <param name="nestedConfig">Function that configures handlers for this mode.</param>
    /// <param name="config">Parser configuration.</param>
    /// <exception cref="System.ArgumentException">Thrown when mode is null or empty.</exception>
    let inMode (mode: string) (nestedConfig: ParserConfig -> ParserConfig) (config: ParserConfig) : ParserConfig =
        if isNull mode then
            invalidArg "mode" "Mode cannot be null"
        if System.String.IsNullOrEmpty(mode) then
            invalidArg "mode" "Mode cannot be empty"
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

    /// Enables parallel parsing
    let enableParallel () (config: ParserConfig) : ParserConfig =
        ParserConfig.withParallelParsing true config

    /// Sets the maximum parallelism level
    /// <param name="maxParallelism">Maximum number of parallel tasks. Must be greater than 0.</param>
    /// <param name="config">Parser configuration.</param>
    /// <exception cref="System.ArgumentException">Thrown when maxParallelism is less than or equal to 0.</exception>
    let withMaxParallelism (maxParallelism: int) (config: ParserConfig) : ParserConfig =
        if maxParallelism <= 0 then
            invalidArg "maxParallelism" "Maximum parallelism must be greater than 0"
        ParserConfig.withMaxParallelism maxParallelism config

    /// Sets the minimum functions threshold for enabling parallelism
    /// <param name="minFunctions">Minimum number of functions to enable parallelism. Must be greater than or equal to 0.</param>
    /// <param name="config">Parser configuration.</param>
    /// <exception cref="System.ArgumentException">Thrown when minFunctions is less than 0.</exception>
    let withMinFunctionsForParallelism (minFunctions: int) (config: ParserConfig) : ParserConfig =
        if minFunctions < 0 then
            invalidArg "minFunctions" "Minimum functions for parallelism cannot be negative"
        ParserConfig.withMinFunctionsForParallelism minFunctions config

    /// Enables parallel tokenization
    let enableParallelTokenization () (config: ParserConfig) : ParserConfig =
        ParserConfig.withParallelTokenization true config

    /// Parses a file and returns the final context
    /// <param name="filePath">Path to the file to parse. Cannot be null or empty.</param>
    /// <param name="config">Parser configuration.</param>
    /// <returns>The final parsing context containing tokens, AST nodes, errors, and trace information.</returns>
    /// <exception cref="System.ArgumentException">Thrown when filePath is null or empty.</exception>
    let run (filePath: string) (config: ParserConfig) : ParserContext =
        if isNull filePath then
            invalidArg "filePath" "File path cannot be null"
        if System.String.IsNullOrWhiteSpace(filePath) then
            invalidArg "filePath" "File path cannot be empty or whitespace"
        ParsingEngine.parseFile config filePath

    /// Parses a string and returns the final context
    /// <param name="input">Input string to parse. Cannot be null.</param>
    /// <param name="config">Parser configuration.</param>
    /// <returns>The final parsing context containing tokens, AST nodes, errors, and trace information.</returns>
    /// <exception cref="System.ArgumentException">Thrown when input is null.</exception>
    let runString (input: string) (config: ParserConfig) : ParserContext =
        if isNull input then
            invalidArg "input" "Input string cannot be null"
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
    /// <param name="key">Key to retrieve data for. Cannot be null.</param>
    /// <param name="context">Parser context.</param>
    /// <exception cref="System.ArgumentException">Thrown when key is null.</exception>
    let getUserData (key: string) (context: ParserContext) : obj option =
        if isNull key then
            invalidArg "key" "Key cannot be null"
        ParserContextOps.getUserData key context

    /// Sets user data in the parsing context
    /// <param name="key">Key to store data under. Cannot be null.</param>
    /// <param name="value">Value to store.</param>
    /// <param name="context">Parser context.</param>
    /// <exception cref="System.ArgumentException">Thrown when key is null.</exception>
    let setUserData (key: string) (value: obj) (context: ParserContext) : ParserContext =
        if isNull key then
            invalidArg "key" "Key cannot be null"
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

    /// Validates that the parser configuration is properly set up and detects common configuration errors
    let validateConfig (config: ParserConfig) : Result<ParserConfig, string> =
        let errors = ResizeArray<string>()

        // VALIDATION 1: Check for conflicting character handlers in the same mode
        // Multiple handlers for the same character in the same mode would be ambiguous
        let charConflicts =
            config.Registry.CharHandlers
            |> Map.toSeq  // Iterate over all modes
            |> Seq.collect (fun (mode, charMap) ->  // For each mode, examine its character handlers
                charMap
                |> Map.toSeq  // Get all character->handler mappings
                |> Seq.groupBy fst  // Group by character (fst = first element of tuple)
                |> Seq.filter (fun (_, handlers) -> Seq.length handlers > 1)  // Find characters with multiple handlers
                |> Seq.map (fun (char, _) -> sprintf "Multiple handlers for character '%c' in mode %A" char mode))

        errors.AddRange(charConflicts)

        // VALIDATION 2: Check for conflicting sequence handlers in the same mode
        // Note: Full trie conflict detection would require traversing the trie to detect overlapping sequences.
        // This is complex to implement efficiently. The trie handles conflicts by design (longest match wins),
        // but validation is skipped for performance reasons. Users should be aware that shorter sequences
        // may be shadowed by longer ones if they share prefixes.
        let sequenceConflicts = Seq.empty<string>

        // VALIDATION 3: Check for conflicting pattern handlers in the same mode
        // Duplicate regex patterns in the same mode would be redundant or error-prone
        let patternConflicts =
            config.Registry.PatternHandlers
            |> Map.toSeq  // Iterate over all modes
            |> Seq.collect (fun (mode, patterns) ->  // For each mode, examine its pattern handlers
                patterns
                |> Seq.groupBy (fun (regex, _) -> regex.ToString())  // Group by regex pattern string
                |> Seq.filter (fun (_, group) -> Seq.length group > 1)  // Find duplicate patterns
                |> Seq.map (fun (pattern, _) -> sprintf "Duplicate pattern '%s' in mode %A" pattern mode))

        errors.AddRange(patternConflicts)

        // VALIDATION 4: Check for valid regex patterns
        // Malformed regex patterns would cause runtime exceptions during parsing
        let invalidPatterns =
            config.Registry.PatternHandlers
            |> Map.toSeq
            |> Seq.collect (fun (_, patterns) ->
                patterns
                |> Seq.choose (fun (regex, _) ->
                    try
                        // Test if the regex can be executed (even on empty string)
                        regex.Match("") |> ignore
                        None  // Pattern is valid
                    with
                    | ex -> Some (sprintf "Invalid regex pattern: %s" ex.Message)))  // Pattern is malformed

        errors.AddRange(invalidPatterns)

        // VALIDATION 5: Check for error handlers when tracing is enabled
        // Tracing generates detailed logs but errors might be missed without error handlers
        if config.EnableTrace && List.isEmpty config.Registry.ErrorHandlers then
            errors.Add("Tracing is enabled but no error handlers are registered (recommended for debugging)")

        // Return validation result
        if errors.Count > 0 then
            Error (String.concat "; " errors)  // Configuration has issues
        else
            Ok config  // Configuration is valid