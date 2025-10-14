namespace SharpParser.Core

/// Module for automatic AST node construction based on parsing context
module ASTBuilder =

    /// Expression stack for building complex expressions with operator precedence.
    /// Uses the shunting-yard algorithm to handle mathematical expressions correctly.
    /// For example, "2 + 3 * 4" becomes "(2 + (3 * 4))" in the AST.
    type ExpressionStack = {
        /// Stack of operands (AST nodes representing values or subexpressions)
        Operands: ASTNode list
        /// Stack of operators with their precedence levels (higher = tighter binding)
        Operators: (string * int) list  // (operator, precedence)
    }

    /// Creates an empty expression stack
    let emptyExpressionStack () : ExpressionStack = {
        Operands = []
        Operators = []
    }

    /// Gets the precedence level for operators in expressions.
    /// Higher numbers indicate tighter binding (evaluated first).
    /// Based on standard mathematical and programming language conventions.
    let operatorPrecedence = function
        | "*" | "/" -> 7  // Multiplication/division (highest precedence)
        | "+" | "-" -> 6  // Addition/subtraction
        | "<" | ">" | "<=" | ">=" -> 5  // Comparison operators
        | "==" | "!=" -> 4  // Equality operators
        | "&&" -> 3  // Logical AND
        | "||" -> 2  // Logical OR
        | "=" -> 1   // Assignment (lowest precedence)
        | _ -> 0     // Unknown operators

    /// Applies a binary operation to the top two operands on the stack
    let applyBinaryOp (op: string) (stack: ExpressionStack) : ExpressionStack =
        match stack.Operands with
        | right :: left :: rest ->
            let binaryNode = ASTNode.BinaryOp (left, op, right)
            { stack with Operands = binaryNode :: rest }
        | _ -> stack  // Not enough operands

    /// Processes operators on the stack based on precedence
    let rec processOperators (minPrecedence: int) (stack: ExpressionStack) : ExpressionStack =
        match stack.Operators with
        | (op, prec) :: restOps when prec >= minPrecedence ->
            let newStack = applyBinaryOp op { stack with Operators = restOps }
            processOperators minPrecedence newStack
        | _ -> stack

    /// Adds an operand to the expression stack
    let pushOperand (operand: ASTNode) (stack: ExpressionStack) : ExpressionStack =
        { stack with Operands = operand :: stack.Operands }

    /// Adds an operator to the expression stack, processing lower precedence operators first
    let pushOperator (op: string) (stack: ExpressionStack) : ExpressionStack =
        let prec = operatorPrecedence op
        let stackAfterProcessing = processOperators prec stack
        { stackAfterProcessing with Operators = (op, prec) :: stackAfterProcessing.Operators }

    /// Finalizes the expression by processing all remaining operators
    let finalizeExpression (stack: ExpressionStack) : ASTNode option =
        let finalStack = processOperators 0 stack
        match finalStack.Operands with
        | [result] -> Some result
        | [] -> None
        | _ -> None  // Multiple operands left - malformed expression

    /// Gets or creates an expression stack for the current parsing context
    let getExpressionStack (context: ParserContext) : ExpressionStack =
        let stackKey = "ExpressionStack"
        let userData = ParserContextOps.getUserData stackKey context
        match userData with
        | Some (:? ExpressionStack as stack) -> stack
        | _ -> emptyExpressionStack ()

    /// Updates the expression stack in the parsing context
    let setExpressionStack (stack: ExpressionStack) (context: ParserContext) : ParserContext =
        let stackKey = "ExpressionStack"
        ParserContextOps.setUserData stackKey (box stack) context

    /// Helper function to add a node to the expression stack if in expression mode
    let addToExpressionStack (node: ASTNode) (context: ParserContext) : ParserContext =
        let currentMode = ParserContextOps.currentMode context
        if currentMode = Some "expression" then
            let stack = getExpressionStack context
            let newStack = pushOperand node stack
            setExpressionStack newStack context
        else
            context

    /// Builds an AST node based on the current parsing mode and matched text.
    /// This function implements context-aware AST construction with special handling for:
    /// - Expression parsing with operator precedence
    /// - Control flow statements (if, while, function declarations)
    /// - Literals (numbers, strings, booleans)
    /// - Identifiers and variables
    ///
    /// When in "expression" mode, operands are added to the expression stack for proper precedence handling.
    /// <param name="mode">Current parsing mode (e.g., "expression", "functionBody")</param>
    /// <param name="matchedText">The text that was matched by a handler</param>
    /// <param name="context">Current parsing context</param>
    /// <returns>Some AST node if one was created, None if no node should be created (e.g., for operators)</returns>
    let buildNode (mode: string option) (matchedText: string) (context: ParserContext) : ASTNode option =
        let modeStr = mode |> Option.defaultValue ""

        match modeStr, matchedText with
        // Function declarations
        | _, "function" ->
            Some (ASTNode.Function ("", [], []))  // Placeholder, will be updated with name and params

        // Control flow
        | _, "if" ->
            Some (ASTNode.If (ASTNode.Literal "true", [], None))  // Placeholder condition
        | _, "else" ->
            Some (ASTNode.Literal "else")  // Marker for else clause
        | _, "while" ->
            Some (ASTNode.While (ASTNode.Literal "true", []))  // Placeholder condition
        | _, "return" ->
            Some (ASTNode.Return None)  // Placeholder return value

        // Operators - add to expression stack
        | "expression", op when operatorPrecedence op > 0 ->
            let stack = getExpressionStack context
            let newStack = pushOperator op stack
            let updatedContext = setExpressionStack newStack context
            // Don't create a node for operators, just update the stack
            None

        // Identifiers
        | "expression", text when System.Text.RegularExpressions.Regex.IsMatch(text, @"^[a-zA-Z_][a-zA-Z0-9_]*$") ->
            let identifierNode = ASTNode.Variable text
            let updatedContext = addToExpressionStack identifierNode context
            Some identifierNode

        // Numbers
        | "expression", text when System.Double.TryParse text |> fst ->
            let number = System.Double.Parse text
            let numberNode = ASTNode.Number number
            let updatedContext = addToExpressionStack numberNode context
            Some numberNode

        // Strings
        | _, text when text.StartsWith("\"") && text.EndsWith("\"") ->
            let stringContent = text.Trim('"')
            Some (ASTNode.StringLiteral stringContent)

        // Booleans
        | _, "true" -> Some (ASTNode.Boolean true)
        | _, "false" -> Some (ASTNode.Boolean false)

        // Default cases for non-expression contexts
        | _, text when System.Text.RegularExpressions.Regex.IsMatch(text, @"^[a-zA-Z_][a-zA-Z0-9_]*$") ->
            Some (ASTNode.Variable text)

        | _, text when System.Double.TryParse text |> fst ->
            let number = System.Double.Parse text
            Some (ASTNode.Number number)

        | _ ->
            None

    /// Automatically creates and adds an AST node if AST building is enabled
    let autoAddNode (config: ParserConfig) (matchedText: string) (context: ParserContext) : ParserContext =
        // Get current mode for context-aware node building
        let currentMode = ParserContextOps.currentMode context

        // First try custom AST builders
        let customBuilders = ParserConfig.getASTBuilders currentMode config
        let customNode =
            customBuilders
            |> List.tryPick (fun builder -> builder currentMode matchedText context)

        match customNode with
        | Some node ->
            let contextWithNode = ParserContextOps.addASTNode node context
            addToExpressionStack node contextWithNode
        | None ->
            // Fall back to default node building
            match buildNode currentMode matchedText context with
            | Some node ->
                let contextWithNode = ParserContextOps.addASTNode node context
                addToExpressionStack node contextWithNode
            | None -> context

    /// Pushes a node onto the AST node stack for nested structures
    let pushNodeStack (node: ASTNode) (context: ParserContext) : ParserContext =
        // Use UserData to maintain a stack of AST nodes for nested structures
        let stackKey = "ASTNodeStack"
        let currentStack =
            let userData = ParserContextOps.getUserData stackKey context
            match userData with
            | Some (:? (ASTNode list) as list) -> list
            | _ -> []

        let newStack = node :: currentStack
        ParserContextOps.setUserData stackKey (box newStack) context

    /// Pops a node from the AST node stack
    let popNodeStack (context: ParserContext) : (ASTNode * ParserContext) option =
        let stackKey = "ASTNodeStack"
        let userData = ParserContextOps.getUserData stackKey context
        match userData with
        | Some (:? (ASTNode list) as stack) ->
            match stack with
            | node :: remaining ->
                let newStack = remaining
                let updatedContext = ParserContextOps.setUserData stackKey (box newStack) context
                Some (node, updatedContext)
            | [] -> None
        | _ -> None

    /// Gets the current AST node stack
    let getNodeStack (context: ParserContext) : ASTNode list =
        let stackKey = "ASTNodeStack"
        let userData = ParserContextOps.getUserData stackKey context
        match userData with
        | Some (:? (ASTNode list) as list) -> list
        | _ -> []

    /// Clears the AST node stack
    let clearNodeStack (context: ParserContext) : ParserContext =
        let stackKey = "ASTNodeStack"
        ParserContextOps.setUserData stackKey (box []) context

    /// Finalizes any pending expression and adds it to the AST
    let finalizePendingExpression (context: ParserContext) : ParserContext =
        let stack = getExpressionStack context
        match finalizeExpression stack with
        | Some exprNode ->
            // Clear the expression stack
            let contextWithoutStack = setExpressionStack (emptyExpressionStack ()) context
            // Add the finalized expression to AST
            ParserContextOps.addASTNode exprNode contextWithoutStack
        | None -> context

    /// Starts a new expression context
    let startExpression (context: ParserContext) : ParserContext =
        setExpressionStack (emptyExpressionStack ()) context

    /// Handles end-of-expression markers like semicolons
    let handleExpressionEnd (context: ParserContext) : ParserContext =
        finalizePendingExpression context

    /// Builds assignment statements from recent tokens
    let buildAssignment (context: ParserContext) : ParserContext =
        let astNodes = (ParserContextOps.getState context).ASTNodes
        match astNodes with
        | value :: var :: rest ->
            match var, value with
            | ASTNode.Variable varName, (ASTNode.Variable _ | ASTNode.Number _ | ASTNode.StringLiteral _ | ASTNode.Boolean _) ->
                let assignment = ASTNode.Assignment (varName, value)
                let newState = { (ParserContextOps.getState context) with ASTNodes = assignment :: rest }
                ParserContextOps.setState newState context
            | _ -> context
        | _ -> context

    /// Builds function calls from recent tokens
    let buildFunctionCall (context: ParserContext) : ParserContext =
        let astNodes = (ParserContextOps.getState context).ASTNodes
        match astNodes with
        | args :: func :: rest ->
            match func with
            | ASTNode.Variable funcName ->
                // Simple heuristic: if we have a function name followed by arguments
                let call = ASTNode.Call (funcName, [args])  // Simplified - assumes single argument
                let newState = { (ParserContextOps.getState context) with ASTNodes = call :: rest }
                ParserContextOps.setState newState context
            | _ -> context
        | _ -> context