namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module ErrorHandlerTests =

    [<Fact>]
    let ``ErrorHandler can trigger errors`` () =
        let config = ParserConfig.create ()
        let context = ParserContextOps.create "test.fs" false
        let errorType = ParseError.GenericError "test error"
        let updated = ErrorHandling.triggerError errorType config context None
        let expectedError = {
            ErrorType = errorType
            Line = 1
            Col = 1
            Mode = None
            Message = "test error"
            Suggestion = None
        }
        updated.State.Errors |> should equal [expectedError]