# SharpParser.Core

[![NuGet](https://img.shields.io/nuget/v/SharpParser.Core.svg)](https://www.nuget.org/packages/SharpParser.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A beginner-friendly, event-driven F# parsing library for language design and implementation.

## Installation

```bash
dotnet add package SharpParser.Core
```

## Quick Start

```fsharp
open SharpParser.Core

// Create your first parser
let parser =
    Parser.create()
    |> Parser.onSequence "hello" (fun ctx ->
        printfn "Hello found!"
        ctx)

Parser.runString "hello world" parser
```

## Features

- **Event-driven parsing** with intuitive handler functions
- **Context-sensitive modes** for nested parsing contexts
- **Automatic tokenization** and AST construction
- **Performance optimized** with trie-based matching
- **Comprehensive error handling** and debugging support

## Documentation

📖 [Full Documentation](https://github.com/alexzzzs/SharpParser.Core)

🔧 [API Reference](https://github.com/alexzzzs/SharpParser.Core/blob/master/docs/API.md)

🚀 [Quick Start Guide](https://github.com/alexzzzs/SharpParser.Core/blob/master/docs/QuickStart.md)

## Example

```fsharp
let parser =
    Parser.create()
    |> Parser.enableTokens()
    |> Parser.enableAST()
    |> Parser.onSequence "function" (fun ctx ->
        ParserContext.enterMode "functionBody" ctx)
    |> Parser.inMode "functionBody" (fun config ->
        config
        |> Parser.onChar '{' (fun ctx -> ctx)
        |> Parser.onChar '}' (fun ctx -> ParserContext.exitMode ctx))

let context = Parser.runString "function test() { return 42 }" parser
let tokens = Parser.getTokens context
let ast = Parser.getAST context
```

## License

MIT License - see [LICENSE](https://github.com/alexzzzs/SharpParser.Core/blob/master/LICENSE) file for details.