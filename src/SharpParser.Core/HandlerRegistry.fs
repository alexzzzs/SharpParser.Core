namespace SharpParser.Core

open System.Text.RegularExpressions

/// Registry for storing and looking up handlers organized by parsing mode
type HandlerRegistry = {
    /// Character handlers organized by mode and character
    CharHandlers: Map<string option, Map<char, CharHandler list>>
    /// Sequence handlers stored in tries organized by mode
    SequenceTrie: Map<string option, TrieNode<SequenceHandler>>
    /// Pattern handlers organized by mode (regex and handler pairs)
    PatternHandlers: Map<string option, (Regex * PatternHandler) list>
    /// Global error handlers
    ErrorHandlers: ErrorHandler list
}

/// Module for handler registry operations
module HandlerRegistry =
    /// Creates an empty handler registry
    let empty () : HandlerRegistry =
        {
            CharHandlers = Map.empty
            SequenceTrie = Map.empty
            PatternHandlers = Map.empty
            ErrorHandlers = []
        }

    /// Adds a character handler for a specific mode
    let addCharHandler (mode: string option) (character: char) (handler: CharHandler) (registry: HandlerRegistry) : HandlerRegistry =
        let modeKey = mode
        let charMap =
            match Map.tryFind modeKey registry.CharHandlers with
            | Some existingMap ->
                match Map.tryFind character existingMap with
                | Some existingHandlers -> Map.add character (handler :: existingHandlers) existingMap
                | None -> Map.add character [handler] existingMap
            | None ->
                Map.empty |> Map.add character [handler]

        { registry with CharHandlers = Map.add modeKey charMap registry.CharHandlers }

    /// Adds a sequence handler for a specific mode
    let addSequenceHandler (mode: string option) (sequence: string) (handler: SequenceHandler) (registry: HandlerRegistry) : HandlerRegistry =
        let modeKey = mode
        let trie =
            match Map.tryFind modeKey registry.SequenceTrie with
            | Some existingTrie -> Trie.insert sequence handler existingTrie
            | None -> Trie.insert sequence handler (Trie.empty ())

        { registry with SequenceTrie = Map.add modeKey trie registry.SequenceTrie }

    /// Adds a pattern handler for a specific mode
    let addPatternHandler (mode: string option) (pattern: string) (handler: PatternHandler) (registry: HandlerRegistry) : HandlerRegistry =
        let modeKey = mode
        let regex = PatternMatcher.getOrCompile pattern
        let patternEntry = (regex, handler)

        let patterns =
            match Map.tryFind modeKey registry.PatternHandlers with
            | Some existingPatterns -> patternEntry :: existingPatterns
            | None -> [patternEntry]

        { registry with PatternHandlers = Map.add modeKey patterns registry.PatternHandlers }

    /// Adds a global error handler
    let addErrorHandler (handler: ErrorHandler) (registry: HandlerRegistry) : HandlerRegistry =
        { registry with ErrorHandlers = handler :: registry.ErrorHandlers }

    /// Gets all character handlers for a character in a specific mode
    let getCharHandlers (mode: string option) (character: char) (registry: HandlerRegistry) : CharHandler list =
        // Try specific mode first, then fall back to global mode
        let tryMode targetMode =
            match Map.tryFind targetMode registry.CharHandlers with
            | Some charMap ->
                match Map.tryFind character charMap with
                | Some handlers -> handlers
                | None -> []
            | None -> []

        let specificHandlers = tryMode mode
        let globalHandlers = tryMode None
        specificHandlers @ globalHandlers

    /// Finds the longest sequence match starting at the given position
    let matchSequence (mode: string option) (text: string) (position: int) (registry: HandlerRegistry) : (int * SequenceHandler) option =
        // Try specific mode first, then fall back to global mode
        let tryMode targetMode =
            match Map.tryFind targetMode registry.SequenceTrie with
            | Some trie -> Trie.longestMatch text position trie
            | None -> None

        match tryMode mode with
        | Some result -> Some result
        | None -> tryMode None

    /// Finds the first pattern match starting at the given position
    let matchPattern (mode: string option) (text: string) (position: int) (registry: HandlerRegistry) : (int * string * PatternHandler) option =
        // Try specific mode first, then fall back to global mode
        let tryMode targetMode =
            match Map.tryFind targetMode registry.PatternHandlers with
            | Some patterns ->
                let regexList = patterns |> List.map fst
                match PatternMatcher.matchAll regexList text position with
                | Some (length, matchedText, patternIndex) ->
                    let (_, handler) = patterns.[patternIndex]
                    Some (length, matchedText, handler)
                | None -> None
            | None -> None

        match tryMode mode with
        | Some result -> Some result
        | None -> tryMode None

    /// Gets all error handlers
    let getErrorHandlers (registry: HandlerRegistry) : ErrorHandler list =
        registry.ErrorHandlers