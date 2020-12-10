using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.Framework.Impl.CompileTime;
using System.Net.Http.Headers;

namespace Caravela.Framework.Impl.Templating
{
    // ReSharper disable TailRecursiveCall

    /// <summary>
    /// A <see cref="CSharpSyntaxRewriter"/> that adds annotation that distinguish compile-time from
    /// run-time syntax nodes. The input should be a syntax tree annotated with a <see cref="SemanticAnnotationMap"/>.
    /// </summary>
    /*
     *  TODO
     *    * Analyze other mutating operators like ++, +=, ...
     *    * Analyze non-pure method access from non-meta conditional branches as mutating
     *    * Analyze while/do, for, foreach, exception handlers as conditional constructs
     *    * Implement foreach as a meta construct
     *    * Analyze out, ref parameters as mutations (in SemanticAnnotationMap too)
     *    * Solve the problem "i = i + 1" - this cannot be solved by naive recursion; a constraint solver may be needed.
     *
     *     IsMeta(symbol) should return three states: meta, nonmeta, or mixed. Only meta symbols cause expressions to be meta.
     */
    internal partial class TemplateAnnotator : CSharpSyntaxRewriter
    {
        private readonly SemanticAnnotationMap _semanticAnnotationMap;

        /// <summary>
        /// Scope of local variables.
        /// </summary>
        private readonly Dictionary<ILocalSymbol, SymbolDeclarationScope> _localScopes = new Dictionary<ILocalSymbol, SymbolDeclarationScope>();
        
        /// <summary>
        /// Specifies that the current node is guarded by a conditional statement where the condition is a runtime-only
        /// expression.
        /// </summary>
        private bool _isRuntimeConditionalBlock;

        private SymbolDeclarationScope _breakOrContinueScope;

        /// <summary>
        /// Specifies that the current expression is obliged to be compile-time-only.
        /// </summary>
        private bool _forceCompileTimeOnlyExpression;
        private MethodDeclarationSyntax? _currentMethod;
        private readonly SymbolClassifier _symbolScopeClassifier;
        
        /// <summary>
        /// Diagnostics produced by the current <see cref="TemplateAnnotator"/>.
        /// </summary>
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public TemplateAnnotator(CSharpCompilation compilation, SemanticAnnotationMap semanticAnnotationMap)
        {
            this._symbolScopeClassifier = new SymbolClassifier( compilation );
            this._semanticAnnotationMap = semanticAnnotationMap;
        }

        public int ChangeId => this._localScopes.Count;

  
        #region Computing scope
        
        /// <summary>
        /// Gets the scope of a symbol.
        /// </summary>
        /// <param name="symbol">A symbol.</param>
        /// <param name="nodeForDiagnostic">The <see cref="SyntaxNode"/> where diagnostics should be anchored.</param>
        /// <returns></returns>
        private SymbolDeclarationScope GetSymbolScope(ISymbol symbol, SyntaxNode nodeForDiagnostic )
        {
            if (symbol == null)
            {
                return SymbolDeclarationScope.Default;
            }
           
            // For local variables, we decide based on  _buildTimeLocals only. This collection is updated
            // at each iteration of the algorithm based on inferences from _requireMetaExpressionStack.
            if (symbol is ILocalSymbol local)
            {
                if (this._localScopes.TryGetValue(local, out var scope ))
                {
                    return scope;
                }
                else
                {
                    if (this._forceCompileTimeOnlyExpression)
                    {
                        this._localScopes.Add(local, SymbolDeclarationScope.CompileTimeOnly);
                        return SymbolDeclarationScope.CompileTimeOnly;
                    }
                    else
                    {
                        return SymbolDeclarationScope.Default;
                    }
                }
            }

            // For other symbols, we use the SymbolScopeClassifier.
            var scopeFromClassifier = this._symbolScopeClassifier.GetSymbolDeclarationScope(symbol);

            switch (scopeFromClassifier)
            {
                case SymbolDeclarationScope.CompileTimeOnly:
                    return SymbolDeclarationScope.CompileTimeOnly;
                
                case SymbolDeclarationScope.RunTimeOnly:
                    if (this._forceCompileTimeOnlyExpression )
                    {
                        // If the current expression must be compile-time by inference, emit a diagnostic. 
                        this.Diagnostics.Add(Diagnostic.Create("CA01", "Annotation",
                            "A compile-time expression is required.",
                            DiagnosticSeverity.Error,
                            DiagnosticSeverity.Error, true, 0, location: nodeForDiagnostic.GetLocation()));
                        return SymbolDeclarationScope.CompileTimeOnly;
                    }

                    return SymbolDeclarationScope.RunTimeOnly;
                    
                default:
                        return SymbolDeclarationScope.Default;
            }
        }

        /// <summary>
        /// Determines if a node is of <c>dynamic</c> type.
        /// </summary>
        /// <param name="originalNode"></param>
        /// <returns></returns>
        private bool IsDynamic(SyntaxNode originalNode)
        {
            var type = this._semanticAnnotationMap.GetType(originalNode);

            return type != null && type.Kind == SymbolKind.DynamicType;
        }


        /// <summary>
        /// Gets the scope of a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private SymbolDeclarationScope GetNodeScope(SyntaxNode node)
        {
            // If the node is dynamic, it is run-time only.
            if (this.IsDynamic(node))
            {
                return SymbolDeclarationScope.RunTimeOnly;
            }
            
            switch (node)
            {
                case IdentifierNameSyntax identifierName:
                    // If the node is an identifier, it means it should have a symbol,
                    // and the scope is given by the symbol.
                    
                    var symbol = this._semanticAnnotationMap.GetSymbol(identifierName);
                    if (symbol != null)
                    {
                        return this.GetSymbolScope(symbol, node);
                    }
                    else
                    {
                        return SymbolDeclarationScope.Default;
                    }

                default:
                    // Otherwise, the scope is given by the annotation given by the deeper 
                    // visitor or the previous algorithm iteration.
                    return node.GetScopeFromAnnotation();
            }
        }

 
        private SymbolDeclarationScope GetCombinedScope(params SymbolDeclarationScope[] scopes) => this.GetCombinedScope((IEnumerable<SymbolDeclarationScope>) scopes);

        private SymbolDeclarationScope GetCombinedScope(params SyntaxNode[] nodes) => this.GetCombinedScope((IEnumerable<SyntaxNode>) nodes);

        private SymbolDeclarationScope GetCombinedScope(IEnumerable<SyntaxNode> nodes) => this.GetCombinedScope(nodes.Select(this.GetNodeScope));
        
        /// <summary>
        /// Gives the <see cref="SymbolDeclarationScope"/> of a parent given the scope of its children.
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private SymbolDeclarationScope GetCombinedScope(IEnumerable<SymbolDeclarationScope> scopes)
        {
            var scopeCount = 0;
            
            var combinedScope = SymbolDeclarationScope.CompileTimeOnly;
            
            foreach (var scope in scopes)
            {
                scopeCount++;
                
                switch (scope)
                {
                    case SymbolDeclarationScope.RunTimeOnly:
                        // If there's a single child runtime-only scope, the parent is run-time only.
                        return SymbolDeclarationScope.RunTimeOnly;
                    
                    case SymbolDeclarationScope.Default:
                        // If one child has undetermined scope, we cannot take a decision.
                        combinedScope = SymbolDeclarationScope.Default;
                        break;
                    
                    case SymbolDeclarationScope.CompileTimeOnly:
                        // If all child scopes are build-time, the parent is build-time too.
                        break;
                }
            }

            if (scopeCount == 0)
            {
                // If there is no child, we cannot take a decision.
                return SymbolDeclarationScope.Default;
            }
            else
            {
                return combinedScope;
            }
        }
        
        #endregion

   
        /// <summary>
        /// Enters a branch of the syntax tree whose execution depends on a runtime-only condition.
        /// Local variables modified within such branch cannot be compile-time.
        /// </summary>
        /// <returns>A cookie to dispose at the end.</returns>
        private ConditionalBranchCookie EnterRuntimeConditionalBlock()
        {
            var cookie = new ConditionalBranchCookie(this, this._isRuntimeConditionalBlock);
            this._isRuntimeConditionalBlock = true;
            return cookie;
        }

        /// <summary>
        /// Enters an expression branch that must be compile-time because the parent must be
        /// compile-time.
        /// </summary>
        /// <returns>A cookie to dispose at the end.</returns>
        private ForceBuildTimeExpressionCookie EnterForceCompileTimeExpression()
        {
            var cookie = new ForceBuildTimeExpressionCookie(this, this._forceCompileTimeOnlyExpression);
            this._forceCompileTimeOnlyExpression = true;
            return cookie;
        }

        private BreakOrContinueScopeCookie EnterBreakOrContinueScope(SymbolDeclarationScope scope)
        {
            var cookie = new BreakOrContinueScopeCookie( this, this._breakOrContinueScope );
            this._breakOrContinueScope = scope;
            return cookie;
        }


        /// <summary>
        /// Default visitor.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            if (node == null)
            {
                return null;
            }
            
            // Adds annotations to the children node.
            var transformedNode = base.Visit(node);
            
            if  (this._forceCompileTimeOnlyExpression)
            {
                // The current expression is obliged to be compile-time-only by inference.
                // Emit an error if the type of the expression is inferred to be runtime-only.
                
                var expressionType = this._semanticAnnotationMap.GetType(transformedNode);
                
                if (expressionType != null && expressionType.Kind == SymbolKind.DynamicType)
                {
                    this.Diagnostics.Add(Diagnostic.Create("CA02", "Annotation",
                        $"The expression {node} cannot be used in a build-time expression.",
                        DiagnosticSeverity.Error,
                        DiagnosticSeverity.Error, true, 0, location: Location.Create(node.SyntaxTree, node.Span)));
                }

                return transformedNode.AddScopeAnnotation(SymbolDeclarationScope.CompileTimeOnly);
            }
            else if (transformedNode.HasScopeAnnotation())
            {
                // If the transformed node has already an annotation, it means it has already been classified by
                // a previous run of the algorithm, and there is no need to classify it again.
                return transformedNode;
            }
            else if (node is ExpressionSyntax)
            {
                // Here is the default implementation for expressions. The scope of the parent is the combined scope of the children.
                
                var childScopes = transformedNode.ChildNodes().Where(c => c is ExpressionSyntax);

                return transformedNode.AddScopeAnnotation(this.GetCombinedScope(childScopes));
            }
            else
            {
                return transformedNode;
            }
        }
        
        public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            // Literals are always compile-time (not really compile-time only but it does not matter).
            return base.VisitLiteralExpression(node)!.AddScopeAnnotation(SymbolDeclarationScope.CompileTimeOnly);
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            var identifierNameSyntax = (IdentifierNameSyntax) base.VisitIdentifierName(node)!;
            var symbol = this._semanticAnnotationMap.GetSymbol(node)!;
            
            return identifierNameSyntax.AddScopeAnnotation( this.GetSymbolScope(symbol, node));
        }

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            
            var transformedName = (SimpleNameSyntax) this.Visit(node.Name)!;

            if (this.GetNodeScope(transformedName) == SymbolDeclarationScope.CompileTimeOnly)
            {
                // If the member is compile-time (because of rules on the symbol), the expression on the left MUST be compile-time.

                using (this.EnterForceCompileTimeExpression())
                {
                    var transformedExpression = (ExpressionSyntax) this.Visit(node.Expression)!;
                    return node.Update(transformedExpression, node.OperatorToken, transformedName).AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly);
                }
                
            }
            else
            {
                var transformedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;

                if ( this.GetNodeScope( transformedExpression ) == SymbolDeclarationScope.RunTimeOnly )
                {
                    // If the left part is runtime-only, then the right part is runtime-only too.
                    return node.Update( transformedExpression, node.OperatorToken, transformedName ).AddScopeAnnotation( SymbolDeclarationScope.RunTimeOnly );
                }
                else
                {
                    // The scope of the expression parent is copied from the child expression.

                    var transformedNode = (MemberAccessExpressionSyntax) base.VisitMemberAccessExpression( node )!;
                    return transformedNode.AddScopeAnnotation( this.GetNodeScope( transformedNode.Expression ) );
                }
            }
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var transformedExpression = (ExpressionSyntax) this.Visit(node.Expression)!;

            if (this.GetNodeScope(transformedExpression) == SymbolDeclarationScope.CompileTimeOnly)
            {
                // If the expression on the left meta is compile-time (because of rules on the symbol),
                // then all arguments MUST be compile-time.

                using (this.EnterForceCompileTimeExpression())
                {

                    var updatedInvocation = node.Update(transformedExpression,
                        (ArgumentListSyntax) this.VisitArgumentList(node.ArgumentList)!);

                    return updatedInvocation.AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly);
                }
            }
            else
            {
                // If the expression on the left of the parenthesis is not compile-time,
                // we cannot take a decision on the parent expression.
                
                return node.Update(transformedExpression,
                    (ArgumentListSyntax) this.VisitArgumentList(node.ArgumentList)!);
            }
        }

        public override SyntaxNode? VisitArgument(ArgumentSyntax node)
        {
            var argument = (ArgumentSyntax) base.VisitArgument(node)!;

            if (argument.RefKindKeyword.IsMissing)
            {
                return argument.AddScopeAnnotation( this.GetNodeScope(argument.Expression));
            }
            else
            {
                // TODO: We're not processing ref/out arguments properly. These are possibly
                // local variable assignments.
                return argument;
            }
        }

        public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
        {
            var annotatedCondition = (ExpressionSyntax) this.Visit(node.Condition);
            var conditionScope = this.GetNodeScope(annotatedCondition);

            if (conditionScope == SymbolDeclarationScope.CompileTimeOnly)
            {
                // We have an if statement where the condition is a compile-time expression. Add annotations
                // to the if and else statements but not to the blocks themselves.
         
                var annotatedStatement = (StatementSyntax) this.Visit(node.Statement)!;
                var annotatedElse = node.Else != null
                    ? ElseClause(
                        node.Else.ElseKeyword,
                        (StatementSyntax) this.Visit(node.Else.Statement)!
                    ).AddScopeAnnotation(SymbolDeclarationScope.CompileTimeOnly).WithTriviaFrom(node.Else)
                    : null;

                return node.Update(node.IfKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken,
                    annotatedStatement, annotatedElse).AddScopeAnnotation(SymbolDeclarationScope.CompileTimeOnly);
            }
            else
            {
                // We have an if statement where the condition is a runtime expression. Any variable assignment
                // within this statement should make the variable as runtime-only, so we're calling EnterRuntimeConditionalBlock.
                
                // TODO: It's not clear here whether we have a run-time expression or an expression that
                // has not been classified yet as compile-time. We may find a counter-example to this algorithm that
                // would counter-proof this code here. However, it may need that our whole algorithm is flawed,
                // so we may want to live with that behavior anyway. Perhaps the same remark is true for `foreach`.
                
                using (this.EnterRuntimeConditionalBlock())
                {
                    var annotatedStatement = (StatementSyntax) this.Visit(node.Statement)!;
                    var annotatedElse = (ElseClauseSyntax) this.Visit(node.Else)!;

                    var result = node.Update(node.IfKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken,
                        annotatedStatement, annotatedElse);

                    return result;
                }
            }
        }

        public override SyntaxNode? VisitBreakStatement( BreakStatementSyntax node )
        {
            return base.VisitBreakStatement( node )!.AddScopeAnnotation(this._breakOrContinueScope);
        }

        public override SyntaxNode? VisitContinueStatement( ContinueStatementSyntax node )
        {
            return base.VisitContinueStatement( node )!.AddScopeAnnotation( this._breakOrContinueScope );
        }

        public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node)
        {
            var local = (ILocalSymbol) this._semanticAnnotationMap.GetDeclaredSymbol(node)!;

            // TODO: Verify the logic here. At least, we should validate that the foreach expression is
            // compile-time. 

            bool isBuildTimeLocalVariable = this._localScopes.TryGetValue( local, out var localScope ) && localScope == SymbolDeclarationScope.CompileTimeOnly;

            ExpressionSyntax? annotatedExpression;

            if ( isBuildTimeLocalVariable )
            {
                using ( this.EnterForceCompileTimeExpression() )
                {
                    annotatedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;
                }
            }
            else
            {
                annotatedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;
            }


            bool isBuildTimeExpression = this.GetNodeScope( annotatedExpression ) == SymbolDeclarationScope.CompileTimeOnly;
            
            if ( isBuildTimeLocalVariable || isBuildTimeExpression )
            {
                // This is a build-time loop.

                if ( !isBuildTimeLocalVariable )
                {
                    this._localScopes.Add( local, SymbolDeclarationScope.CompileTimeOnly );
                }

                StatementSyntax annotatedStatement;
                using ( this.EnterBreakOrContinueScope( SymbolDeclarationScope.CompileTimeOnly ) )
                {
                    annotatedStatement = (StatementSyntax) this.Visit( node.Statement )!;
                }

                var transformedNode = 
                    ForEachStatement(
                        default,
                        node.ForEachKeyword,
                        node.OpenParenToken,
                        node.Type,
                        node.Identifier,
                        node.InKeyword,
                        annotatedExpression,
                        node.CloseParenToken,
                        annotatedStatement)
                        .AddScopeAnnotation(localScope)
                        .WithSymbolAnnotationsFrom(node);

           
                return transformedNode;
            }
            else
            {
                // Run-time or default loop, we don't know.

                using (this.EnterRuntimeConditionalBlock())
                {
                    StatementSyntax annotatedStatement;
                    using ( this.EnterBreakOrContinueScope( SymbolDeclarationScope.Default ) )
                    {
                        annotatedStatement = (StatementSyntax) this.Visit( node.Statement )!;
                    }


                    return ForEachStatement(
                        default,
                        node.ForEachKeyword,
                        node.OpenParenToken,
                        node.Type,
                        node.Identifier,
                        node.InKeyword,
                        annotatedExpression,
                        node.CloseParenToken,
                        annotatedStatement).WithSymbolAnnotationsFrom(node);

                }
            }
        }

        private SymbolDeclarationScope GetAssignmentScope(SyntaxNode node)
        {
            switch (node)
            {
                case VariableDeclaratorSyntax declarator when declarator.Initializer == null:
                    return SymbolDeclarationScope.Default;
                
                case VariableDeclaratorSyntax declarator when declarator.Initializer != null:
                    return this.GetNodeScope(declarator.Initializer.Value);

                case AssignmentExpressionSyntax assignment:
                    // Assignments must be classified by the visitor to take into account _isNonMetaConditionalBranch.
                    var assignmentScope = this.GetNodeScope( assignment );
                    if ( assignmentScope == SymbolDeclarationScope.Default )
                    {
                        return this.GetNodeScope( assignment.Right );
                    }
                    else
                    {
                        return assignmentScope;
                    }

                default:
                    throw new AssertionFailedException();
            }
        }

        public override SyntaxNode? VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var transformedNode = (VariableDeclaratorSyntax) base.VisitVariableDeclarator(node)!;

            var local = (ILocalSymbol) this._semanticAnnotationMap.GetDeclaredSymbol(node)!;

            var localScope = this.GetSymbolScope(local, node); 

            if ( this._forceCompileTimeOnlyExpression )
            {
                switch ( localScope )
                {
                    case SymbolDeclarationScope.CompileTimeOnly:
                        // Nothing to do.
                        break;

                    case SymbolDeclarationScope.RunTimeOnly:
                        throw new AssertionFailedException();

                    case SymbolDeclarationScope.Default:
                        this._localScopes.Add( local, SymbolDeclarationScope.CompileTimeOnly );
                        break;
                }
                return transformedNode.AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
            }
            else if ( localScope != SymbolDeclarationScope.Default )
            {
                return transformedNode.AddScopeAnnotation( localScope );
            }
            else
            {
                // If a variable is always assigned to a meta expression, it is meta itself.
                // The next line will not return anything in the first run because it refers to the unmodified tree.
                var assignments = this._semanticAnnotationMap.GetAssignments(local, this._currentMethod!);

                var combinedScope = this.GetCombinedScope(assignments.Select(this.GetAssignmentScope));
                
                
                if (combinedScope != SymbolDeclarationScope.Default)
                {
                    this._localScopes.Add(local, combinedScope);
                    return transformedNode.AddScopeAnnotation(combinedScope);
                }

                return transformedNode;
            }
        }

        public override SyntaxNode? VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var transformedType = this.Visit( node.Type )!;

            if ( this.GetNodeScope( transformedType ) == SymbolDeclarationScope.CompileTimeOnly )
            {
                using ( this.EnterForceCompileTimeExpression() )
                {
                    var transformedVariableDeclaration = (VariableDeclarationSyntax) base.VisitVariableDeclaration( node )!;
                    return transformedVariableDeclaration.AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
                }

            }
            else
            {
                var transformedVariableDeclaration = (VariableDeclarationSyntax) base.VisitVariableDeclaration( node )!;

                var variableScopes = transformedVariableDeclaration.Variables.Select( v => v.GetScopeFromAnnotation() ).Distinct();

                if ( variableScopes.Count() == 1 )
                {
                    return transformedVariableDeclaration.AddScopeAnnotation( variableScopes.Single() );
                }
                else
                {
                    // TODO: We may have to write this diagnostic in the last iteration only.

                    this.Diagnostics.Add( Diagnostic.Create( "CA01", "Annotation",
                        "Split build-time and run-time variables into several declarations.",
                        DiagnosticSeverity.Error,
                        DiagnosticSeverity.Error, true, 0, location: node.GetLocation() ) );
                    return transformedVariableDeclaration;
                }
            }
        }

        public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var transformedNode = (LocalDeclarationStatementSyntax) base.VisitLocalDeclarationStatement(node)!;
            
            return transformedNode.AddScopeAnnotation(this.GetNodeScope(transformedNode.Declaration));
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            this._currentMethod = node;
            try
            {
                return base.VisitMethodDeclaration(node);
            }
            finally
            {
                this._currentMethod = null;
            }
        }

        public override SyntaxNode? VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var transformedNode = (AssignmentExpressionSyntax) base.VisitAssignmentExpression(node)!;

            if (this._isRuntimeConditionalBlock)
            {
                return transformedNode.AddScopeAnnotation(SymbolDeclarationScope.RunTimeOnly);
            }
            else
            {
                var scope = this.GetCombinedScope(transformedNode.Left, transformedNode.Right);

                return transformedNode.AddScopeAnnotation(scope);

            }
        }

        public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var transformedNode = (ExpressionStatementSyntax) base.VisitExpressionStatement(node)!;

            return transformedNode.WithScopeAnnotationFrom(node.Expression).WithScopeAnnotationFrom(node);
        }

        #region Unsupported syntax

        public override SyntaxNode? VisitForStatement( ForStatementSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "for statement" );
            this.Diagnostics.Add( diagnostic );
            
            return base.VisitForStatement( node );
        }

        public override SyntaxNode? VisitWhileStatement( WhileStatementSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "while statement" );
            this.Diagnostics.Add( diagnostic );

            return base.VisitWhileStatement( node );
        }

        public override SyntaxNode? VisitDoStatement( DoStatementSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "do statement" );
            this.Diagnostics.Add( diagnostic );
            
            return base.VisitDoStatement( node );
        }

        public override SyntaxNode? VisitGotoStatement( GotoStatementSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "goto statement" );
            this.Diagnostics.Add( diagnostic );

            return base.VisitGotoStatement( node );
        }

        public override SyntaxNode? VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "local function" );
            this.Diagnostics.Add( diagnostic );
            
            return base.VisitLocalFunctionStatement( node );
        }

        public override SyntaxNode? VisitAnonymousMethodExpression( AnonymousMethodExpressionSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "anonymous method" );
            this.Diagnostics.Add( diagnostic );
            
            return base.VisitAnonymousMethodExpression( node );
        }

        public override SyntaxNode? VisitParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "lambda expression" );
            this.Diagnostics.Add( diagnostic );

            return base.VisitParenthesizedLambdaExpression( node );
        }

        public override SyntaxNode? VisitSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "lambda expression" );
            this.Diagnostics.Add( diagnostic );

            return base.VisitSimpleLambdaExpression( node );
        }

        public override SyntaxNode? VisitSwitchStatement( SwitchStatementSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "switch statement" );
            this.Diagnostics.Add( diagnostic );
            
            return base.VisitSwitchStatement( node );
        }

        public override SyntaxNode? VisitQueryExpression( QueryExpressionSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "query expression" );
            this.Diagnostics.Add( diagnostic );
            
            return base.VisitQueryExpression( node );
        }

        public override SyntaxNode? VisitLockStatement( LockStatementSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "lock statement" );
            this.Diagnostics.Add( diagnostic );
            
            return base.VisitLockStatement( node );
        }

        public override SyntaxNode? VisitAwaitExpression( AwaitExpressionSyntax node )
        {
            var diagnostic = Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), "await expression" );
            this.Diagnostics.Add( diagnostic );

            return base.VisitAwaitExpression( node );
        }

        #endregion
    }
}