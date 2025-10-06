namespace SharpParser.Core

/// Configuration for the parser including all settings and handlers
type ParserConfig = {
    /// Registry containing all registered handlers
    Registry: HandlerRegistry
    /// Whether to collect tokens during parsing
    EnableTokens: bool
    /// Whether to build AST nodes during parsing
    EnableAST: bool
    /// Whether to log trace messages during parsing
    EnableTrace: bool
    /// Current mode context for handler registration (used by inMode)
    CurrentModeContext: string option
    /// Set of keywords for token classification
    Keywords: Set<string>
    /// Custom AST builders for flexible node construction
    CustomASTBuilders: Map<string option, ASTBuilderFunc list>
    /// Configuration for parallel parsing features
    ParallelConfig: ParallelConfig
}

/// Module for parser configuration operations
module ParserConfig =
    /// Common keywords for simple programming languages
    let private commonKeywords = set [ "function"; "if"; "else"; "while"; "for"; "return"; "var"; "let"; "const" ]

    /// Default parallel configuration (disabled by default)
    let private defaultParallelConfig : ParallelConfig = {
        EnableParallelParsing = false
        MaxParallelism = System.Environment.ProcessorCount
        MinFunctionsForParallelism = 3
        EnableParallelTokenization = false
    }

    /// Creates a default parser configuration
    let create () : ParserConfig =
        {
            Registry = HandlerRegistry.empty ()
            EnableTokens = false
            EnableAST = false
            EnableTrace = false
            CurrentModeContext = None
            Keywords = Set.empty
            CustomASTBuilders = Map.empty
            ParallelConfig = defaultParallelConfig
        }

    /// Adds a character handler for the current mode context
    let withCharHandler (character: char) (handler: CharHandler) (config: ParserConfig) : ParserConfig =
        let updatedRegistry = HandlerRegistry.addCharHandler config.CurrentModeContext character handler config.Registry
        { config with Registry = updatedRegistry }

    /// Adds a sequence handler for the current mode context
    let withSequenceHandler (sequence: string) (handler: SequenceHandler) (config: ParserConfig) : ParserConfig =
        let updatedRegistry = HandlerRegistry.addSequenceHandler config.CurrentModeContext sequence handler config.Registry
        { config with Registry = updatedRegistry }

    /// Adds a pattern handler for the current mode context
    let withPatternHandler (pattern: string) (handler: PatternHandler) (config: ParserConfig) : ParserConfig =
        let updatedRegistry = HandlerRegistry.addPatternHandler config.CurrentModeContext pattern handler config.Registry
        { config with Registry = updatedRegistry }

    /// Adds a global error handler
    let withErrorHandler (handler: ErrorHandler) (config: ParserConfig) : ParserConfig =
        let updatedRegistry = HandlerRegistry.addErrorHandler handler config.Registry
        { config with Registry = updatedRegistry }

    /// Temporarily sets the mode context for nested handler registration
    let withModeContext (mode: string) (nestedConfig: ParserConfig -> ParserConfig) (config: ParserConfig) : ParserConfig =
        let originalContext = config.CurrentModeContext
        let configWithNewContext = { config with CurrentModeContext = Some mode }
        let configAfterNested = nestedConfig configWithNewContext
        { configAfterNested with CurrentModeContext = originalContext }

    /// Enables or disables tokenization
    let withTokens (enabled: bool) (config: ParserConfig) : ParserConfig =
        let keywords = if enabled then commonKeywords else Set.empty
        { config with EnableTokens = enabled; Keywords = keywords }

    /// Enables or disables AST building
    let withAST (enabled: bool) (config: ParserConfig) : ParserConfig =
        { config with EnableAST = enabled }

    /// Enables or disables tracing
    let withTrace (enabled: bool) (config: ParserConfig) : ParserConfig =
        { config with EnableTrace = enabled }

    /// Gets the handler registry from the configuration
    let getRegistry (config: ParserConfig) : HandlerRegistry =
        config.Registry

    /// Checks if tokenization is enabled
    let isTokenizationEnabled (config: ParserConfig) : bool =
        config.EnableTokens

    /// Checks if AST building is enabled
    let isASTBuildingEnabled (config: ParserConfig) : bool =
        config.EnableAST

    /// Checks if tracing is enabled
    let isTracingEnabled (config: ParserConfig) : bool =
        config.EnableTrace

    /// Gets the current mode context
    let getCurrentModeContext (config: ParserConfig) : string option =
        config.CurrentModeContext

    /// Adds a custom AST builder for the current mode context
    let withASTBuilder (builder: ASTBuilderFunc) (config: ParserConfig) : ParserConfig =
        let modeKey = config.CurrentModeContext
        let builders =
            match Map.tryFind modeKey config.CustomASTBuilders with
            | Some existingBuilders -> builder :: existingBuilders
            | None -> [builder]

        { config with CustomASTBuilders = Map.add modeKey builders config.CustomASTBuilders }

    /// Gets custom AST builders for a specific mode
    let getASTBuilders (mode: string option) (config: ParserConfig) : ASTBuilderFunc list =
        // Try specific mode first, then fall back to global mode
        let tryMode targetMode =
            match Map.tryFind targetMode config.CustomASTBuilders with
            | Some builders -> builders
            | None -> []

        let specificBuilders = tryMode mode
        let globalBuilders = tryMode None
        specificBuilders @ globalBuilders

    /// Enables or disables parallel parsing
    let withParallelParsing (enabled: bool) (config: ParserConfig) : ParserConfig =
        let parallelConfig = { config.ParallelConfig with EnableParallelParsing = enabled }
        { config with ParallelConfig = parallelConfig }

    /// Sets the maximum parallelism level
    let withMaxParallelism (maxParallelism: int) (config: ParserConfig) : ParserConfig =
        let parallelConfig = { config.ParallelConfig with MaxParallelism = maxParallelism }
        { config with ParallelConfig = parallelConfig }

    /// Sets the minimum functions threshold for enabling parallelism
    let withMinFunctionsForParallelism (minFunctions: int) (config: ParserConfig) : ParserConfig =
        let parallelConfig = { config.ParallelConfig with MinFunctionsForParallelism = minFunctions }
        { config with ParallelConfig = parallelConfig }

    /// Enables or disables parallel tokenization
    let withParallelTokenization (enabled: bool) (config: ParserConfig) : ParserConfig =
        let parallelConfig = { config.ParallelConfig with EnableParallelTokenization = enabled }
        { config with ParallelConfig = parallelConfig }

    /// Gets the parallel configuration
    let getParallelConfig (config: ParserConfig) : ParallelConfig =
        config.ParallelConfig

    /// Checks if parallel parsing is enabled
    let isParallelParsingEnabled (config: ParserConfig) : bool =
        config.ParallelConfig.EnableParallelParsing

    /// Checks if parallel tokenization is enabled
    let isParallelTokenizationEnabled (config: ParserConfig) : bool =
        config.ParallelConfig.EnableParallelTokenization