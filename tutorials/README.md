# SharpParser.Core Tutorials

This directory contains step-by-step tutorials that teach you how to build real-world parsers using SharpParser.Core. Each tutorial includes complete working code examples and explains the concepts progressively.

## Available Tutorials

### [Parsing JSON with SharpParser](Parsing-JSON-with-SharpParser.md)
Learn how to build a JSON parser that handles:
- Primitive values (strings, numbers, booleans, null)
- Objects and arrays with proper nesting
- Context-sensitive parsing using modes
- AST building for JSON structures

**Difficulty**: Intermediate
**Time**: 30-45 minutes

### [Building a CSV Parser](Building-a-CSV-Parser.md)
Master CSV parsing with proper handling of:
- Quoted fields containing commas
- Escaped quotes within fields
- Mixed quoted and unquoted fields
- Record separation and field grouping

**Difficulty**: Intermediate
**Time**: 30-45 minutes

### [Creating Configuration File Parsers](Creating-Configuration-File-Parsers.md)
Build parsers for configuration files including:
- INI file format with sections and key-value pairs
- Comment handling
- Properties files (Java-style)
- Extension patterns for other config formats

**Difficulty**: Beginner to Intermediate
**Time**: 25-35 minutes

## Tutorial Structure

Each tutorial follows this structure:

1. **Prerequisites** - What you need to get started
2. **Project Setup** - How to create and configure a new project
3. **Understanding the Format** - Overview of the data format being parsed
4. **Step-by-step Implementation** - Progressive building of the parser
5. **Testing and Examples** - Working code with test cases
6. **Advanced Features** - Optional enhancements
7. **Next Steps** - What to learn or build next

## Prerequisites for All Tutorials

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- SharpParser.Core NuGet package
- Basic understanding of F# syntax
- A text editor or IDE (Visual Studio, VS Code with Ionide, Rider)

## Getting Started

1. Choose a tutorial based on your interests
2. Follow the setup instructions to create a new project
3. Work through each step, running the code as you go
4. Experiment with the examples and modify them
5. Check the related example files in `examples/` for complete implementations

## Learning Path

If you're new to SharpParser, we recommend:

1. Start with **Configuration File Parsers** (simplest concepts)
2. Move to **CSV Parser** (pattern matching and escaping)
3. Advance to **JSON Parser** (modes and complex structures)

## Additional Resources

- [Main Documentation](../../README.md) - Overview and API reference
- [Examples Directory](../../examples/) - Complete working examples
- [API Documentation](../../docs/API.md) - Detailed function reference
- [Architecture Guide](../../docs/Architecture.md) - How SharpParser works internally

## Contributing

Found an issue with a tutorial or want to add a new one? Check our [contributing guidelines](../../README.md#contributing).

## Questions?

- Open an issue on [GitHub](https://github.com/alexzzzs/SharpParser.Core/issues)
- Check existing examples in the `examples/` directory
- Review the API documentation for function details