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

    /// Finds the longest matching sequence starting at the given position in text
    let longestMatch (text: string) (position: int) (node: TrieNode<'T>) : (int * 'T) option =
        let rec traverse currentPos currentNode lastMatch =
            if currentPos >= text.Length then
                lastMatch
            else
                let currentChar = text.[currentPos]
                match Map.tryFind currentChar currentNode.Children with
                | Some childNode ->
                    let newLastMatch =
                        if childNode.IsTerminal then
                            Some (currentPos + 1 - position, childNode.Value.Value)
                        else
                            lastMatch
                    traverse (currentPos + 1) childNode newLastMatch
                | None ->
                    lastMatch
        traverse position node None