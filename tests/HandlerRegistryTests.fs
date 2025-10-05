namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module HandlerRegistryTests =

    [<Fact>]
    let ``HandlerRegistry can be created`` () =
        let registry = HandlerRegistry.empty ()
        registry |> should not' (be Null)

    [<Fact>]
    let ``HandlerRegistry can register char handler`` () =
        let registry = HandlerRegistry.empty ()
        let handler = fun ctx -> ctx
        let updated = HandlerRegistry.addCharHandler None 'a' handler registry
        updated |> should not' (be Null)

    [<Fact>]
    let ``HandlerRegistry can register sequence handler`` () =
        let registry = HandlerRegistry.empty ()
        let handler = fun ctx -> ctx
        let updated = HandlerRegistry.addSequenceHandler None "test" handler registry
        updated |> should not' (be Null)

    [<Fact>]
    let ``HandlerRegistry can register pattern handler`` () =
        let registry = HandlerRegistry.empty ()
        let handler = fun ctx matched -> ctx
        let updated = HandlerRegistry.addPatternHandler None @"\d+" handler registry
        updated |> should not' (be Null)