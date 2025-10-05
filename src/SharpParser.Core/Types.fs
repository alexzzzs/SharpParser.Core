namespace SharpParser.Core

/// Represents the type of a token in the parsed input
type TokenType =
    /// A keyword in the language (e.g., "function", "if", "else")
    | Keyword of string
    /// An identifier (variable name, function name)
    | Identifier of string
    /// A symbol or operator (e.g., "+", "=", "{", "}")
    | Symbol of string
    /// A numeric literal
    | Number of float
    /// A string literal
    | StringLiteral of string
    /// End of file marker
    | EOF

/// Represents a token with position information
type Token = {
    /// The type of this token
    Type: TokenType
    /// Line number (1-based) where this token appears
    Line: int
    /// Column number (1-based) where this token appears
    Col: int
    /// The parsing mode when this token was created (if applicable)
    Mode: string option
}

/// Represents a node in the Abstract Syntax Tree (AST)
type ASTNode =
    /// A function definition with name, parameters, and body
    | Function of string * string list * ASTNode list
    /// An if statement with condition, then body, and optional else body
    | If of ASTNode * ASTNode list * ASTNode list option
    /// A while loop with condition and body
    | While of ASTNode * ASTNode list
    /// A for loop with initialization, condition, increment, and body
    | For of ASTNode option * ASTNode option * ASTNode option * ASTNode list
    /// A block of statements
    | Block of ASTNode list
    /// An assignment operation
    | Assignment of string * ASTNode
    /// A return statement
    | Return of ASTNode option
    /// A function call with name and arguments
    | Call of string * ASTNode list
    /// An expression (for complex expressions)
    | Expression of string
    /// A literal value
    | Literal of string
    /// A binary operation (left op right)
    | BinaryOp of ASTNode * string * ASTNode
    /// A unary operation (op operand)
    | UnaryOp of string * ASTNode
    /// A variable reference
    | Variable of string
    /// A number literal
    | Number of float
    /// A string literal
    | StringLiteral of string
    /// A boolean literal
    | Boolean of bool

/// Represents different types of parsing errors
type ParseError =
    /// Unexpected character encountered
    | UnexpectedChar of char
    /// Unexpected sequence encountered
    | UnexpectedSequence of string
    /// Invalid token or syntax
    | InvalidSyntax of string
    /// Mode-related error
    | ModeError of string
    /// Configuration error
    | ConfigError of string
    /// Generic parsing error
    | GenericError of string

/// Represents an error with additional context
type ErrorInfo = {
    /// The type of error
    ErrorType: ParseError
    /// Line number where error occurred
    Line: int
    /// Column number where error occurred
    Col: int
    /// Current parsing mode when error occurred
    Mode: string option
    /// Error message
    Message: string
    /// Optional suggestion for fixing the error
    Suggestion: string option
}

/// Represents the mutable state during parsing
type ParserState = {
    /// List of tokens collected during parsing (if tokenization enabled)
    Tokens: Token list
    /// List of AST nodes built during parsing (if AST building enabled)
    ASTNodes: ASTNode list
    /// List of errors encountered during parsing
    Errors: ErrorInfo list
    /// List of trace messages for debugging (if tracing enabled)
    TraceLog: string list
    /// User-defined data storage for custom parsing state
    UserData: Map<string, obj>
}

/// Represents the current parsing context and state
type ParserContext = {
    /// Current line number (1-based)
    Line: int
    /// Current column number (1-based)
    Col: int
    /// Source file path or identifier
    FilePath: string
    /// Current character being processed (if any)
    CurrentChar: char option
    /// Current line buffer
    Buffer: string
    /// Stack of active parsing modes (head = current mode)
    ModeStack: string list
    /// Mutable parsing state
    State: ParserState
    /// Whether tracing is enabled
    EnableTrace: bool
}

/// Handler function type for character-based parsing
type CharHandler = ParserContext -> ParserContext

/// Handler function type for sequence-based parsing (e.g., keywords, operators)
type SequenceHandler = ParserContext -> ParserContext

/// Handler function type for pattern-based parsing (e.g., identifiers, numbers)
type PatternHandler = ParserContext -> string -> ParserContext

/// Handler function type for error reporting
type ErrorHandler = ParserContext -> string -> ParserContext

/// Handler function type for custom AST node construction
type ASTBuilderFunc = string option -> string -> ParserContext -> ASTNode option
