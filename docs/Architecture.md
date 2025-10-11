# SharpParser.Core Architecture Documentation

## Overview

SharpParser.Core is designed as a modular, functional F# parsing library that emphasizes simplicity for beginners while providing powerful features for advanced users. The architecture follows functional programming principles with immutable data structures and clear separation of concerns.

## Core Architecture Principles

### 1. Immutable State Management
All parsing state is immutable. Updates return new instances rather than modifying existing ones:

```fsharp
// ParserContext operations return new contexts
let newContext = ParserContext.addToken token context
let modeContext = ParserContext.enterMode "functionBody" context
```

### 2. Functional Configuration
Parser configuration uses a fluent, functional style:

```fsharp
let config =
    Parser.create ()
    |> Parser.onChar '+' handler1
    |> Parser.onSequence "function" handler2
    |> Parser.inMode "functionBody" (fun config ->
        config |> Parser.onChar '{' handler3)
```

### 3. Mode-Based Context Sensitivity
Parsing modes enable context-sensitive parsing without complex state machines:

```fsharp
Parser.create ()
|> Parser.onSequence "function" (fun ctx -> ParserContext.enterMode "functionBody" ctx)
|> Parser.inMode "functionBody" (fun config ->
    config |> Parser.onChar '{' (fun ctx -> printfn "Function start"; ctx))
```

## Module Architecture

### 1. Types Module (`Types.fs`)
**Responsibility**: Core type definitions
**Dependencies**: None

Defines the fundamental types used throughout the library:
- `TokenType` and `Token` for lexical analysis
- `ASTNode` for syntax tree representation
- Handler function types (`CharHandler`, `SequenceHandler`, `PatternHandler`, `ErrorHandler`)
- `ParserState` for mutable parsing state

### 2. Trie Module (`Trie.fs`)
**Responsibility**: Efficient sequence matching
**Dependencies**: Types

Implements a prefix tree (trie) data structure for fast multi-character sequence detection:
- `TrieNode<'T>` with children map and terminal values
- `insert` for adding sequences with associated values
- `search` for exact sequence lookup
- `longestMatch` for finding longest matching prefix

### 3. PatternMatcher Module (`PatternMatcher.fs`)
**Responsibility**: Regex-based pattern matching
**Dependencies**: Types

Provides regex pattern matching with performance optimizations:
- `compile` for regex compilation with appropriate options
- `tryMatch` for position-based matching
- `matchAll` for trying multiple patterns
- Caching system to avoid regex recompilation

### 4. HandlerRegistry Module (`HandlerRegistry.fs`)
**Responsibility**: Handler storage and lookup
**Dependencies**: Types, Trie, PatternMatcher

Central registry for all handler types organized by parsing mode:
- `CharHandlers` map: mode → char → handler list
- `SequenceTrie` map: mode → trie of sequences
- `PatternHandlers` map: mode → compiled regex and handler pairs
- `ErrorHandlers` list: global error handlers

### 5. ParserContext Module (`ParserContext.fs`)
**Responsibility**: Parsing state management
**Dependencies**: Types

Manages the current parsing state and position:
- `ParserContext` record with line, column, mode stack, and state
- Mode stack operations (`enterMode`, `exitMode`, `currentMode`)
- Position tracking (`updatePosition`, `updateChar`, `updateBuffer`)
- State management (`addToken`, `addASTNode`, `addError`, `addTrace`)

### 6. ParserConfig Module (`ParserConfig.fs`)
**Responsibility**: Parser configuration
**Dependencies**: Types, HandlerRegistry

Holds parser settings and registered handlers:
- `ParserConfig` record with registry and feature flags
- Fluent API functions for handler registration
- Mode context management for nested configurations
- Feature toggles (tokens, AST, tracing)

### 7. ParsingEngine Module (`ParsingEngine.fs`)
**Responsibility**: Core parsing logic
**Dependencies**: Types, ParserContext, ParserConfig, HandlerRegistry

Main parsing engine that processes input and dispatches to handlers:
- `processChar`: Handles single character processing with priority order
- `processLine`: Processes entire line with character iteration
- `processInput`: Processes sequence of lines
- `parseFile` and `parseString`: High-level parsing entry points

### 8. Tokenizer Module (`Tokenizer.fs`)
**Responsibility**: Token generation
**Dependencies**: Types, ParserContext

Automatic token emission based on handler matches:
- `emitToken`: Creates tokens with position and mode information
- `inferTokenType`: Classifies tokens (keyword, identifier, symbol, number)
- Keyword registry for distinguishing keywords from identifiers

### 9. ASTBuilder Module (`ASTBuilder.fs`)
**Responsibility**: AST construction with expression parsing
**Dependencies**: Types, ParserContext

Advanced AST node building with proper expression parsing and operator precedence:
- `ExpressionStack`: Stack-based system for handling operator precedence
- `buildNode`: Creates AST nodes based on mode and matched text with expression context awareness
- `pushOperand`/`pushOperator`: Expression stack management for complex mathematical expressions
- `finalizeExpression`: Completes expression parsing and builds proper AST trees
- `buildAssignment`/`buildFunctionCall`: Statement-level AST construction
- Operator precedence handling (*, / > +, -) following mathematical conventions
- Node stack management for nested structures and context-aware construction

### 10. ErrorHandler Module (`ErrorHandler.fs`)
**Responsibility**: Error handling
**Dependencies**: Types, ParserContext, ParserConfig

Comprehensive error handling and reporting:
- `triggerError`: Invokes registered error handlers
- Error formatting with position information
- Error collection and reporting utilities

### 11. Tracer Module (`Tracer.fs`)
**Responsibility**: Debugging and tracing
**Dependencies**: Types, ParserContext

Debugging support with trace logging:
- `trace`: Adds trace messages to parsing state
- Specialized tracing functions for different event types
- Trace log management and printing

### 12. Parser Module (`Parser.fs`)
**Responsibility**: Public API
**Dependencies**: All other modules

Provides the clean, fluent public API:
- Configuration functions (`create`, `onChar`, `onSequence`, `onPattern`, `inMode`)
- Feature toggles (`enableTokens`, `enableAST`, `enableTrace`)
- Execution functions (`run`, `runString`)
- Result extraction (`getTokens`, `getAST`, `getErrors`, `getTrace`)

## Data Flow Architecture

### Parsing Flow

1. **Input Reception**: `Parser.run` or `Parser.runString` receives input
2. **Context Creation**: `ParserContext.create` initializes parsing state
3. **Line Processing**: `ParsingEngine.processInput` iterates through lines
4. **Character Processing**: `ParsingEngine.processChar` handles each character
5. **Handler Dispatch**: Priority-based handler lookup and invocation:
   - Sequence matching (longest first) via Trie
   - Pattern matching via compiled regexes
   - Character handlers for individual chars
6. **State Updates**: Handlers modify context via ParserContext functions
7. **Feature Integration**: Optional tokenization, AST building, and tracing
8. **Result Collection**: Final context contains all tokens, AST nodes, errors, and traces

### Handler Priority System

The parsing engine uses a three-tier priority system:

1. **Sequence Handlers** (Highest Priority)
   - Multi-character operators and keywords
   - Longest match wins
   - Implemented via Trie for efficiency

2. **Pattern Handlers** (Medium Priority)
   - Regular expression patterns
   - First match wins
   - Compiled regexes with caching

3. **Character Handlers** (Lowest Priority)
   - Individual character responses
   - All matching handlers invoked
   - Simple but powerful for punctuation

### Mode Stack Architecture

Modes enable context-sensitive parsing:

```
Global Mode
  └── functionBody Mode (entered by "function" sequence)
      └── ifBody Mode (entered by "if" sequence)
```

Each mode maintains separate handler sets, allowing different behavior in different contexts.

## Performance Considerations

### Optimizations Implemented

1. **Trie-based Sequence Matching**
    - O(1) lookup time using Dictionary instead of Map
    - Avoids repeated string operations
    - Memory efficient for large keyword sets

2. **Regex Compilation and Caching**
    - Compiled regexes for pattern handlers
    - ConcurrentDictionary cache to avoid recompilation
    - Anchored patterns for position-based matching
    - Pre-compiled patterns for common token types

3. **Parallel Processing**
    - Parallel tokenization for independent line processing
    - Function boundary detection for potential parallel function parsing
    - Configurable parallelism levels with automatic fallback

4. **Immutable Data Structures**
    - Thread-safe parsing contexts
    - No locking required for concurrent access
    - Predictable memory usage patterns

5. **Functional Design**
    - No mutable state in core algorithms
    - Easy to test and reason about
    - Composability through function chaining

### Algorithm Complexities

- **Sequence Matching**: O(m) where m is length of matched sequence
- **Pattern Matching**: O(n) where n is number of patterns (first match wins)
- **Character Processing**: O(1) for handler lookup
- **Mode Management**: O(1) for stack operations

## Extension Points

The architecture supports several extension mechanisms:

1. **Custom Token Types**: Extend `TokenType` union for domain-specific tokens
2. **Custom AST Nodes**: Extend `ASTNode` union for language-specific constructs
3. **Handler Plugins**: Load handlers from external assemblies
4. **Grammar DSL**: Define grammars in a domain-specific language
5. **Parallel Processing**: Process multiple files concurrently (implemented)
6. **Incremental Parsing**: Parse changes without full reparse

## Testing Strategy

The modular architecture enables focused testing:

- **Unit Tests**: Test individual modules in isolation
- **Integration Tests**: Test module interactions
- **End-to-End Tests**: Test complete parsing scenarios
- **Performance Tests**: Benchmark critical paths

## Future Architecture Enhancements

1. **Plugin System**: Dynamic handler loading
2. **Grammar DSL**: Declarative grammar definition
3. **Incremental Parsing**: Efficient change-based parsing
4. **Visual Debugging**: GUI for parsing visualization
5. **Language Server**: IDE integration for syntax highlighting and error reporting