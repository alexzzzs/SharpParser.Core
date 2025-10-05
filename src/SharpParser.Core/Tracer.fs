namespace SharpParser.Core

/// Module for tracing and debugging functionality
module Tracer =
    /// Checks if tracing is enabled
    let isTracingEnabled (context: ParserContext) : bool =
        context.EnableTrace

    /// Adds a trace message if tracing is enabled
    let trace (message: string) (context: ParserContext) : ParserContext =
        if isTracingEnabled context then
            ParserContextOps.addTrace message context
        else
            context

    /// Traces character processing
    let traceChar (character: char) (context: ParserContext) : ParserContext =
        let mode = ParserContextOps.currentMode context |> Option.defaultValue "default"
        let message = sprintf "[%s] Processing char '%c' at line %d, col %d"
                       mode character context.Line context.Col
        trace message context

    /// Traces sequence match
    let traceSequence (sequence: string) (context: ParserContext) : ParserContext =
        let mode = ParserContextOps.currentMode context |> Option.defaultValue "default"
        let message = sprintf "[%s] Matched sequence '%s' at line %d, col %d"
                       mode sequence context.Line context.Col
        trace message context

    /// Traces pattern match
    let tracePattern (pattern: string) (matchedText: string) (context: ParserContext) : ParserContext =
        let mode = ParserContextOps.currentMode context |> Option.defaultValue "default"
        let message = sprintf "[%s] Matched pattern '%s' -> '%s' at line %d, col %d"
                       mode pattern matchedText context.Line context.Col
        trace message context

    /// Traces mode entry
    let traceModeEnter (mode: string) (context: ParserContext) : ParserContext =
        let message = sprintf "Entering mode '%s' at line %d, col %d"
                       mode context.Line context.Col
        trace message context

    /// Traces mode exit
    let traceModeExit (mode: string) (context: ParserContext) : ParserContext =
        let message = sprintf "Exiting mode '%s' at line %d, col %d"
                       mode context.Line context.Col
        trace message context

    /// Traces token emission
    let traceToken (token: Token) (context: ParserContext) : ParserContext =
        let mode = token.Mode |> Option.defaultValue "default"
        let message = sprintf "[%s] Emitted token %A at line %d, col %d"
                       mode token.Type token.Line token.Col
        trace message context

    /// Traces AST node creation
    let traceAST (node: ASTNode) (context: ParserContext) : ParserContext =
        let mode = ParserContextOps.currentMode context |> Option.defaultValue "default"
        let message = sprintf "[%s] Created AST node %A at line %d, col %d"
                       mode node context.Line context.Col
        trace message context

    /// Formats all trace messages as a string
    let formatTrace (context: ParserContext) : string =
        let state = ParserContextOps.getState context
        if List.isEmpty state.TraceLog then
            "No trace messages."
        else
            let traceLines =
                state.TraceLog
                |> List.rev // Reverse to show in chronological order
                |> List.map (fun message -> sprintf "  %s" message)
            "Trace Log:\n" + String.concat "\n" traceLines

    /// Prints all trace messages to the console
    let printTrace (context: ParserContext) : unit =
        formatTrace context |> printfn "%s"

    /// Gets all trace messages from the context
    let getTrace (context: ParserContext) : string list =
        (ParserContextOps.getState context).TraceLog

    /// Gets the count of trace messages
    let getTraceCount (context: ParserContext) : int =
        List.length (ParserContextOps.getState context).TraceLog

    /// Clears all trace messages from the context
    let clearTrace (context: ParserContext) : ParserContext =
        let currentState = ParserContextOps.getState context
        let newState = { currentState with TraceLog = [] }
        ParserContextOps.setState newState context
