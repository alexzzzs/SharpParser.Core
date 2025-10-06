namespace SharpParser.Examples

open System

/// Main entry point for the SharpParser examples application
module Program =
    /// Main function
    [<EntryPoint>]
    let main argv =
        printfn "SharpParser.Core Examples"
        printfn "========================"
        printfn ""

        try
            // Run all examples in sequence
            BasicExample.run ()
            ModeExample.run ()
            FullExample.run ()
            JsonExample.run ()
            CsvExample.run ()
            ExpressionExample.run ()
            IniExample.run ()

            printfn "=== All Examples Completed ==="
            printfn "Thank you for trying SharpParser.Core!"

            0 // Return success code

        with
        | ex ->
            printfn "Error running examples: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            1 // Return error code