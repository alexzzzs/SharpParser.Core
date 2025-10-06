namespace SharpParser.Core.Benchmarks

open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open SharpParser.Core

[<MemoryDiagnoser>]
[<RankColumn>]
type ParserBenchmarks() =

    // Simple character handler benchmark
    [<Benchmark>]
    member _.SimpleCharacterParsing() =
        let parser = Parser.create() |> Parser.onChar 'a' (fun ctx -> ctx)
        let input = String.replicate 1000 "a"
        Parser.runString input parser |> ignore

    // Sequence handler benchmark (keywords)
    [<Benchmark>]
    member _.SequenceParsing() =
        let parser =
            Parser.create()
            |> Parser.onSequence "function" (fun ctx -> ctx)
            |> Parser.onSequence "if" (fun ctx -> ctx)
            |> Parser.onSequence "else" (fun ctx -> ctx)
            |> Parser.onSequence "return" (fun ctx -> ctx)
        let input = "function if else return " |> String.replicate 100
        Parser.runString input parser |> ignore

    // Pattern handler benchmark (identifiers and numbers)
    [<Benchmark>]
    member _.PatternParsing() =
        let parser =
            Parser.create()
            |> Parser.onPattern @"\d+" (fun ctx _ -> ctx)
            |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx _ -> ctx)
        let input = "var123 abc_def 42 999 " |> String.replicate 100
        Parser.runString input parser |> ignore

    // Mode-based parsing benchmark
    [<Benchmark>]
    member _.ModeBasedParsing() =
        let parser =
            Parser.create()
            |> Parser.onSequence "function" (fun ctx -> ParserContextOps.enterMode "body" ctx)
            |> Parser.inMode "body" (fun config ->
                config
                |> Parser.onChar '{' (fun ctx -> ctx)
                |> Parser.onChar '}' (fun ctx -> ParserContextOps.exitMode ctx))
        let input = "function { } function { } " |> String.replicate 50
        Parser.runString input parser |> ignore

    // Tokenization enabled benchmark
    [<Benchmark>]
    member _.TokenizationEnabled() =
        let parser =
            Parser.create()
            |> Parser.enableTokens()
            |> Parser.onSequence "let" (fun ctx -> ctx)
            |> Parser.onPattern @"\d+" (fun ctx _ -> ctx)
            |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx _ -> ctx)
        let input = "let x = 42 let y = 123 " |> String.replicate 50
        Parser.runString input parser |> ignore

    // AST building enabled benchmark
    [<Benchmark>]
    member _.ASTBuildingEnabled() =
        let parser =
            Parser.create()
            |> Parser.enableAST()
            |> Parser.onSequence "function" (fun ctx -> ctx)
            |> Parser.onSequence "return" (fun ctx -> ctx)
            |> Parser.onPattern @"\d+" (fun ctx _ -> ctx)
        let input = "function return 42 function return 123 " |> String.replicate 50
        Parser.runString input parser |> ignore

    // Large input parsing benchmark
    [<Benchmark>]
    member _.LargeInputParsing() =
        let parser = Parser.create() |> Parser.onChar 'x' (fun ctx -> ctx)
        let input = String.replicate 10000 "x"
        Parser.runString input parser |> ignore

    // Complex parsing with all features enabled
    [<Benchmark>]
    member _.FullFeatureParsing() =
        let parser =
            Parser.create()
            |> Parser.enableTokens()
            |> Parser.enableAST()
            |> Parser.enableTrace false
            |> Parser.onSequence "function" (fun ctx -> ParserContextOps.enterMode "body" ctx)
            |> Parser.onSequence "if" (fun ctx -> ctx)
            |> Parser.onSequence "return" (fun ctx -> ctx)
            |> Parser.onChar '+' (fun ctx -> ctx)
            |> Parser.onChar '=' (fun ctx -> ctx)
            |> Parser.onPattern @"\d+" (fun ctx _ -> ctx)
            |> Parser.onPattern @"[a-zA-Z_][a-zA-Z0-9_]*" (fun ctx _ -> ctx)
            |> Parser.inMode "body" (fun config ->
                config
                |> Parser.onChar '{' (fun ctx -> ctx)
                |> Parser.onChar '}' (fun ctx -> ParserContextOps.exitMode ctx))
        let input = "function calculate(x, y) {\n    if x > 0 {\n        return x + y\n    }\n    return 0\n}\nfunction test(a, b) {\n    return a + b\n}\n" |> String.replicate 10
        Parser.runString input parser |> ignore

module Program =
    [<EntryPoint>]
    let main argv =
        let summary = BenchmarkRunner.Run<ParserBenchmarks>()
        0