namespace SharpParser.Core.Tests

open Xunit
open FsUnit.Xunit
open SharpParser.Core

module ASTBuilderTests =

    [<Fact>]
    let ``ASTBuilder can build nodes`` () =
        let context = ParserContextOps.create "test.fs" false
        let result = ASTBuilder.buildNode (Some "global") "test" context
        result |> should not' (be Null)

    [<Fact>]
    let ``ASTBuilder handles expression stack`` () =
        let context = ParserContextOps.create "test.fs" false
        let stack = ASTBuilder.getExpressionStack context
        stack.Operands |> should be Empty
        stack.Operators |> should be Empty

    [<Fact>]
    let ``ASTBuilder builds binary operations`` () =
        let context = ParserContextOps.create "test.fs" false
        // Push operands
        let context = ASTBuilder.setExpressionStack (ASTBuilder.pushOperand (ASTNode.Number 2.0) (ASTBuilder.getExpressionStack context)) context
        let context = ASTBuilder.setExpressionStack (ASTBuilder.pushOperator "+" (ASTBuilder.getExpressionStack context)) context
        let context = ASTBuilder.setExpressionStack (ASTBuilder.pushOperand (ASTNode.Number 3.0) (ASTBuilder.getExpressionStack context)) context

        // Finalize expression
        let result = ASTBuilder.finalizeExpression (ASTBuilder.getExpressionStack context)
        match result with
        | Some (ASTNode.BinaryOp (ASTNode.Number 2.0, "+", ASTNode.Number 3.0)) -> Assert.True(true)
        | _ -> Assert.True(false, "Expected binary operation AST node")

    [<Fact>]
    let ``ASTBuilder handles operator precedence`` () =
        let context = ParserContextOps.create "test.fs" false
        // Build: 2 + 3 * 4
        let stack = ASTBuilder.emptyExpressionStack ()
        let stack = ASTBuilder.pushOperand (ASTNode.Number 2.0) stack
        let stack = ASTBuilder.pushOperator "+" stack
        let stack = ASTBuilder.pushOperand (ASTNode.Number 3.0) stack
        let stack = ASTBuilder.pushOperator "*" stack
        let stack = ASTBuilder.pushOperand (ASTNode.Number 4.0) stack

        let result = ASTBuilder.finalizeExpression stack
        result |> should not' (be Null)  // Should create a valid expression tree