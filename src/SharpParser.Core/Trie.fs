namespace SharpParser.Core

/// A node in the trie data structure
type TrieNode<'T> = {
    /// Child nodes indexed by character
    Children: Map<char, TrieNode<'T>>
    /// Whether this node represents the end of a sequence
    IsTerminal: bool
    /// The value associated with this terminal node (if any)
    Value: 'T option
}

/// Module for trie operations
module Trie =
    /// Creates an empty trie node
    let empty<'T> () : TrieNode<'T> =
        {
            Children = Map.empty
            IsTerminal = false
            Value = None
        }

    /// Inserts a sequence into the trie with an associated value
    let rec insert (sequence: string) (value: 'T) (node: TrieNode<'T>) : TrieNode<'T> =
        match sequence with
        | "" ->
            { node with
                IsTerminal = true
                Value = Some value }
        | s when s.Length > 0 ->
            let firstChar = s.[0]
            let rest = s.Substring(1)
            let childNode =
                match Map.tryFind firstChar node.Children with
                | Some existingChild -> insert rest value existingChild
                | None -> insert rest value (empty ())
            { node with
                Children = Map.add firstChar childNode node.Children }
        | _ -> node

    /// Searches for an exact sequence match in the trie
    let rec search (sequence: string) (node: TrieNode<'T>) : 'T option =
        match sequence with
        | "" ->
            if node.IsTerminal then node.Value else None
        | s when s.Length > 0 ->
            let firstChar = s.[0]
            let rest = s.Substring(1)
            match Map.tryFind firstChar node.Children with
            | Some childNode -> search rest childNode
            | None -> None
        | _ -> None

    /// Finds the longest matching sequence starting at the given position in text.
    /// Traverses the trie character by character, keeping track of the last successful terminal match.
    /// This implements the "longest match" semantics where longer sequences take precedence over shorter ones.
    let longestMatch (text: string) (position: int) (node: TrieNode<'T>) : (int * 'T) option =
        // Recursive helper function that traverses the trie and text simultaneously
        let rec traverse currentPos currentNode lastMatch =
            // If we've reached the end of the text, return the last successful match (if any)
            if currentPos >= text.Length then
                lastMatch
            else
                // Get the character at the current position in the text
                let currentChar = text.[currentPos]
                // Try to find a child node for this character in the trie
                match Map.tryFind currentChar currentNode.Children with
                | Some childNode ->
                    // Character exists in trie - continue traversing
                    // Update lastMatch if this node represents a complete sequence (terminal)
                    let newLastMatch =
                        if childNode.IsTerminal then
                            // Calculate match length: currentPos + 1 (after this char) - starting position
                            // Include the associated value from the terminal node
                            Some (currentPos + 1 - position, childNode.Value.Value)
                        else
                            // Not a terminal node, keep previous lastMatch
                            lastMatch
                    // Continue with next character and child node
                    traverse (currentPos + 1) childNode newLastMatch
                | None ->
                    // Character not found in trie - stop and return last successful match
                    // This ensures we return the longest valid prefix that was a terminal match
                    lastMatch
        // Start traversal from the given position with no previous matches
        traverse position node None