namespace SharpParser.Core

/// Module for parser context operations
module ParserContextOps =
    /// Creates an initial parsing context for a file
    let create (filePath: string) (enableTrace: bool) : ParserContext =
        {
            Line = 1
            Col = 1
            FilePath = filePath
            CurrentChar = None
            Buffer = ""
            ModeStack = []
            State = {
                Tokens = []
                ASTNodes = []
                Errors = []
                TraceLog = []
                UserData = Map.empty
            }
            EnableTrace = enableTrace
        }

    /// Pushes a new mode onto the mode stack
    let enterMode (mode: string) (context: ParserContext) : ParserContext =
        { context with ModeStack = mode :: context.ModeStack }

    /// Pops a mode from the mode stack
    let exitMode (context: ParserContext) : ParserContext =
        match context.ModeStack with
        | [] -> context // Cannot exit if no modes on stack
        | _ :: remainingModes -> { context with ModeStack = remainingModes }

    /// Gets the current parsing mode (head of stack)
    let currentMode (context: ParserContext) : string option =
        match context.ModeStack with
        | [] -> None
        | currentMode :: _ -> Some currentMode

    /// Updates the current line and column position
    let updatePosition (line: int) (col: int) (context: ParserContext) : ParserContext =
        { context with Line = line; Col = col }

    /// Updates the current character being processed
    let updateChar (character: char option) (context: ParserContext) : ParserContext =
        { context with CurrentChar = character }

    /// Updates the current line buffer
    let updateBuffer (buffer: string) (context: ParserContext) : ParserContext =
        { context with Buffer = buffer }

    /// Adds a token to the parsing state
    let addToken (token: Token) (context: ParserContext) : ParserContext =
        let newState = { context.State with Tokens = token :: context.State.Tokens }
        { context with State = newState }

    /// Adds an AST node to the parsing state
    let addASTNode (node: ASTNode) (context: ParserContext) : ParserContext =
        let newState = { context.State with ASTNodes = node :: context.State.ASTNodes }
        { context with State = newState }

    /// Records an error at the current position
    let addError (errorInfo: ErrorInfo) (context: ParserContext) : ParserContext =
        let newState = { context.State with Errors = errorInfo :: context.State.Errors }
        { context with State = newState }

    /// Records a simple error message at the current position
    let addSimpleError (message: string) (context: ParserContext) : ParserContext =
        let errorInfo = {
            ErrorType = ParseError.GenericError message
            Line = context.Line
            Col = context.Col
            Mode = currentMode context
            Message = message
            Suggestion = None
        }
        addError errorInfo context

    /// Adds a trace message to the parsing state
    let addTrace (message: string) (context: ParserContext) : ParserContext =
        let newState = { context.State with TraceLog = message :: context.State.TraceLog }
        { context with State = newState }

    /// Stores user-defined data in the parsing context
    let setUserData (key: string) (value: obj) (context: ParserContext) : ParserContext =
        let newState = { context.State with UserData = Map.add key value context.State.UserData }
        { context with State = newState }

    /// Retrieves user-defined data from the parsing context
    let getUserData (key: string) (context: ParserContext) : obj option =
        Map.tryFind key context.State.UserData

    /// Gets the current parsing state
    let getState (context: ParserContext) : ParserState =
        context.State

    /// Updates the entire parsing state
    let setState (state: ParserState) (context: ParserContext) : ParserContext =
        { context with State = state }