# Changelog

All notable changes to SharpParser.Core will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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