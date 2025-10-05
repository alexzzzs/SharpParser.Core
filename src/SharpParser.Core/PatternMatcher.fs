namespace SharpParser.Core

open System.Text.RegularExpressions

/// Module for pattern matching functionality using .NET Regex
module PatternMatcher =
    /// Compiles a pattern string to a Regex with appropriate options
    let compile (pattern: string) : Regex =
        // Use Compiled for performance, and anchor at start for position-based matching
        new Regex(pattern, RegexOptions.Compiled)

    /// Attempts to match a pattern in text starting at the given position
    let tryMatch (regex: Regex) (text: string) (position: int) : (int * string) option =
        if position < 0 || position >= text.Length then
            None
        else
            let substring = text.Substring(position)
            let matchResult = regex.Match(substring)
            if matchResult.Success && matchResult.Index = 0 then
                Some (matchResult.Length, matchResult.Value)
            else
                None

    /// Tries all patterns in order and returns the first successful match.
    /// This implements "first match wins" semantics for pattern handlers.
    /// Returns the match length, matched text, and the index of the matching pattern.
    let matchAll (patterns: Regex list) (text: string) (position: int) : (int * string * int) option =
        patterns
        |> List.mapi (fun index pattern ->  // Add index to track which pattern matched
            match tryMatch pattern text position with  // Try this pattern at the position
            | Some (length, matchedText) -> Some (length, matchedText, index)  // Success: include pattern index
            | None -> None)  // This pattern didn't match
        |> List.tryFind Option.isSome  // Find the first successful match (if any)
        |> Option.flatten  // Unwrap the nested Option

    /// Cache for compiled regex patterns to avoid recompilation
    let private patternCache = ref Map.empty<string, Regex>

    /// Gets or creates a compiled regex for the given pattern
    let getOrCompile (pattern: string) : Regex =
        match Map.tryFind pattern !patternCache with
        | Some regex -> regex
        | None ->
            let regex = compile pattern
            patternCache := Map.add pattern regex !patternCache
            regex