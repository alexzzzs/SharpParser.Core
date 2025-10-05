namespace SharpParser.Core

/// Module for error handling and reporting
module ErrorHandling =
    /// Triggers error handling by invoking all registered error handlers
    let triggerError (errorType: ParseError) (config: ParserConfig) (context: ParserContext) (suggestion: string option) : ParserContext =
        // Invoke all registered error handlers
        let errorHandlers = HandlerRegistry.getErrorHandlers config.Registry

        // Create error message
        let message =
            match errorType with
            | UnexpectedChar c -> sprintf "Unexpected character '%c'" c
            | UnexpectedSequence s -> sprintf "Unexpected sequence '%s'" s
            | InvalidSyntax msg -> msg
            | ModeError mode -> sprintf "Mode error in '%s'" mode
            | ConfigError msg -> msg
            | GenericError msg -> msg

        // Create error info
        let errorInfo = {
            ErrorType = errorType
            Line = context.Line
            Col = context.Col
            Mode = ParserContextOps.currentMode context
            Message = message
            Suggestion = suggestion
        }

        // Record the error first
        let errorContext = ParserContextOps.addError errorInfo context

        // Invoke all error handlers if any are registered
        if List.isEmpty errorHandlers then
            errorContext
        else
            let message = errorInfo.Message
            errorHandlers |> List.fold (fun ctx handler -> handler ctx message) errorContext

    /// Formats an error message with line and column information
    let formatError (line: int) (col: int) (message: string) : string =
        sprintf "Error at line %d, column %d: %s" line col message

    /// Generates an "unexpected character" error message
    let unexpectedChar (character: char) (context: ParserContext) : string =
        let formatted = formatError context.Line context.Col (sprintf "Unexpected character '%c'" character)
        formatted

    /// Generates an "unexpected sequence" error message
    let unexpectedSequence (sequence: string) (context: ParserContext) : string =
        let formatted = formatError context.Line context.Col (sprintf "Unexpected sequence '%s'" sequence)
        formatted

    /// Generates a mode-related error message
    let modeError (mode: string) (context: ParserContext) : string =
        let formatted = formatError context.Line context.Col (sprintf "Mode error in '%s'" mode)
        formatted

    /// Generates a generic parsing error message
    let parsingError (message: string) (context: ParserContext) : string =
        let formatted = formatError context.Line context.Col message
        formatted

    /// Checks if there are any errors in the parsing context
    let hasErrors (context: ParserContext) : bool =
        not (List.isEmpty (ParserContextOps.getState context).Errors)

    /// Gets all errors from the parsing context
    let getErrors (context: ParserContext) : ErrorInfo list =
        (ParserContextOps.getState context).Errors

    /// Gets the count of errors in the parsing context
    let getErrorCount (context: ParserContext) : int =
        List.length (ParserContextOps.getState context).Errors

    /// Formats all errors as a string
    let formatErrors (context: ParserContext) : string =
        if hasErrors context then
            let errorLines =
                (ParserContextOps.getState context).Errors
                |> List.rev // Reverse to show in chronological order
                |> List.map (fun errorInfo ->
                    let baseMsg = sprintf "  Line %d, Col %d: %s" errorInfo.Line errorInfo.Col errorInfo.Message
                    match errorInfo.Suggestion with
                    | Some suggestion -> baseMsg + sprintf " (Suggestion: %s)" suggestion
                    | None -> baseMsg)
            "Parsing Errors:\n" + String.concat "\n" errorLines
        else
            "No parsing errors."

    /// Prints all errors to the console
    let printErrors (context: ParserContext) : unit =
        formatErrors context |> printfn "%s"

    /// Clears all errors from the parsing context
    let clearErrors (context: ParserContext) : ParserContext =
        let currentState = ParserContextOps.getState context
        let newState = { currentState with Errors = [] }
        ParserContextOps.setState newState context