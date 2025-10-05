namespace SharpParser.Core

/// Module for automatic AST node construction based on parsing context
module ASTBuilder =
    /// Builds an AST node based on mode and matched text
    let buildNode (mode: string) (matchedText: string) (context: ParserContext) : ASTNode option =
        match mode, matchedText with
        | "functionBody", "function" ->
            // Create a function node - this is a simplified example
            Some (ASTNode.Function ("unnamed", [], []))
        | "ifBody", "if" ->
            // Create an if node - this is a simplified example
            Some (ASTNode.If (ASTNode.Literal "true", [], None))
        | _, "=" ->
            // Create an assignment node - this is a simplified example
            Some (ASTNode.Assignment ("variable", ASTNode.Literal "value"))
        | _, text when System.Text.RegularExpressions.Regex.IsMatch(text, @"^[a-zA-Z_][a-zA-Z0-9_]*$") ->
            // Identifiers become expressions or literals
            if mode = "expression" then
                Some (ASTNode.Expression text)
            else
                Some (ASTNode.Literal text)
        | _, text when System.Double.TryParse text |> fst ->
            // Numbers become literals
            Some (ASTNode.Literal text)
        | _ ->
            // No AST node should be created for this match
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
        | Some node -> ParserContextOps.addASTNode node context
        | None ->
            // Fall back to default node building
            match buildNode (currentMode |> Option.defaultValue "") matchedText context with
            | Some node -> ParserContextOps.addASTNode node context
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