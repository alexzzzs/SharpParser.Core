# SharpParser.Core API Documentation

## Overview

SharpParser.Core is a beginner-friendly, event-driven F# parsing library that provides a fluent API for building parsers with support for character handlers, sequence handlers, pattern handlers, and context-sensitive parsing modes.

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
    | Function of string * ASTNode list  // Function definition
    | If of ASTNode * ASTNode list       // If statement
    | Assignment of string * ASTNode     // Variable assignment
    | Expression of string               // Expression
    | Literal of string                  // Literal value
    | BinaryOp of ASTNode * string * ASTNode  // Binary operation (left op right)
    | UnaryOp of string * ASTNode        // Unary operation (op operand)
    | Variable of string                 // Variable reference
    | Number of float                    // Number literal
    | StringLiteral of string            // String literal
```

### Handler Types
Type aliases for different handler function signatures:

```fsharp
type CharHandler = ParserContext -> unit                    // Character handlers
type SequenceHandler = ParserContext -> unit                // Sequence handlers
type PatternHandler = ParserContext -> string -> unit       // Pattern handlers (receives matched text)
type ErrorHandler = ParserContext -> string -> unit         // Error handlers (receives error message)
```

## Parser Module (Main API)

The `Parser` module provides the main fluent API for configuring and running parsers.

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

#### `Parser.validateConfig (config: ParserConfig) : Result<ParserConfig, string>`
Validates that the parser configuration is properly set up, checking for conflicting handlers, invalid regex patterns, and other configuration issues.

```fsharp
let result = Parser.validateConfig config
match result with
| Ok validConfig -> printfn "Configuration is valid"
| Error msg -> printfn "Configuration error: %s" msg
```

### Execution Functions

#### `Parser.run (filePath: string) (config: ParserConfig) : ParserContext`
Parses a file and returns the final parsing context with all results.

```fsharp
let context = Parser.run "program.lang" config
```

#### `Parser.runString (input: string) (config: ParserConfig) : ParserContext`
Parses a string input and returns the final parsing context.

```fsharp
let context = Parser.runString "function test() { return 42 }" config
```

### Result Extraction

#### `Parser.getTokens (context: ParserContext) : Token list`
Extracts all collected tokens from the parsing context.

#### `Parser.getAST (context: ParserContext) : ASTNode list`
Extracts all constructed AST nodes from the parsing context.

#### `Parser.getErrors (context: ParserContext) : (int * int * string) list`
Extracts all errors encountered during parsing (line, column, message).

#### `Parser.getTrace (context: ParserContext) : string list`
Extracts all trace messages generated during parsing.

#### `Parser.formatSummary (context: ParserContext) : string`
Formats a summary of parsing results as a string.

#### `Parser.printSummary (context: ParserContext) : unit`
Prints a summary of parsing results to the console.

## ParserContextOps Module

The `ParserContextOps` module provides functions for managing the parsing context state.

### Context Creation

#### `ParserContextOps.create (filePath: string) : ParserContext`
Creates an initial parsing context for a file.

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

#### `ParserContextOps.addError (message: string) (context: ParserContext) : ParserContext`
Records an error at the current position.

#### `ParserContextOps.addTrace (message: string) (context: ParserContext) : ParserContext`
Adds a trace message to the parsing state.

#### `ParserContextOps.setUserData (key: string) (value: obj) (context: ParserContext) : ParserContext`
Stores user-defined data in the context.

#### `ParserContextOps.getUserData (key: string) (context: ParserContext) : obj option`
Retrieves user-defined data from the context.

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

## Utility Modules

### ErrorHandling Module

#### `ErrorHandling.triggerError (message: string) (config: ParserConfig) (context: ParserContext) : ParserContext`
Triggers error handling by invoking registered error handlers.

#### `ErrorHandling.formatError (line: int) (col: int) (message: string) : string`
Formats an error message with position information.

#### `ErrorHandling.unexpectedChar (character: char) (context: ParserContext) : string`
Generates an "unexpected character" error message.

#### `ErrorHandling.hasErrors (context: ParserContext) : bool`
Checks if there are any errors in the parsing context.

#### `ErrorHandling.getErrors (context: ParserContext) : (int * int * string) list`
Gets all errors from the parsing context.

#### `ErrorHandling.formatErrors (context: ParserContext) : string`
Formats all errors as a string.

#### `ErrorHandling.printErrors (context: ParserContext) : unit`
Prints all errors to the console.

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

### Tokenizer Module

#### `Tokenizer.emitToken (tokenType: TokenType) (context: ParserContext) : ParserContext`
Creates and emits a token with current position and mode.

#### `Tokenizer.inferTokenType (matchedText: string) : TokenType`
Infers token type from matched text.

#### `Tokenizer.registerKeyword (keyword: string) : unit`
Registers a keyword for token classification.

### ASTBuilder Module

#### `ASTBuilder.buildNode (mode: string) (matchedText: string) (context: ParserContext) : ASTNode option`
Builds an AST node based on mode and matched text.

#### `ASTBuilder.pushNodeStack (node: ASTNode) (context: ParserContext) : ParserContext`
Pushes a node onto the AST stack for nested structures.

#### `ASTBuilder.popNodeStack (context: ParserContext) : (ASTNode * ParserContext) option`
Pops a node from the AST stack.

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