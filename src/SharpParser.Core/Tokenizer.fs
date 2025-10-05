namespace SharpParser.Core

/// Module for automatic token emission based on matched handlers
module Tokenizer =
    /// Emits a token with the current position and mode
    let emitToken (tokenType: TokenType) (context: ParserContext) : ParserContext =
        let token = {
            Type = tokenType
            Line = context.Line
            Col = context.Col
            Mode = ParserContextOps.currentMode context
        }
        ParserContextOps.addToken token context

    /// Infers token type from matched text and keywords
    let inferTokenType (keywords: Set<string>) (matchedText: string) : TokenType =
        if Set.contains matchedText keywords then
            TokenType.Keyword matchedText
        else
            // Check for number pattern
            match System.Double.TryParse matchedText with
            | true, number -> TokenType.Number number
            | false, _ ->
                // Check for identifier pattern (starts with letter or underscore)
                if System.Text.RegularExpressions.Regex.IsMatch(matchedText, @"^[a-zA-Z_][a-zA-Z0-9_]*$") then
                    TokenType.Identifier matchedText
                else
                    TokenType.Symbol matchedText

    /// Automatically emits a token for matched text if tokenization is enabled
    let autoEmit (keywords: Set<string>) (matchedText: string) (context: ParserContext) : ParserContext =
        // Infer token type and emit token
        let tokenType = inferTokenType keywords matchedText
        emitToken tokenType context