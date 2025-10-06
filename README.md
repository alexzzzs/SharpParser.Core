# SharpParser.Core

[![NuGet](https://img.shields.io/nuget/v/SharpParser.Core.svg)](https://www.nuget.org/packages/SharpParser.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-6.0+-blue.svg)](https://dotnet.microsoft.com/)
[![F#](https://img.shields.io/badge/F%23-00539C?logo=fsharp&logoColor=white)](https://fsharp.org/)

A beginner-friendly, event-driven F# parsing library for language design and implementation.

## Why SharpParser.Core?

Writing parsers in F# often requires complex combinator libraries or heavy compiler frameworks. SharpParser.Core takes a different approach: a lightweight, event-driven API that makes building language parsers **intuitive, composable, and fun** — without sacrificing power. Whether you're building a domain-specific language, configuration parser, or language tool, SharpParser gives you the control and performance you need with an API that feels natural in F#.

## Features

- **Character, sequence, and pattern handlers** - Handle individual characters, multi-character sequences, and regex patterns
- **Context-sensitive parsing modes** - Use mode stacks for nested parsing contexts
- **Optional tokenization and AST construction** - Enable automatic token and AST generation
- **Enhanced AST types** - Support for binary operations, unary operations, variables, numbers, and strings
- **Configuration validation** - Built-in validation to catch configuration errors early
- **Error handling and debugging** - Comprehensive error reporting and trace logging
- **Parallel parsing** - Multi-threaded parsing for improved performance on multi-core systems
- **Functional programming** - Pure functions, immutable data structures, and no mutable state
- **Fluent, chainable API** - Easy-to-use functional programming style
- **Comprehensive testing** - 138 tests covering all functionality and edge cases

## Quick Start

### Hello World Parser

Here's your first parser in just 10 lines:

```fsharp
open SharpParser.Core

let parser =
    Parser.create()
    |> Parser.onSequence "hello" (fun ctx ->
        printfn "Hello found at line %d, col %d" ctx.Line ctx.Col
        ctx)

Parser.runString "hello world" parser
```

**Output:**
```
Hello found at line 1, col 1
```

✅ **That's all you need to start building a parser!** The next example shows how to handle nested contexts, ASTs, and more advanced features.

### Full Example with Modes & AST

```fsharp
open SharpParser.Core

// Create a parser with mode-based handlers
let parser =
    Parser.create ()
    |> Parser.enableTokens()  // Enable automatic tokenization
    |> Parser.enableAST()     // Enable AST building
    |> Parser.onSequence "function" (fun ctx ->
        printfn "Found function at line %d, col %d" ctx.Line ctx.Col
        ParserContext.enterMode "functionBody" ctx)
    |> Parser.inMode "functionBody" (fun config ->
        config
        |> Parser.onChar '{' (fun ctx ->
            printfn "Start function block"
            ctx)
        |> Parser.onChar '}' (fun ctx ->
            printfn "End function block"
            ParserContext.exitMode ctx))

// Parse input
let context = Parser.runString "function test() { return 42 }" parser

// Get results
let tokens = Parser.getTokens context
let ast = Parser.getAST context
```

**Sample Output:**
```
Found function at line 1, col 1
Start function block
End function block

Tokens: [Keyword("function"), Identifier("test"), Symbol("("), Symbol(")"), Symbol("{"), Keyword("return"), Number(42.0), Symbol("}")]

AST: [Function("test", [], [Return(Some(Number(42.0)))])]
```

## Installation

### Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- F# development environment (Visual Studio, VS Code with Ionide, or Rider)

### NuGet Package (Recommended)

Install SharpParser.Core from NuGet:

```bash
dotnet add package SharpParser.Core
```

Or add it manually to your `.fsproj`:

```xml
<PackageReference Include="SharpParser.Core" Version="1.0.0" />
```

### From Source

Clone the repository and reference the project directly:

```bash
git clone https://github.com/alexzzzs/SharpParser.Core.git
```

Then add to your `.fsproj`:

```xml
<ProjectReference Include="path/to/SharpParser.Core/src/SharpParser.Core/SharpParser.Core.fsproj" />
```

### Verify Installation

Create a simple test file to verify the installation:

```fsharp
open SharpParser.Core

let parser = Parser.create() |> Parser.onChar 'a' (fun ctx -> printfn "Found 'a'!"; ctx)
let result = Parser.runString "test" parser
printfn "Installation successful!"
```

## Core Concepts

### Handlers

SharpParser.Core provides three types of handlers:

- **Character handlers** (`onChar`) - Handle individual characters
- **Sequence handlers** (`onSequence`) - Handle multi-character sequences like keywords and operators
- **Pattern handlers** (`onPattern`) - Handle regex patterns like identifiers and numbers

### Parsing Modes

Use modes to create context-sensitive parsers:

```fsharp
Parser.create ()
|> Parser.onSequence "function" (fun ctx -> ParserContext.enterMode "functionBody" ctx)
|> Parser.inMode "functionBody" (fun config ->
    config
    |> Parser.onChar '{' (fun ctx -> printfn "Function start"; ctx)
    |> Parser.onChar '}' (fun ctx -> printfn "Function end"; ParserContext.exitMode ctx))
```

### Context

The `ParserContext` contains all parsing state:

- Current position (line, column)
- Mode stack
- Collected tokens and AST nodes
- Error and trace information

### Optional Features

Enable additional features as needed:

```fsharp
Parser.create ()
|> Parser.enableTokens ()  // Enable tokenization
|> Parser.enableAST ()     // Enable AST building
|> Parser.enableTrace true // Enable tracing
```

## API Reference

### Core Functions

- `Parser.create()` - Create new parser configuration
- `Parser.onChar char handler` - Register character handler
- `Parser.onSequence sequence handler` - Register sequence handler
- `Parser.onPattern pattern handler` - Register pattern handler
- `Parser.inMode mode nestedConfig` - Set mode context for nested handlers
- `Parser.onError handler` - Register global error handler
- `Parser.enableTokens ()` - Enable automatic tokenization
- `Parser.enableAST ()` - Enable automatic AST building
- `Parser.enableTrace enabled` - Enable or disable tracing
- `Parser.run filePath` - Parse file and return context
- `Parser.runString input` - Parse string and return context

### Context Accessors

- `Parser.getTokens context` - Extract collected tokens
- `Parser.getAST context` - Extract AST nodes
- `Parser.getErrors context` - Extract errors
- `Parser.getTrace context` - Extract trace log
- `Parser.getUserData key context` - Get user-defined data
- `Parser.setUserData key value context` - Set user-defined data

### Context Manipulation

- `ParserContextOps.enterMode mode context` - Push mode onto stack
- `ParserContextOps.exitMode context` - Pop mode from stack
- `ParserContextOps.currentMode context` - Get current mode
- `ParserContextOps.addToken token context` - Add token to state
- `ParserContextOps.addASTNode node context` - Add AST node to state
- `ParserContextOps.addError message context` - Record error
- `ParserContextOps.addTrace message context` - Add trace message

### Utility Functions

- `Parser.printSummary context` - Print parsing results summary
- `Parser.formatSummary context` - Format parsing results as string
- `Parser.validateConfig config` - Validate parser configuration
- `ErrorHandling.formatErrors context` - Format errors as string
- `Tracer.formatTrace context` - Format trace log as string

## Examples

See the `examples/` directory for comprehensive examples:

- **BasicExample** - Simple character and pattern handlers
- **ModeExample** - Context-sensitive parsing with modes
- **FullExample** - All features (tokens, AST, error handling, tracing)

Run the examples:

```bash
dotnet run --project examples/SharpParser.Examples/SharpParser.Examples.fsproj
```

## Architecture

SharpParser.Core consists of several focused modules:

- **Types** - Core type definitions (Token, ASTNode, handlers)
- **Trie** - Efficient sequence matching with prefix tree
- **PatternMatcher** - Regex-based pattern matching
- **HandlerRegistry** - Storage and lookup for all handler types
- **ParserContext** - Parsing state and mode management
- **ParserConfig** - Parser configuration with fluent API
- **ParsingEngine** - Core parsing logic and handler dispatch
- **Tokenizer** - Automatic token generation
- **ASTBuilder** - Automatic AST construction
- **ErrorHandler** - Error handling and reporting
- **Tracer** - Debugging and trace logging
- **Parser** - Public fluent API

## Performance

- **Trie-based sequence matching** for efficient multi-character detection
- **Compiled regex patterns** for fast pattern matching
- **Parallel parsing** for improved performance on multi-core systems
- **Immutable data structures** for thread safety and functional style
- **Functional programming patterns** with no mutable state for better performance
- **Minimal allocations** through pure functions and immutability
- **Comprehensive test coverage** ensuring reliability and performance

## Future Extensions

The architecture supports several extension points:

- **Grammar DSL** - Domain-specific language for grammar definition
- **Incremental parsing** - Parse changes without full reparse
- **Custom token types** - Extensible token type system
- **Plugin system** - Load handlers from external assemblies
- **Visual debugging** - GUI for parsing visualization

## Contributing

We welcome contributions! SharpParser.Core is an open-source project that benefits from community involvement.

### Ways to Contribute

- **🐛 Bug Reports**: Found an issue? [Open a GitHub issue](https://github.com/alexzzzs/SharpParser.Core/issues)
- **💡 Feature Requests**: Have an idea? [Start a discussion](https://github.com/alexzzzs/SharpParser.Core/discussions)
- **📖 Documentation**: Help improve docs, examples, or tutorials
- **🧪 Testing**: Add test cases or improve test coverage
- **🔧 Code**: Submit pull requests for bug fixes or new features

### Development Setup

```bash
git clone https://github.com/alexzzzs/SharpParser.Core.git
cd SharpParser.Core
dotnet build
dotnet test
```

### Recent Improvements

✅ **Functional programming** - Eliminated mutable state throughout the codebase
✅ **Enhanced AST types** - Added support for complex expressions and literals
✅ **Configuration validation** - Built-in validation for parser configurations
✅ **Parallel parsing** - Multi-threaded parsing for improved performance on multi-core systems
✅ **Performance optimizations** - Regex anchoring, trie Dictionary optimization, token caching
✅ **Comprehensive testing** - 138 tests covering all functionality and edge cases
✅ **Error handling** - Proper invocation of error handlers
✅ **Documentation** - Updated API docs and README

### Future Roadmap

1. **Performance optimizations** - Continue benchmarking and optimizing hot paths
2. **Additional examples** - More real-world parsing scenarios
3. **Extensions** - Implement planned extension points (grammar DSL, incremental parsing)
4. **Visual debugging** - GUI for parsing visualization

## License

MIT License - see LICENSE file for details.

---

SharpParser.Core makes parsing fun and accessible while providing the power needed for complex language implementations!