namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit
open SharpParser.Core
open System.Text.RegularExpressions

module PatternMatcherTests =

    [<Fact>]
    let ``PatternMatcher.compile creates regex with compiled option`` () =
        let regex = PatternMatcher.compile @"\d+"
        regex.Options.HasFlag(RegexOptions.Compiled) |> should be True

    [<Fact>]
    let ``PatternMatcher.tryMatch finds match at start position`` () =
        let regex = PatternMatcher.compile @"\d+"
        let text = "123abc"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal (Some (3, "123"))

    [<Fact>]
    let ``PatternMatcher.tryMatch returns None when no match at start`` () =
        let regex = PatternMatcher.compile @"\d+"
        let text = "abc123"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal None

    [<Fact>]
    let ``PatternMatcher.tryMatch handles position offset`` () =
        let regex = PatternMatcher.compile @"\d+"
        let text = "abc123def"
        let result = PatternMatcher.tryMatch regex text 3 // Start at "123"
        result |> should equal (Some (3, "123"))

    [<Fact>]
    let ``PatternMatcher.tryMatch returns None for out of bounds position`` () =
        let regex = PatternMatcher.compile @"\d+"
        let text = "123"
        let result = PatternMatcher.tryMatch regex text 10
        result |> should equal None

    [<Fact>]
    let ``PatternMatcher.tryMatch handles empty string`` () =
        let regex = PatternMatcher.compile @""
        let text = "hello"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal (Some (0, ""))

    [<Fact>]
    let ``PatternMatcher.matchAll returns first successful match`` () =
        let patterns = [
            PatternMatcher.compile @"[a-z]+"
            PatternMatcher.compile @"\d+"
            PatternMatcher.compile @"[A-Z]+"
        ]
        let text = "123abc"
        let result = PatternMatcher.matchAll patterns text 0
        result |> should equal (Some (3, "123", 1)) // First pattern that matches is \d+ at index 1

    [<Fact>]
    let ``PatternMatcher.matchAll returns None when no patterns match`` () =
        let patterns = [
            PatternMatcher.compile @"\d+"
            PatternMatcher.compile @"[a-z]+"
        ]
        let text = "ABC"
        let result = PatternMatcher.matchAll patterns text 0
        result |> should equal None

    [<Fact>]
    let ``PatternMatcher.getOrCompile caches compiled regexes`` () =
        let pattern = @"test\d+"
        let regex1 = PatternMatcher.getOrCompile pattern
        let regex2 = PatternMatcher.getOrCompile pattern
        regex1 = regex2 |> should be True

    [<Fact>]
    let ``PatternMatcher.getOrCompile handles different patterns`` () =
        let pattern1 = @"test\d+"
        let pattern2 = @"other\d+"
        let regex1 = PatternMatcher.getOrCompile pattern1
        let regex2 = PatternMatcher.getOrCompile pattern2
        regex1 <> regex2 |> should be True

    [<Fact>]
    let ``PatternMatcher handles word boundaries`` () =
        let regex = PatternMatcher.compile @"\b\w+\b"
        let text = "hello world"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal (Some (5, "hello"))

    [<Fact>]
    let ``PatternMatcher handles character classes`` () =
        let regex = PatternMatcher.compile @"[A-Za-z_][A-Za-z0-9_]*"
        let text = "variable_name123"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal (Some (16, "variable_name123"))

    [<Fact>]
    let ``PatternMatcher handles quantifiers`` () =
        let regex = PatternMatcher.compile @"\d{2,4}"
        let text = "12345"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal (Some (4, "1234")) // Matches 4 digits as it's within 2-4 range

    [<Fact>]
    let ``PatternMatcher handles anchors`` () =
        let regex = PatternMatcher.compile @"^start"
        let text = "start of line"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal (Some (5, "start"))

    [<Fact>]
    let ``PatternMatcher handles groups`` () =
        let regex = PatternMatcher.compile @"(\w+)\s*=\s*(\w+)"
        let text = "x = 42"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal (Some (6, "x = 42")) // Note: this matches the full pattern

    [<Fact>]
    let ``PatternMatcher handles invalid regex gracefully`` () =
        // Invalid regex should not crash compilation
        try
            let regex = PatternMatcher.compile "[unclosed"
            let text = "test"
            let result = PatternMatcher.tryMatch regex text 0
            result |> should equal None // Should not match due to invalid regex
        with
        | :? System.ArgumentException -> Assert.True(true) // Expected for invalid regex
        | ex -> Assert.True(false, sprintf "Unexpected exception: %s" ex.Message)

    [<Fact>]
    let ``PatternMatcher handles empty pattern`` () =
        let regex = PatternMatcher.compile ""
        let text = "hello"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal (Some (0, ""))

    [<Fact>]
    let ``PatternMatcher handles position beyond string length`` () =
        let regex = PatternMatcher.compile @"\d+"
        let text = "123"
        let result = PatternMatcher.tryMatch regex text 5
        result |> should equal None

    [<Fact>]
    let ``PatternMatcher handles negative position`` () =
        let regex = PatternMatcher.compile @"\d+"
        let text = "123"
        let result = PatternMatcher.tryMatch regex text -1
        result |> should equal None

    [<Fact>]
    let ``PatternMatcher.matchAll handles empty patterns list`` () =
        let patterns = []
        let text = "test"
        let result = PatternMatcher.matchAll patterns text 0
        result |> should equal None

    [<Fact>]
    let ``PatternMatcher handles very long patterns`` () =
        let longPattern = String.replicate 1000 "a" + "+"
        let regex = PatternMatcher.compile longPattern
        let text = String.replicate 1000 "a" + "b"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal (Some (1000, String.replicate 1000 "a"))

    [<Fact>]
    let ``PatternMatcher handles unicode characters`` () =
        let regex = PatternMatcher.compile @"[\u0041-\u005A]+" // A-Z
        let text = "HELLO世界"
        let result = PatternMatcher.tryMatch regex text 0
        result |> should equal (Some (5, "HELLO"))

    [<Property>]
    let ``PatternMatcher.tryMatch never exceeds string length`` (text: string) (pattern: string) (pos: int) =
        (not (isNull text) && not (isNull pattern)) ==> lazy (
            let safePos = max 0 (min pos text.Length)
            try
                let regex = PatternMatcher.compile pattern
                let result = PatternMatcher.tryMatch regex text safePos
                match result with
                | Some (len, matched) ->
                    safePos + len <= text.Length && matched.Length = len
                | None -> true
            with
            | :? System.ArgumentException -> true // Invalid regex is acceptable
            | _ -> false
        )


    [<Property>]
    let ``PatternMatcher caching works correctly`` (pattern: string) =
        if not (isNull pattern || pattern = "") then
            try
                let regex1 = PatternMatcher.getOrCompile pattern
                let regex2 = PatternMatcher.getOrCompile pattern
                regex1 = regex2
            with
            | :? System.ArgumentException -> true
            | _ -> false
        else
            true