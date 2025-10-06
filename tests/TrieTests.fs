namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit
open SharpParser.Core

module TrieTests =

    [<Fact>]
    let ``Trie.empty creates empty trie node`` () =
        let trie = Trie.empty<string> ()
        trie.Children |> should be Empty
        trie.IsTerminal |> should be False
        trie.Value |> should equal None

    [<Fact>]
    let ``Trie.insert adds single character sequence`` () =
        let trie = Trie.empty<string> () |> Trie.insert "a" "valueA"
        let success, childNode = trie.Children.TryGetValue('a')
        success |> should be True
        childNode.IsTerminal |> should be True
        childNode.Value |> should equal (Some "valueA")

    [<Fact>]
    let ``Trie.insert adds multi-character sequence`` () =
        let trie = Trie.empty<string> () |> Trie.insert "hello" "greeting"
        let result = Trie.search "hello" trie
        result |> should equal (Some "greeting")

    [<Fact>]
    let ``Trie.insert handles multiple sequences with common prefix`` () =
        let trie =
            Trie.empty<string> ()
            |> Trie.insert "hello" "greeting"
            |> Trie.insert "help" "assistance"
            |> Trie.insert "world" "planet"

        Trie.search "hello" trie |> should equal (Some "greeting")
        Trie.search "help" trie |> should equal (Some "assistance")
        Trie.search "world" trie |> should equal (Some "planet")
        Trie.search "hell" trie |> should equal None

    [<Fact>]
    let ``Trie.search finds exact matches`` () =
        let trie =
            Trie.empty<string> ()
            |> Trie.insert "function" "keyword"
            |> Trie.insert "fun" "not-keyword"

        Trie.search "function" trie |> should equal (Some "keyword")
        Trie.search "fun" trie |> should equal (Some "not-keyword")
        Trie.search "func" trie |> should equal None

    [<Fact>]
    let ``Trie.search returns None for non-existent sequences`` () =
        let trie = Trie.empty<string> () |> Trie.insert "test" "value"
        Trie.search "nonexistent" trie |> should equal None

    [<Fact>]
    let ``Trie.longestMatch finds longest prefix match`` () =
        let trie =
            Trie.empty<string> ()
            |> Trie.insert "function" "keyword"
            |> Trie.insert "fun" "not-keyword"

        let text = "function call"
        let result = Trie.longestMatch text 0 trie
        result |> should equal (Some (8, "keyword")) // "function" is 8 chars

    [<Fact>]
    let ``Trie.longestMatch returns None when no match`` () =
        let trie = Trie.empty<string> () |> Trie.insert "test" "value"
        let text = "xyz"
        let result = Trie.longestMatch text 0 trie
        result |> should equal None

    [<Fact>]
    let ``Trie.longestMatch handles position offset`` () =
        let trie = Trie.empty<string> () |> Trie.insert "abc" "value"
        let text = "xyzabc"
        let result = Trie.longestMatch text 3 trie // Start at position 3 ("abc")
        result |> should equal (Some (3, "value"))

    [<Fact>]
    let ``Trie.longestMatch prefers longer matches`` () =
        let trie =
            Trie.empty<string> ()
            |> Trie.insert "if" "short"
            |> Trie.insert "ifelse" "long"

        let text = "ifelse statement"
        let result = Trie.longestMatch text 0 trie
        result |> should equal (Some (6, "long")) // "ifelse" is 6 chars

    [<Fact>]
    let ``Trie.longestMatch stops at first non-matching character`` () =
        let trie = Trie.empty<string> () |> Trie.insert "hello" "greeting"
        let text = "hellox"
        let result = Trie.longestMatch text 0 trie
        result |> should equal (Some (5, "greeting")) // "hello" matches, "x" doesn't

    [<Fact>]
    let ``Trie handles empty string insertion`` () =
        let trie = Trie.empty<string> () |> Trie.insert "" "empty"
        trie.IsTerminal |> should be True
        trie.Value |> should equal (Some "empty")

    [<Fact>]
    let ``Trie handles empty string search`` () =
        let trie = Trie.empty<string> () |> Trie.insert "" "empty"
        let result = Trie.search "" trie
        result |> should equal (Some "empty")

    [<Fact>]
    let ``Trie handles overlapping sequences`` () =
        let trie =
            Trie.empty<int> ()
            |> Trie.insert "a" 1
            |> Trie.insert "ab" 2
            |> Trie.insert "abc" 3

        Trie.search "a" trie |> should equal (Some 1)
        Trie.search "ab" trie |> should equal (Some 2)
        Trie.search "abc" trie |> should equal (Some 3)
        Trie.search "abcd" trie |> should equal None

    [<Fact>]
    let ``Trie handles empty strings`` () =
        let trie = Trie.empty<string> () |> Trie.insert "" "empty"
        trie.IsTerminal |> should be True
        trie.Value |> should equal (Some "empty")
        Trie.search "" trie |> should equal (Some "empty")

    [<Fact>]
    let ``Trie.longestMatch handles empty text`` () =
        let trie = Trie.empty<string> () |> Trie.insert "test" "value"
        let result = Trie.longestMatch "" 0 trie
        result |> should equal None

    [<Fact>]
    let ``Trie.longestMatch handles position beyond text length`` () =
        let trie = Trie.empty<string> () |> Trie.insert "test" "value"
        let result = Trie.longestMatch "hi" 5 trie
        result |> should equal None

    [<Fact>]
    let ``Trie handles very long sequences`` () =
        let longSeq = String.replicate 1000 "a"
        let trie = Trie.empty<string> () |> Trie.insert longSeq "long"
        let result = Trie.search longSeq trie
        result |> should equal (Some "long")

    [<Fact>]
    let ``Trie handles unicode characters`` () =
        let trie = Trie.empty<string> () |> Trie.insert "héllo" "unicode"
        let result = Trie.search "héllo" trie
        result |> should equal (Some "unicode")

    [<Fact>]
    let ``Trie handles case sensitivity`` () =
        let trie = Trie.empty<string> () |> Trie.insert "Hello" "mixed"
        Trie.search "hello" trie |> should equal None
        Trie.search "Hello" trie |> should equal (Some "mixed")

    [<Property>]
    let ``Trie insert and search roundtrip`` (key: string) (value: int) =
        (not (isNull key)) ==> lazy (
            let trie = Trie.empty<int> () |> Trie.insert key value
            let result = Trie.search key trie
            result = Some value
        )
