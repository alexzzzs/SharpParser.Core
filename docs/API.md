# SharpParser.Core API Documentation

## Overview

SharpParser.Core is a beginner-friendly, event-driven F# parsing library that provides a fluent API for building parsers with support for character handlers, sequence handlers, pattern handlers, and context-sensitive parsing modes.

## Quickstart

Here's a minimal working example to get you started:

```fsharp
open SharpParser.Core

let parser =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.onSequence "print" (fun ctx -> printfn "Print statement detected"; ctx)
    |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx id ->
        printfn "Identifier: %s" id
        ctx)

let context = Parser.runString "print hello" parser

Parser.getTokens context |> List.iter (printfn "%A")
```

This creates a parser that recognizes "print" statements and identifiers, runs it on a string, and prints the collected tokens.

## Core Types

### TokenType
Discriminated union representing different types of tokens:

```fsharp
type TokenType =
    | Keyword of string      // Keywords like "function", "if", "else"
    | Identifier of string   // Variable and function names
    | Symbol of string       // Operators and symbols like "+", "=", "{"
    | Number of float        // Numeric literals
    | StringLiteral of string // String literals
    | EOF                    // End of file marker
```

### Token
Record type representing a token with position information:

```fsharp
type Token = {
    Type: TokenType      // The token type
    Line: int           // Line number (1-based)
    Col: int            // Column number (1-based)
    Mode: string option // Parsing mode when token was created
}
```

### ASTNode
Discriminated union representing nodes in the Abstract Syntax Tree:

```fsharp
type ASTNode =
    | Function of string * string list * ASTNode list  // Function definition (name, parameters, body)
    | If of ASTNode * ASTNode list * ASTNode list option  // If statement (condition, then body, optional else body)
    | While of ASTNode * ASTNode list   // While loop (condition, body)
    | For of ASTNode option * ASTNode option * ASTNode option * ASTNode list  // For loop (init, condition, increment, body)
    | Block of ASTNode list             // Block of statements
    | Assignment of string * ASTNode    // Variable assignment
    | Return of ASTNode option          // Return statement
    | Call of string * ASTNode list     // Function call (name, arguments)
    | Expression of string              // Expression
    | Literal of string                 // Literal value
    | BinaryOp of ASTNode * string * ASTNode  // Binary operation (left op right)
    | UnaryOp of string * ASTNode       // Unary operation (op operand)
    | Variable of string                // Variable reference
    | Number of float                   // Number literal
    | StringLiteral of string           // String literal
    | Boolean of bool                   // Boolean literal
```

### ParseError
Discriminated union representing different types of parsing errors:

```fsharp
type ParseError =
    | UnexpectedChar of char         // Unexpected character encountered
    | UnexpectedSequence of string   // Unexpected sequence encountered
    | InvalidSyntax of string        // Invalid token or syntax
    | ModeError of string            // Mode-related error
    | ConfigError of string          // Configuration error
    | GenericError of string         // Generic parsing error
```

### ErrorInfo
Record type representing an error with additional context:

```fsharp
type ErrorInfo = {
    ErrorType: ParseError    // The type of error
    Line: int               // Line number where error occurred
    Col: int                // Column number where error occurred
    Mode: string option     // Current parsing mode when error occurred
    Message: string         // Error message
    Suggestion: string option // Optional suggestion for fixing the error
}
```

### ParserState
Record type representing the mutable state during parsing:

```fsharp
type ParserState = {
    Tokens: Token list      // List of tokens collected during parsing
    ASTNodes: ASTNode list  // List of AST nodes built during parsing
    Errors: ErrorInfo list  // List of errors encountered during parsing
    TraceLog: string list   // List of trace messages for debugging
    UserData: Map<string, obj> // User-defined data storage
}
```

### ParserContext
Record type representing the current parsing context and state:

```fsharp
type ParserContext = {
    Line: int               // Current line number (1-based)
    Col: int                // Current column number (1-based)
    FilePath: string        // Source file path or identifier
    CurrentChar: char option // Current character being processed
    Buffer: string          // Current line buffer
    ModeStack: string list  // Stack of active parsing modes
    State: ParserState      // Mutable parsing state
    EnableTrace: bool       // Whether tracing is enabled
}
```

### Handler Types
Type aliases for different handler function signatures:

```fsharp
type CharHandler = ParserContext -> ParserContext                    // Character handlers
type SequenceHandler = ParserContext -> ParserContext                // Sequence handlers
type PatternHandler = ParserContext -> string -> ParserContext       // Pattern handlers (receives matched text)
type ErrorHandler = ParserContext -> string -> ParserContext         // Error handlers (receives error message)
type ASTBuilderFunc = string option -> string -> ParserContext -> ASTNode option  // Custom AST builders
```

## Parser Module (Main API)

The `Parser` module provides the main fluent API for configuring and running parsers.

### Handler Execution Model

When the parser scans input, handlers are resolved in this priority order:

1. **Sequence handlers** (`onSequence`) are checked first, matching multi-character keywords or operators using efficient trie-based lookup.
2. If no sequence matches, **pattern handlers** (`onPattern`) are tested using regex-based matching.
3. If no pattern matches, **character handlers** (`onChar`) are applied to the current character.

This priority order allows broad matches (like keywords) to take precedence over granular ones (single characters), preventing conflicts where a character handler might fire for part of a sequence.

### Configuration Functions

#### `Parser.create () : ParserConfig`
Creates a new parser configuration with default settings.

```fsharp
let config = Parser.create ()
```

#### `Parser.onChar (character: char) (handler: CharHandler) (config: ParserConfig) : ParserConfig`
Registers a character handler for the current mode context.

```fsharp
let config =
    Parser.create ()
    |> Parser.onChar '+' (fun ctx ->
        printfn "Found + at line %d, col %d" ctx.Line ctx.Col
        ctx)
```

#### `Parser.onSequence (sequence: string) (handler: SequenceHandler) (config: ParserConfig) : ParserConfig`
Registers a sequence handler for multi-character sequences like keywords and operators.

```fsharp
let config =
    Parser.create ()
    |> Parser.onSequence "function" (fun ctx ->
        printfn "Found function keyword"
        ParserContextOps.enterMode "functionBody" ctx)
```

#### `Parser.onPattern (pattern: string) (handler: PatternHandler) (config: ParserConfig) : ParserConfig`
Registers a pattern handler using regular expressions for identifiers, numbers, etc.

```fsharp
let config =
    Parser.create ()
    |> Parser.onPattern @"\d+" (fun ctx matched ->
        printfn "Found number: %s" matched
        ctx)
```

#### `Parser.inMode (mode: string) (nestedConfig: ParserConfig -> ParserConfig) (config: ParserConfig) : ParserConfig`
Sets a mode context for nested handler registration, enabling context-sensitive parsing.

```fsharp
let config =
    Parser.create ()
    |> Parser.onSequence "function" (fun ctx -> ParserContextOps.enterMode "functionBody" ctx)
    |> Parser.inMode "functionBody" (fun functionConfig ->
        functionConfig
        |> Parser.onChar '{' (fun ctx -> printfn "Function start"; ctx)
        |> Parser.onChar '}' (fun ctx -> printfn "Function end"; ParserContextOps.exitMode ctx))
```

#### Understanding Modes

Modes allow you to switch parsing behavior dynamically based on context. Think of them as a state machine:

- **Default mode**: Global handlers apply here when no specific mode is active.
- **Enter a mode**: Use `ParserContextOps.enterMode "modeName" ctx` to push a new context onto the mode stack.
- **Exit a mode**: Use `ParserContextOps.exitMode ctx` to return to the previous context.
- **Current mode**: `ParserContextOps.currentMode ctx` returns the active mode (top of stack).

Modes stack arbitrarily - you can nest them, and handlers registered inside a mode (`Parser.inMode`) only fire when that mode is active. This enables context-sensitive parsing, such as different rules inside functions vs. global scope.

#### `Parser.onError (handler: ErrorHandler) (config: ParserConfig) : ParserConfig`
Registers a global error handler that gets called when parsing errors occur.

```fsharp
let config =
    Parser.create ()
    |> Parser.onError (fun ctx msg ->
        printfn "ERROR: %s" msg
        ctx)
```

### Feature Toggles

#### `Parser.enableTokens () (config: ParserConfig) : ParserConfig`
Enables automatic token collection during parsing.

#### `Parser.enableAST () (config: ParserConfig) : ParserConfig`
Enables automatic AST node construction during parsing.

#### `Parser.enableTrace (enabled: bool) (config: ParserConfig) : ParserConfig`
Enables or disables trace logging for debugging.

#### `Parser.onAST (builder: ASTBuilderFunc) (config: ParserConfig) : ParserConfig`
Registers a custom AST builder function for the current mode context.

```fsharp
let config =
    Parser.create ()
    |> Parser.onAST (fun mode matched ctx ->
        match matched with
        | "function" -> Some (ASTNode.Function ("custom", [], []))
        | _ -> None)
```

#### `Parser.validateConfig (config: ParserConfig) : Result<ParserConfig, string>`
Validates that the parser configuration is properly set up, checking for conflicting handlers, invalid regex patterns, and other configuration issues.

```fsharp
let result = Parser.validateConfig config
match result with
| Ok validConfig -> printfn "Configuration is valid"
| Error msg -> printfn "Configuration error: %s" msg
```

### Execution Functions

#### `Parser.run : string -> ParserConfig -> ParserContext`
Parses a file and returns the final parsing context with all results.

```fsharp
let context = Parser.run "program.lang" config
```

#### `Parser.runString : string -> ParserConfig -> ParserContext`
Parses a string input and returns the final parsing context.

```fsharp
let context = Parser.runString "function test() { return 42 }" config
```

### Result Extraction

#### `Parser.getTokens (context: ParserContext) : Token list`
Extracts all collected tokens from the parsing context.

#### `Parser.getAST (context: ParserContext) : ASTNode list`
Extracts all constructed AST nodes from the parsing context.

#### `Parser.getErrors (context: ParserContext) : ErrorInfo list`
Extracts all errors encountered during parsing as ErrorInfo records.

#### `Parser.getTrace (context: ParserContext) : string list`
Extracts all trace messages generated during parsing.

#### `Parser.getUserData (key: string) (context: ParserContext) : obj option`
Retrieves user-defined data from the parsing context.

#### `Parser.setUserData (key: string) (value: obj) (context: ParserContext) : ParserContext`
Stores user-defined data in the parsing context.

```fsharp
let context = Parser.runString "input" config
let updatedContext = Parser.setUserData "customKey" (box "value") context
let retrieved = Parser.getUserData "customKey" updatedContext
```

#### `Parser.formatSummary (context: ParserContext) : string`
Formats a summary of parsing results as a string.

#### `Parser.printSummary (context: ParserContext) : unit`
Prints a summary of parsing results to the console.

## ParserContextOps Module

The `ParserContextOps` module provides functions for managing the parsing context state.

### Context Creation

#### `ParserContextOps.create (filePath: string) (enableTrace: bool) : ParserContext`
Creates an initial parsing context for a file with tracing enabled or disabled.

### Mode Management

#### `ParserContextOps.enterMode (mode: string) (context: ParserContext) : ParserContext`
Pushes a new mode onto the mode stack.

#### `ParserContextOps.exitMode (context: ParserContext) : ParserContext`
Pops the current mode from the mode stack.

#### `ParserContextOps.currentMode (context: ParserContext) : string option`
Gets the current parsing mode (top of stack).

### Position Management

#### `ParserContextOps.updatePosition (line: int) (col: int) (context: ParserContext) : ParserContext`
Updates the current line and column position.

#### `ParserContextOps.updateChar (character: char option) (context: ParserContext) : ParserContext`
Updates the current character being processed.

#### `ParserContextOps.updateBuffer (buffer: string) (context: ParserContext) : ParserContext`
Updates the current line buffer.

### State Management

#### `ParserContextOps.addToken (token: Token) (context: ParserContext) : ParserContext`
Adds a token to the parsing state.

#### `ParserContextOps.addASTNode (node: ASTNode) (context: ParserContext) : ParserContext`
Adds an AST node to the parsing state.

#### `ParserContextOps.addError (errorInfo: ErrorInfo) (context: ParserContext) : ParserContext`
Records an error with full context information.

#### `ParserContextOps.addSimpleError (message: string) (context: ParserContext) : ParserContext`
Records a simple error message at the current position.

#### `ParserContextOps.addTrace (message: string) (context: ParserContext) : ParserContext`
Adds a trace message to the parsing state.

#### `ParserContextOps.setUserData (key: string) (value: obj) (context: ParserContext) : ParserContext`
Stores user-defined data in the context.

#### `ParserContextOps.getUserData (key: string) (context: ParserContext) : obj option`
Retrieves user-defined data from the context.

#### `ParserContextOps.getState (context: ParserContext) : ParserState`
Gets the current parsing state.

#### `ParserContextOps.setState (state: ParserState) (context: ParserContext) : ParserContext`
Updates the entire parsing state.

## ParserConfig Module

The `ParserConfig` holds all parser settings and registered handlers.

### Configuration Functions

#### `ParserConfig.create () : ParserConfig`
Creates a default parser configuration.

#### `ParserConfig.withCharHandler (character: char) (handler: CharHandler) (config: ParserConfig) : ParserConfig`
Adds a character handler for the current mode context.

#### `ParserConfig.withSequenceHandler (sequence: string) (handler: SequenceHandler) (config: ParserConfig) : ParserConfig`
Adds a sequence handler for the current mode context.

#### `ParserConfig.withPatternHandler (pattern: string) (handler: PatternHandler) (config: ParserConfig) : ParserConfig`
Adds a pattern handler for the current mode context.

#### `ParserConfig.withModeContext (mode: string) (nestedConfig: ParserConfig -> ParserConfig) (config: ParserConfig) : ParserConfig`
Temporarily sets mode context for nested handler registration.

#### `ParserConfig.withErrorHandler (handler: ErrorHandler) (config: ParserConfig) : ParserConfig`
Adds a global error handler.

#### `ParserConfig.withTokens (enabled: bool) (config: ParserConfig) : ParserConfig`
Enables or disables tokenization.

#### `ParserConfig.withAST (enabled: bool) (config: ParserConfig) : ParserConfig`
Enables or disables AST building.

#### `ParserConfig.withTrace (enabled: bool) (config: ParserConfig) : ParserConfig`
Enables or disables tracing.

#### `ParserConfig.withASTBuilder (builder: ASTBuilderFunc) (config: ParserConfig) : ParserConfig`
Adds a custom AST builder for the current mode context.

#### `ParserConfig.getRegistry (config: ParserConfig) : HandlerRegistry`
Gets the handler registry from the configuration.

#### `ParserConfig.isTokenizationEnabled (config: ParserConfig) : bool`
Checks if tokenization is enabled in the configuration.

#### `ParserConfig.isASTBuildingEnabled (config: ParserConfig) : bool`
Checks if AST building is enabled in the configuration.

#### `ParserConfig.isTracingEnabled (config: ParserConfig) : bool`
Checks if tracing is enabled in the configuration.

#### `ParserConfig.getCurrentModeContext (config: ParserConfig) : string option`
Gets the current mode context for handler registration.

#### `ParserConfig.getASTBuilders (mode: string option) (config: ParserConfig) : ASTBuilderFunc list`
Gets custom AST builders for a specific mode, falling back to global builders if none found.

## Utility Modules

### ErrorHandling Module

#### `ErrorHandling.triggerError (errorType: ParseError) (config: ParserConfig) (context: ParserContext) (suggestion: string option) : ParserContext`
Triggers error handling by invoking registered error handlers with detailed error information.

#### `ErrorHandling.formatError (line: int) (col: int) (message: string) : string`
Formats an error message with position information.

#### `ErrorHandling.unexpectedChar (character: char) (context: ParserContext) : string`
Generates an "unexpected character" error message.

#### `ErrorHandling.unexpectedSequence (sequence: string) (context: ParserContext) : string`
Generates an "unexpected sequence" error message.

#### `ErrorHandling.modeError (mode: string) (context: ParserContext) : string`
Generates a mode-related error message.

#### `ErrorHandling.parsingError (message: string) (context: ParserContext) : string`
Generates a generic parsing error message.

#### `ErrorHandling.hasErrors (context: ParserContext) : bool`
Checks if there are any errors in the parsing context.

#### `ErrorHandling.getErrors (context: ParserContext) : ErrorInfo list`
Gets all errors from the parsing context.

#### `ErrorHandling.getErrorCount (context: ParserContext) : int`
Gets the count of errors in the parsing context.

#### `ErrorHandling.formatErrors (context: ParserContext) : string`
Formats all errors as a string.

#### `ErrorHandling.printErrors (context: ParserContext) : unit`
Prints all errors to the console.

#### `ErrorHandling.clearErrors (context: ParserContext) : ParserContext`
Clears all errors from the parsing context.

### Tracer Module

#### `Tracer.trace (message: string) (context: ParserContext) : ParserContext`
Adds a trace message if tracing is enabled.

#### `Tracer.traceChar (character: char) (context: ParserContext) : ParserContext`
Traces character processing.

#### `Tracer.traceSequence (sequence: string) (context: ParserContext) : ParserContext`
Traces sequence matches.

#### `Tracer.tracePattern (pattern: string) (matchedText: string) (context: ParserContext) : ParserContext`
Traces pattern matches.

#### `Tracer.formatTrace (context: ParserContext) : string`
Formats all trace messages as a string.

#### `Tracer.printTrace (context: ParserContext) : unit`
Prints all trace messages to the console.

#### `Tracer.isTracingEnabled (context: ParserContext) : bool`
Checks if tracing is enabled in the context.

#### `Tracer.traceModeEnter (mode: string) (context: ParserContext) : ParserContext`
Traces mode entry.

#### `Tracer.traceModeExit (mode: string) (context: ParserContext) : ParserContext`
Traces mode exit.

#### `Tracer.traceToken (token: Token) (context: ParserContext) : ParserContext`
Traces token emission.

#### `Tracer.traceAST (node: ASTNode) (context: ParserContext) : ParserContext`
Traces AST node creation.

#### `Tracer.getTrace (context: ParserContext) : string list`
Gets all trace messages from the context.

#### `Tracer.getTraceCount (context: ParserContext) : int`
Gets the count of trace messages.

#### `Tracer.clearTrace (context: ParserContext) : ParserContext`
Clears all trace messages from the context.

### Tokenizer Module

#### `Tokenizer.emitToken (tokenType: TokenType) (context: ParserContext) : ParserContext`
Creates and emits a token with current position and mode.

#### `Tokenizer.inferTokenType (matchedText: string) : TokenType`
Infers token type from matched text.

#### `Tokenizer.autoEmit (keywords: Set<string>) (matchedText: string) (context: ParserContext) : ParserContext`
Automatically emits a token for matched text if tokenization is enabled.

### ASTBuilder Module

#### `ASTBuilder.buildNode (mode: string) (matchedText: string) (context: ParserContext) : ASTNode option`
Builds an AST node based on mode and matched text.

#### `ASTBuilder.pushNodeStack (node: ASTNode) (context: ParserContext) : ParserContext`
Pushes a node onto the AST stack for nested structures.

#### `ASTBuilder.popNodeStack (context: ParserContext) : (ASTNode * ParserContext) option`
Pops a node from the AST stack.

#### `ASTBuilder.autoAddNode (config: ParserConfig) (matchedText: string) (context: ParserContext) : ParserContext`
Automatically creates and adds an AST node if AST building is enabled.

#### `ASTBuilder.getNodeStack (context: ParserContext) : ASTNode list`
Gets the current AST node stack.

#### `ASTBuilder.clearNodeStack (context: ParserContext) : ParserContext`
Clears the AST node stack.

## Usage Examples

### Basic Character Handler

```fsharp
let config =
    Parser.create ()
    |> Parser.onChar '+' (fun ctx ->
        printfn "Addition operator found!"
        ctx)
```

### Sequence Handler with Mode

```fsharp
let config =
    Parser.create ()
    |> Parser.onSequence "function" (fun ctx ->
        printfn "Function definition found"
        ParserContextOps.enterMode "functionBody" ctx)
    |> Parser.inMode "functionBody" (fun functionConfig ->
        functionConfig
        |> Parser.onChar '{' (fun ctx -> printfn "Function body start"; ctx)
        |> Parser.onChar '}' (fun ctx -> printfn "Function body end"; ParserContextOps.exitMode ctx))
```

### Pattern Handler for Identifiers

```fsharp
let config =
    Parser.create ()
    |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx identifier ->
        printfn "Identifier found: %s" identifier
        ctx)
```

### End-to-End AST Construction Example

Here's how input text gets transformed into an AST:

```fsharp
let parser =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.enableAST ()
    |> Parser.onSequence "return" (fun ctx -> ctx)
    |> Parser.onPattern @"\d+" (fun ctx num -> ctx)

let context = Parser.runString "return 42" parser
let ast = Parser.getAST context

// Example AST output:
[ Return (Some (Number 42.0)) ]
```

This shows how the parser processes "return 42" and builds a `Return` node containing a `Number` literal.

### Complete Parser with All Features

```fsharp
let parser =
    Parser.create ()
    |> Parser.enableTokens ()
    |> Parser.enableAST ()
    |> Parser.enableTrace true
    |> Parser.onError (fun ctx msg -> printfn "Error: %s" msg; ctx)
    |> Parser.onSequence "function" (fun ctx -> ParserContextOps.enterMode "functionBody" ctx)
    |> Parser.inMode "functionBody" (fun config ->
        config
        |> Parser.onSequence "if" (fun ctx -> ParserContextOps.enterMode "ifBody" ctx)
        |> Parser.inMode "ifBody" (fun ifConfig ->
            ifConfig
            |> Parser.onChar '{' (fun ctx -> printfn "If block"; ctx)
            |> Parser.onChar '}' (fun ctx -> ParserContextOps.exitMode ctx)))

let context = Parser.runString "function test() { if true { return 42 } }" parser
let tokens = Parser.getTokens context
let ast = Parser.getAST context
let errors = Parser.getErrors context

## Performance and Limitations

### Performance Characteristics

SharpParser.Core is designed for educational and light to medium parsing tasks:

- **Sequence matching**: O(m) using trie-based lookup for efficient keyword/operator recognition
- **Pattern matching**: Regex-based with compiled caching for identifiers, numbers, etc.
- **Memory usage**: Functional immutable data structures ensure thread-safety but may have higher memory overhead for very large files
- **Scalability**: Best suited for files up to ~100KB; not optimized for full compiler-grade performance

### Limitations

- Not intended for parsing massive files or high-performance compiler scenarios
- Regex patterns should be validated to avoid catastrophic backtracking
- Error recovery is basic - the parser stops on first error by default
- No built-in support for left-recursive grammars or advanced parsing techniques

For production compiler work, consider more specialized parsing libraries or tools.

## Common Use Cases

SharpParser.Core excels at:

- **Toy languages**: Building parsers for simple programming languages or DSLs
- **Data formats**: Parsing custom configuration files or structured text
- **Educational projects**: Learning parsing concepts without complex tooling
- **Prototyping**: Quick parser development for research or experimentation
- **Domain-specific parsing**: Custom formats for business logic or data processing

### Examples by Domain

- **Calculator parser**: Binary operations with precedence
- **Mini-JavaScript subset**: Functions, variables, control flow
- **Configuration DSL**: Custom syntax for application settings
- **Log file parser**: Structured log analysis with patterns
- **Template engine**: Variable substitution and control structures