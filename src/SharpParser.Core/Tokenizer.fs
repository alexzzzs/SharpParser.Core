namespace SharpParser.Core

open System.Collections.Concurrent
open System.Text.RegularExpressions

/// Module for automatic token emission based on matched handlers
module Tokenizer =
    /// Pre-compiled regex for identifier validation (avoids recompilation)
    let private identifierRegex = Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled)

    /// Cache for token type inference to avoid repeated computations
    let private tokenTypeCache = ConcurrentDictionary<string * Set<string>, TokenType>()

    /// Emits a token with the current position and mode
    let emitToken (tokenType: TokenType) (context: ParserContext) : ParserContext =
        let token = {
            Type = tokenType
            Line = context.Line
            Col = context.Col
            Mode = ParserContextOps.currentMode context
        }
        ParserContextOps.addToken token context

    /// Infers token type from matched text and keywords with caching
    let inferTokenType (keywords: Set<string>) (matchedText: string) : TokenType =
        let cacheKey = (matchedText, keywords)
        match tokenTypeCache.TryGetValue(cacheKey) with
        | true, cachedType -> cachedType
        | false, _ ->
            let tokenType =
                if Set.contains matchedText keywords then
                    TokenType.Keyword matchedText
                else
                    // Check for number pattern
                    match System.Double.TryParse matchedText with
                    | true, number -> TokenType.Number number
                    | false, _ ->
                        // Check for identifier pattern using pre-compiled regex
                        if identifierRegex.IsMatch(matchedText) then
                            TokenType.Identifier matchedText
                        else
                            TokenType.Symbol matchedText
            tokenTypeCache.TryAdd(cacheKey, tokenType) |> ignore
            tokenType

    /// Automatically emits a token for matched text if tokenization is enabled
    let autoEmit (keywords: Set<string>) (matchedText: string) (context: ParserContext) : ParserContext =
        // Infer token type and emit token
        let tokenType = inferTokenType keywords matchedText
        emitToken tokenType context