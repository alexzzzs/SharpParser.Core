# Changelog

All notable changes to SharpParser.Core will be documented in this file.

## [Unreleased]

### Added

### Fixed

### Changed

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2025-10-14

### Added
- **Complete Parallel Parsing Implementation**: Fixed and enhanced parallel parsing functionality
  - Proper parallel tokenization with correct line number adjustments
  - Parallel function parsing with configurable boundary detection
  - Multi-language support for JavaScript, F# functions, and C# methods
  - Thread-safe result merging for tokens, AST nodes, and errors
  - Configurable function boundary patterns using regex

- **Comprehensive CI/CD Pipeline**: GitHub Actions automation for quality assurance
  - Multi-platform testing (Windows, Linux, macOS) with .NET 6.0, 7.0, 8.0, 9.0
  - Automated test execution with coverage reporting via Codecov
  - Performance benchmark automation on main branch pushes
  - NuGet package validation and local testing
  - Release validation with version consistency checks
  - Pull request templates and contribution guidelines

- **Dependency Management**: Automated dependency updates
  - Dependabot configuration for NuGet packages and GitHub Actions
  - Weekly update schedule with proper commit prefixes
  - Controlled update frequency to prevent breaking changes

- **Input Validation**: Enhanced robustness of public API functions
  - Added null/empty string validation to `Parser.run` and `Parser.runString`
  - Parameter validation for `onSequence`, `onPattern`, and `inMode` functions
  - Range validation for `withMaxParallelism` and `withMinFunctionsForParallelism`
  - Null key validation for user data functions `getUserData` and `setUserData`
  - Comprehensive XML documentation with parameter descriptions and exception details

- **Parallel Processing Module**: Extracted parallel functionality for better code organization
  - Created dedicated `ParallelProcessing.fs` module for all parallel operations
  - Moved function boundary detection, parallel parsing, and result merging
  - Resolved circular dependencies and improved separation of concerns
  - Maintained backward compatibility with existing API

- **Automated NuGet Publishing**: Complete CI/CD pipeline with automatic package publishing
  - GitHub Actions workflow for automated releases
  - NuGet API key integration for secure publishing
  - Release validation and package creation pipeline

### Fixed
- **Parallel Tokenization Merging**: Corrected token merging logic with proper line offsets
- **Function Boundary Detection**: Replaced hardcoded patterns with configurable regex system
- **Parallel Result Merging**: Fixed state merging for multi-threaded parsing operations

### Changed
- **Test Suite**: Expanded from 138 to 148 tests with parallel parsing coverage
- **README**: Added CI/CD badges and comprehensive contribution guidelines
- **Documentation**: Updated with CI/CD information and improved contributor experience
- **ASTBuilder Module**: Simplified expression stack logic with new helper functions
  - Added `addToExpressionStack` helper to eliminate code duplication
  - Refactored `buildNode` and `autoAddNode` for better maintainability
  - Improved code organization and reduced repetitive expression handling
- **API Documentation**: Enhanced documentation for complex functions
  - Added comprehensive XML documentation to parsing engine functions
  - Improved documentation for AST building and expression parsing
  - Enhanced Trie and PatternMatcher function documentation
  - Better parameter descriptions and usage examples throughout

### Technical Details
- Parallel parsing now correctly handles line number adjustments across function boundaries
- CI pipeline ensures cross-platform compatibility and performance regression detection
- Function boundary detection supports extensible patterns for different programming languages
- Code coverage reporting integrated with automated quality gates
- Input validation prevents runtime exceptions from invalid parameters
- ASTBuilder refactoring improves maintainability without changing functionality
- Enhanced API documentation provides better developer experience
- Parallel processing extraction improves code organization and testability
- Automated release pipeline ensures consistent and reliable package publishing

## [1.2.0] - 2025-10-11

### Added
- **Enhanced AST Building System**: Complete rewrite of the AST builder with proper expression parsing
  - Added expression stack system for handling operator precedence
  - Implemented Shunting-yard algorithm for binary operations
  - Added support for complex mathematical expressions with correct precedence (*, / > +, -)
  - Added expression finalization at statement boundaries (semicolons)
  - Added assignment statement building (`x = value`)
  - Added function call construction with argument handling
  - Improved tree construction for nested expressions

### Changed
- **ASTBuilder Module**: Major refactoring with new ExpressionStack type and precedence handling
- **FullExample**: Updated to demonstrate improved expression parsing capabilities
- **Test Suite**: Added comprehensive tests for expression stack, operator precedence, and binary operations

### Technical Details
- Expression parsing now uses a proper stack-based algorithm instead of hardcoded rules
- Operator precedence follows standard mathematical conventions
- AST nodes are now properly nested for complex expressions
- Expression contexts are tracked separately from statement contexts
- Backward compatibility maintained for existing custom AST builders

## [1.1.0] - 2025-10-6

### Added
- Parallel parsing support for improved performance on multi-core systems
- Enhanced AST types with support for binary operations, unary operations, variables, numbers, and strings
- Configuration validation to catch setup errors early
- Enhanced error handling and debugging with comprehensive error reporting and trace logging
- Functional programming patterns throughout the codebase (immutable data structures, pure functions)

### Changed
- Eliminated mutable state throughout the codebase
- Improved performance with regex anchoring and trie Dictionary optimization
- Enhanced token caching for better memory efficiency

### Fixed
- Proper invocation of error handlers
- Configuration validation for conflicting handlers

## [1.0.0] - 2025 10 - 5

### Added
- Initial release of SharpParser.Core
- Character, sequence, and pattern handler support
- Context-sensitive parsing with mode stacks
- Automatic tokenization and AST construction
- Comprehensive test coverage (138 tests)
- Fluent, chainable API
- Trie-based sequence matching for efficiency
- Compiled regex patterns for fast matching