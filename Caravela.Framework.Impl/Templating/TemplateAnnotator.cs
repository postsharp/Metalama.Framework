// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    // ReSharper disable TailRecursiveCall

    /// <summary>
    /// A <see cref="CSharpSyntaxRewriter"/> that adds annotation that distinguish compile-time from
    /// run-time syntax nodes. The input should be a syntax tree annotated with a <see cref="SemanticAnnotationMap"/>.
    /// </summary>
    internal partial class TemplateAnnotator : CSharpSyntaxRewriter
    {
        private readonly SemanticAnnotationMap _semanticAnnotationMap;

        /// <summary>
        /// Scope of local variables.
        /// </summary>
        private readonly Dictionary<ILocalSymbol, SymbolDeclarationScope> _localScopes = new Dictionary<ILocalSymbol, SymbolDeclarationScope>();

        private readonly SymbolClassifier _symbolScopeClassifier;

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

        /// <summary>
        /// Gets the list of diagnostics produced by the current <see cref="TemplateAnnotator"/>.
        /// </summary>
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public TemplateAnnotator( CSharpCompilation compilation, SemanticAnnotationMap semanticAnnotationMap )
        {
            this._symbolScopeClassifier = new SymbolClassifier( compilation );
            this._semanticAnnotationMap = semanticAnnotationMap;
        }

        public int ChangeId => this._localScopes.Count;

        private bool TrySetLocalVariableScope( ILocalSymbol local, SymbolDeclarationScope scope )
        {
            if ( this._localScopes.TryGetValue( local, out var oldScope ) )
            {
                if ( oldScope != scope )
                {
                    this.Diagnostics.Add( Diagnostic.Create(
                        TemplatingDiagnosticDescriptors.LocalVariableAmbiguousCoercion,
                        local.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation(),
                        local.Name ) );

                    return false;
                }
                else
                {
                    // Nothing to do.
                    return true;
                }
            }

            this._localScopes.Add( local, scope );
            return true;
        }

        /// <summary>
        /// Gets the scope of a symbol.
        /// </summary>
        /// <param name="symbol">A symbol.</param>
        /// <param name="nodeForDiagnostic">The <see cref="SyntaxNode"/> where diagnostics should be anchored.</param>
        /// <returns></returns>
        private SymbolDeclarationScope GetSymbolScope( ISymbol? symbol, SyntaxNode nodeForDiagnostic )
        {
            if ( symbol == null )
            {
                return SymbolDeclarationScope.Default;
            }

            // For local variables, we decide based on  _buildTimeLocals only. This collection is updated
            // at each iteration of the algorithm based on inferences from _requireMetaExpressionStack.
            if ( symbol is ILocalSymbol local )
            {
                if ( this._localScopes.TryGetValue( local, out var scope ) )
                {
                    return scope;
                }
                else
                {
                    // TODO: remove this coercion
                    if ( this._forceCompileTimeOnlyExpression )
                    {
                        this.TrySetLocalVariableScope( local, SymbolDeclarationScope.CompileTimeOnly );
                        return SymbolDeclarationScope.CompileTimeOnly;
                    }
                    else
                    {
                        return SymbolDeclarationScope.Default;
                    }
                }
            }

            // For other symbols, we use the SymbolScopeClassifier.
            var scopeFromClassifier = this._symbolScopeClassifier.GetSymbolDeclarationScope( symbol );

            switch ( scopeFromClassifier )
            {
                case SymbolDeclarationScope.CompileTimeOnly:
                    return SymbolDeclarationScope.CompileTimeOnly;

                case SymbolDeclarationScope.RunTimeOnly:
                    if ( this._forceCompileTimeOnlyExpression )
                    {
                        // If the current expression must be compile-time by inference, emit a diagnostic.
                        this.Diagnostics.Add( Diagnostic.Create(
                            "CA01",
                            "Annotation",
                            "A compile-time expression is required.",
                            DiagnosticSeverity.Error,
                            DiagnosticSeverity.Error,
                            true,
                            0,
                            location: nodeForDiagnostic.GetLocation() ) );
                        return SymbolDeclarationScope.CompileTimeOnly;
                    }

                    return SymbolDeclarationScope.RunTimeOnly;

                case SymbolDeclarationScope.Template:
                    return SymbolDeclarationScope.Template;

                default:
                    return SymbolDeclarationScope.Default;
            }
        }

        /// <summary>
        /// Determines if a node is of <c>dynamic</c> type.
        /// </summary>
        /// <param name="originalNode"></param>
        /// <returns></returns>
        private bool IsDynamic( SyntaxNode originalNode ) =>
            this._semanticAnnotationMap.GetType( originalNode ) is IDynamicTypeSymbol;

        /// <summary>
        /// Gets the scope of a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private SymbolDeclarationScope GetNodeScope( SyntaxNode? node )
        {
            if ( node == null )
            {
                return SymbolDeclarationScope.Default;
            }

            // If the node is dynamic, it is run-time only.
            if ( this.IsDynamic( node ) )
            {
                return SymbolDeclarationScope.RunTimeOnly;
            }

            switch ( node )
            {
                case IdentifierNameSyntax identifierName:
                    // If the node is an identifier, it means it should have a symbol,
                    // and the scope is given by the symbol.

                    var symbol = this._semanticAnnotationMap.GetSymbol( identifierName );
                    if ( symbol != null )
                    {
                        return this.GetSymbolScope( symbol, node );
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

        // ReSharper disable once UnusedMember.Local
        private SymbolDeclarationScope GetCombinedScope( params SymbolDeclarationScope[] scopes ) => this.GetCombinedScope( (IEnumerable<SymbolDeclarationScope>) scopes );

        private SymbolDeclarationScope GetCombinedScope( params SyntaxNode?[] nodes ) => this.GetCombinedScope( (IEnumerable<SyntaxNode?>) nodes );

        private SymbolDeclarationScope GetCombinedScope( IEnumerable<SyntaxNode?> nodes ) => this.GetCombinedScope( nodes.Select( this.GetNodeScope ) );

        /// <summary>
        /// Gives the <see cref="SymbolDeclarationScope"/> of a parent given the scope of its children.
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private SymbolDeclarationScope GetCombinedScope( IEnumerable<SymbolDeclarationScope> scopes )
        {
            var scopeCount = 0;

            var combinedScope = SymbolDeclarationScope.CompileTimeOnly;

            foreach ( var scope in scopes )
            {
                scopeCount++;

                switch ( scope )
                {
                    case SymbolDeclarationScope.RunTimeOnly:
                        // If there's a single child runtime-only scope, the parent is run-time only.
                        return SymbolDeclarationScope.RunTimeOnly;

                    case SymbolDeclarationScope.Default:
                        // If one child has undetermined scope, we cannot take a decision.
                        combinedScope = SymbolDeclarationScope.Default;
                        break;

                    case SymbolDeclarationScope.CompileTimeOnly:
                        // If all child scopes are compile-time, the parent is compile-time too.
                        break;
                }
            }

            if ( scopeCount == 0 )
            {
                // If there is no child, we cannot take a decision.
                return SymbolDeclarationScope.Default;
            }
            else
            {
                return combinedScope;
            }
        }

        /// <summary>
        /// Enters a branch of the syntax tree whose execution depends on a runtime-only condition.
        /// Local variables modified within such branch cannot be compile-time.
        /// </summary>
        /// <returns>A cookie to dispose at the end.</returns>
        private ConditionalBranchCookie EnterRuntimeConditionalBlock()
        {
            var cookie = new ConditionalBranchCookie( this, this._isRuntimeConditionalBlock );
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
            var cookie = new ForceBuildTimeExpressionCookie( this, this._forceCompileTimeOnlyExpression );
            this._forceCompileTimeOnlyExpression = true;
            return cookie;
        }

        private BreakOrContinueScopeCookie EnterBreakOrContinueScope( SymbolDeclarationScope scope )
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
        public override SyntaxNode? Visit( SyntaxNode? node )
        {
            if ( node == null )
            {
                return null;
            }

            // Adds annotations to the children node.
            var transformedNode = base.Visit( node );

            if ( this._forceCompileTimeOnlyExpression )
            {
                if ( transformedNode.GetScopeFromAnnotation() == SymbolDeclarationScope.RunTimeOnly ||
                     this.IsDynamic( transformedNode ) )
                {
                    // The current expression is obliged to be compile-time-only by inference.
                    // Emit an error if the type of the expression is inferred to be runtime-only.
                    this.Diagnostics.Add( Diagnostic.Create(
                        "CA02",
                        "Annotation",
                        $"The expression {node} cannot be used in a build-time expression.",
                        DiagnosticSeverity.Error,
                        DiagnosticSeverity.Error,
                        true,
                        0,
                        location: Location.Create( node.SyntaxTree, node.Span ) ) );

                    return transformedNode;
                }

                return transformedNode.AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
            }
            else if ( transformedNode.HasScopeAnnotation() )
            {
                // If the transformed node has already an annotation, it means it has already been classified by
                // a previous run of the algorithm, and there is no need to classify it again.
                return transformedNode;
            }
            else if ( node is ExpressionSyntax )
            {
                // Here is the default implementation for expressions. The scope of the parent is the combined scope of the children.
                var childScopes = transformedNode.ChildNodes().Where( c => c is ExpressionSyntax );
                return transformedNode.AddScopeAnnotation( this.GetCombinedScope( childScopes ) );
            }
            else
            {
                return transformedNode;
            }
        }

        public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            var typeScope = this.GetSymbolScope( this._semanticAnnotationMap.GetDeclaredSymbol( node ), node );

            if ( typeScope == SymbolDeclarationScope.CompileTimeOnly )
            {
                return base.VisitClassDeclaration( node )?.AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
            }
            else
            {
                // This is not a build-time class so there's no need to analyze it.
                return node;
            }
        }

        public override SyntaxNode? VisitLiteralExpression( LiteralExpressionSyntax node )
        {
            // Literals are always compile-time (not really compile-time only but it does not matter), unless they are converted to dynamic.
            var scope = this.IsDynamic( node ) ? SymbolDeclarationScope.RunTimeOnly : SymbolDeclarationScope.CompileTimeOnly;
            return base.VisitLiteralExpression( node )!.AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
        {
            var identifierNameSyntax = (IdentifierNameSyntax) base.VisitIdentifierName( node )!;
            var symbol = this._semanticAnnotationMap.GetSymbol( node );

            if ( symbol != null )
            {
                var scope = this.GetSymbolScope( symbol, node );
                var annotatedNode = identifierNameSyntax.AddScopeAnnotation( scope );

                if ( (symbol is ILocalSymbol localSymbol &&
                      scope == SymbolDeclarationScope.CompileTimeOnly) ||
                     symbol.GetAttributes().Any( a =>
                         a.AttributeClass != null && a.AttributeClass.AnyBaseType( t => t.Name == nameof( TemplateKeywordAttribute ) ) ) )
                {
                    annotatedNode = annotatedNode.AddColoringAnnotation( TextSpanClassification.TemplateKeyword );
                }
                else if ( scope == SymbolDeclarationScope.RunTimeOnly &&
                          (symbol.Kind == SymbolKind.Property || symbol.Kind == SymbolKind.Method) )
                {
                    // Annotate dynamic members differently for syntax coloring.
                    annotatedNode = annotatedNode.AddColoringAnnotation( TextSpanClassification.Dynamic );
                }

                return annotatedNode;
            }
            else
            {
                return identifierNameSyntax;
            }
        }

        public override SyntaxNode? VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
        {
            var transformedName = (SimpleNameSyntax) this.Visit( node.Name )!;

            if ( this.GetNodeScope( transformedName ) == SymbolDeclarationScope.CompileTimeOnly )
            {
                // If the member is compile-time (because of rules on the symbol), the expression on the left MUST be compile-time.

                using ( this.EnterForceCompileTimeExpression() )
                {
                    var transformedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;
                    return node.Update( transformedExpression, node.OperatorToken, transformedName ).AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
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

        public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
        {
            var transformedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;

            InvocationExpressionSyntax updatedInvocation;

            if ( this.GetNodeScope( transformedExpression ) == SymbolDeclarationScope.CompileTimeOnly )
            {
                // If the expression on the left meta is compile-time (because of rules on the symbol),
                // then arguments MUST be compile-time, unless they are dynamic.

                var transformedArguments = new List<ArgumentSyntax>( node.ArgumentList.Arguments.Count );

                foreach ( var argument in node.ArgumentList.Arguments )
                {
                    var parameterType = this._semanticAnnotationMap.GetParameterSymbol( argument )?.Type;

                    ArgumentSyntax transformedArgument;

                    // dynamic or dynamic[]
                    if ( parameterType is IDynamicTypeSymbol or IArrayTypeSymbol { ElementType: IDynamicTypeSymbol } )
                    {
                        transformedArgument = (ArgumentSyntax) this.VisitArgument( argument )!;
                    }
                    else
                    {
                        using ( this.EnterForceCompileTimeExpression() )
                        {
                            transformedArgument = (ArgumentSyntax) this.VisitArgument( argument )!;
                        }
                    }

                    transformedArgument = transformedArgument.WithTriviaFrom( argument );
                    transformedArguments.Add( transformedArgument );
                }

                updatedInvocation = node.Update(
                    transformedExpression,
                    ArgumentList(
                        node.ArgumentList.OpenParenToken,
                        SeparatedList( transformedArguments, node.ArgumentList.Arguments.GetSeparators() ),
                        node.ArgumentList.CloseParenToken ) );

                updatedInvocation = updatedInvocation.AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
            }
            else
            {
                // If the expression on the left of the parenthesis is not compile-time,
                // we cannot take a decision on the parent expression.

                var transformedArgumentList = (ArgumentListSyntax) this.VisitArgumentList( node.ArgumentList )!;
                transformedArgumentList = transformedArgumentList.WithOpenParenToken( node.ArgumentList.OpenParenToken );
                transformedArgumentList = transformedArgumentList.WithCloseParenToken( node.ArgumentList.CloseParenToken );
                updatedInvocation = node.Update( transformedExpression, transformedArgumentList );
            }

            updatedInvocation = updatedInvocation.WithTriviaFrom( node );

            return updatedInvocation;
        }

        public override SyntaxNode? VisitArgument( ArgumentSyntax node )
        {
            var argument = (ArgumentSyntax) base.VisitArgument( node )!;

            if ( argument.RefKindKeyword.IsMissing )
            {
                return argument.AddScopeAnnotation( this.GetNodeScope( argument.Expression ) );
            }
            else
            {
                // TODO: We're not processing ref/out arguments properly. These are possibly
                // local variable assignments.
                return argument;
            }
        }

        public override SyntaxNode? VisitIfStatement( IfStatementSyntax node )
        {
            var annotatedCondition = (ExpressionSyntax) this.Visit( node.Condition )!;
            var conditionScope = this.GetNodeScope( annotatedCondition );

            if ( conditionScope == SymbolDeclarationScope.CompileTimeOnly )
            {
                // We have an if statement where the condition is a compile-time expression. Add annotations
                // to the if and else statements but not to the blocks themselves.

                var annotatedStatement = (StatementSyntax) this.Visit( node.Statement )!;

                var annotatedElse = node.Else != null
                    ? ElseClause(
                        node.Else.ElseKeyword,
                        (StatementSyntax) this.Visit( node.Else.Statement )! ).AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly ).WithTriviaFrom( node.Else )
                    : null;

                return node.Update( node.AttributeLists, node.IfKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken, annotatedStatement, annotatedElse )
                    .AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
            }
            else
            {
                // We have an if statement where the condition is a runtime expression. Any variable assignment
                // within this statement should make the variable as runtime-only, so we're calling EnterRuntimeConditionalBlock.

                // TODO: It's not clear here whether we have a run-time expression or an expression that
                // has not been classified yet as compile-time. We may find a counter-example to this algorithm that
                // would counter-proof this code here. However, it may need that our whole algorithm is flawed,
                // so we may want to live with that behavior anyway. Perhaps the same remark is true for `foreach`.

                using ( this.EnterRuntimeConditionalBlock() )
                {
                    var annotatedStatement = (StatementSyntax) this.Visit( node.Statement )!;
                    var annotatedElse = (ElseClauseSyntax) this.Visit( node.Else )!;

                    var result = node.Update( node.IfKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken, annotatedStatement, annotatedElse );

                    return result;
                }
            }
        }

        public override SyntaxNode? VisitBreakStatement( BreakStatementSyntax node )
        {
            return base.VisitBreakStatement( node )!.AddScopeAnnotation( this._breakOrContinueScope );
        }

        public override SyntaxNode? VisitContinueStatement( ContinueStatementSyntax node )
        {
            return base.VisitContinueStatement( node )!.AddScopeAnnotation( this._breakOrContinueScope );
        }

        public override SyntaxNode? VisitForEachStatement( ForEachStatementSyntax node )
        {
            var callsProceed = node.HasCallsProceedAnnotation();

            var local = (ILocalSymbol) this._semanticAnnotationMap.GetDeclaredSymbol( node )!;

            if ( callsProceed )
            {
                // If the loop calls proceed, we force it to be run-time.
                this.TrySetLocalVariableScope( local, SymbolDeclarationScope.RunTimeOnly );
            }

            // TODO: Verify the logic here. At least, we should validate that the foreach expression is
            // compile-time.

            var isBuildTimeLocalVariable = this._localScopes.TryGetValue( local, out var localScope ) && localScope == SymbolDeclarationScope.CompileTimeOnly;

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

            var isBuildTimeExpression = this.GetNodeScope( annotatedExpression ) == SymbolDeclarationScope.CompileTimeOnly;

            if ( (isBuildTimeLocalVariable || isBuildTimeExpression) && !callsProceed )
            {
                // This is a build-time loop.

                if ( !isBuildTimeLocalVariable )
                {
                    this.TrySetLocalVariableScope( local, SymbolDeclarationScope.CompileTimeOnly );
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
                            node.Identifier.AddColoringAnnotation( TextSpanClassification.CompileTimeVariable ),
                            node.InKeyword,
                            annotatedExpression,
                            node.CloseParenToken,
                            annotatedStatement )
                        .AddScopeAnnotation( localScope )
                        .WithSymbolAnnotationsFrom( node );

                return transformedNode;
            }
            else
            {
                // Run-time or default loop, we don't know.

                using ( this.EnterRuntimeConditionalBlock() )
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
                            annotatedStatement )
                        .WithSymbolAnnotationsFrom( node )
                        .WithCallsProceedAnnotationFrom( node );
                }
            }
        }

        public override SyntaxNode? VisitVariableDeclarator( VariableDeclaratorSyntax node )
        {
            var transformedNode = (VariableDeclaratorSyntax) base.VisitVariableDeclarator( node )!;

            var symbol = this._semanticAnnotationMap.GetDeclaredSymbol( node )!;

            if ( symbol is not ILocalSymbol local )
            {
                // it's a field, or a field-like event
                return node;
            }

            var localScope = this.GetSymbolScope( local, node );

            if ( localScope == SymbolDeclarationScope.Default )
            {
                SymbolDeclarationScope initializerScope;

                if ( node.Initializer != null )
                {
                    if ( node.Initializer.Value is LiteralExpressionSyntax )
                    {
                        // Variables initialized with literal expression have runtime scope.
                        initializerScope = SymbolDeclarationScope.RunTimeOnly;
                    }
                    else
                    {
                        // Inference the variable scope from the initializer.
                        var transformedInitializerValue = this.Visit( node.Initializer.Value );
                        if ( transformedInitializerValue != null )
                        {
                            initializerScope = this.GetNodeScope( transformedInitializerValue );
                            transformedNode = transformedNode.WithInitializer( node.Initializer.WithValue( (ExpressionSyntax) transformedInitializerValue ) );
                        }
                        else
                        {
                            // Variables without initializer have runtime scope.
                            initializerScope = SymbolDeclarationScope.RunTimeOnly;
                        }
                    }
                }
                else
                {
                    // Variables without initializer have runtime scope.
                    initializerScope = SymbolDeclarationScope.RunTimeOnly;
                }

                if ( initializerScope != SymbolDeclarationScope.Default )
                {
                    if ( this.TrySetLocalVariableScope( local, initializerScope ) )
                    {
                        localScope = initializerScope;
                    }
                }
            }

            var forcedScope = this._forceCompileTimeOnlyExpression ? SymbolDeclarationScope.CompileTimeOnly : SymbolDeclarationScope.Default;
            if ( forcedScope != SymbolDeclarationScope.Default )
            {
                if ( this.TrySetLocalVariableScope( local, forcedScope ) )
                {
                    localScope = forcedScope;
                }
            }

            if ( localScope == SymbolDeclarationScope.Default )
            {
                // The default scope for variable declaration is RunTimeOnly.
                if ( this.TrySetLocalVariableScope( local, SymbolDeclarationScope.RunTimeOnly ) )
                {
                    localScope = SymbolDeclarationScope.RunTimeOnly;
                }
                else
                {
                    throw new AssertionFailedException();
                }
            }
            else
            {
                transformedNode =
                    transformedNode.WithIdentifier(
                        transformedNode.Identifier.AddColoringAnnotation( TextSpanClassification.CompileTimeVariable ) );
            }

            return transformedNode.AddScopeAnnotation( localScope );
        }

        public override SyntaxNode? VisitVariableDeclaration( VariableDeclarationSyntax node )
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

                // TODO: We are no longer relying on other assignments than initialization, so this code should be removed.
                var variableScopes = transformedVariableDeclaration.Variables.Select( v => v.GetScopeFromAnnotation() ).Distinct().ToList();

                if ( variableScopes.Count() == 1 )
                {
                    return transformedVariableDeclaration.AddScopeAnnotation( variableScopes.Single() );
                }
                else
                {
                    // TODO: We may have to write this diagnostic in the last iteration only.
                    this.Diagnostics.Add( Diagnostic.Create(
                        "CA01",
                        "Annotation",
                        "Split build-time and run-time variables into several declarations.",
                        DiagnosticSeverity.Error,
                        DiagnosticSeverity.Error,
                        true,
                        0,
                        location: node.GetLocation() ) );
                    return transformedVariableDeclaration;
                }
            }
        }

        public override SyntaxNode? VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
        {
            var transformedNode = (LocalDeclarationStatementSyntax) base.VisitLocalDeclarationStatement( node )!;

            return transformedNode.AddScopeAnnotation( this.GetNodeScope( transformedNode.Declaration ) );
        }

        public override SyntaxNode? VisitAttribute( AttributeSyntax node )
        {
            // Don't process attributes.
            return node;
        }

        public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            node = (MethodDeclarationSyntax) base.VisitMethodDeclaration( node )!;

            var symbol = this._semanticAnnotationMap.GetDeclaredSymbol( node )!;

            if ( this.GetSymbolScope( symbol, node ) == SymbolDeclarationScope.Template )
            {
                node = node.AddScopeAnnotation( SymbolDeclarationScope.Template );
            }

            return node;
        }

        public override SyntaxNode? VisitAssignmentExpression( AssignmentExpressionSyntax node )
        {
            var transformedNode = (AssignmentExpressionSyntax) base.VisitAssignmentExpression( node )!;

            if ( this._isRuntimeConditionalBlock )
            {
                return transformedNode.AddScopeAnnotation( SymbolDeclarationScope.RunTimeOnly );
            }
            else
            {
                var scope = this.GetCombinedScope( transformedNode.Left, transformedNode.Right );

                return transformedNode.AddScopeAnnotation( scope );
            }
        }

        public override SyntaxNode? VisitExpressionStatement( ExpressionStatementSyntax node )
        {
            var transformedNode = (ExpressionStatementSyntax) base.VisitExpressionStatement( node )!;

            return transformedNode.WithScopeAnnotationFrom( transformedNode.Expression ).WithScopeAnnotationFrom( node );
        }

        public override SyntaxNode? VisitCastExpression( CastExpressionSyntax node )
        {
            var annotatedType = (TypeSyntax?) this.Visit( node.Type );
            var annotatedExpression = (ExpressionSyntax?) this.Visit( node.Expression );
            var transformedNode = node.WithType( annotatedType ?? node.Type ).WithExpression( annotatedExpression ?? node.Expression );

            return this.AnnotateCastExpression( transformedNode, annotatedType, annotatedExpression );
        }

        public override SyntaxNode? VisitBinaryExpression( BinaryExpressionSyntax node )
        {
            switch ( node.Kind() )
            {
                case SyntaxKind.IsExpression:
                case SyntaxKind.AsExpression:
                    var annotatedType = (TypeSyntax?) this.Visit( node.Right );
                    var annotatedExpression = (ExpressionSyntax?) this.Visit( node.Left );
                    var transformedNode = node.WithLeft( annotatedExpression ?? node.Left ).WithRight( annotatedType ?? node.Right );

                    return this.AnnotateCastExpression( transformedNode, annotatedType, annotatedExpression );
            }

            return base.VisitBinaryExpression( node );
        }

        private SyntaxNode? AnnotateCastExpression( SyntaxNode transformedCastNode, TypeSyntax? annotatedType, ExpressionSyntax? annotatedExpression )
        {
            var combinedScope = this.GetNodeScope( annotatedType ) == SymbolDeclarationScope.Default
                ? this.GetNodeScope( annotatedExpression )
                : this.GetCombinedScope( annotatedType, annotatedExpression );

            if ( combinedScope != SymbolDeclarationScope.Default )
            {
                return transformedCastNode.AddScopeAnnotation( combinedScope );
            }

            return transformedCastNode;
        }

        public override SyntaxNode? VisitForStatement( ForStatementSyntax node )
        {
            // This is a quick-and-dirty implementation that all for statements runtime.

            if ( node.Declaration != null )
            {
                foreach ( var localDeclaration in node.Declaration.Variables )
                {
                    var local = (ILocalSymbol?) this._semanticAnnotationMap.GetDeclaredSymbol( localDeclaration );

                    if ( local != null )
                    {
                        this.TrySetLocalVariableScope( local, SymbolDeclarationScope.RunTimeOnly );
                    }
                }
            }

            var transformedVariableDeclaration = (VariableDeclarationSyntax) this.Visit( node.Declaration )!;
            var transformedInitializers = node.Initializers.Select( i => (ExpressionSyntax) this.Visit( i )! );
            var transformedCondition = (ExpressionSyntax) this.Visit( node.Condition )!;
            var transformedIncrementors = node.Incrementors.Select( syntax => this.Visit( syntax )! );

            StatementSyntax transformedStatement;
            using ( this.EnterRuntimeConditionalBlock() )
            {
                transformedStatement = (StatementSyntax) this.Visit( node.Statement )!;
            }

            return ForStatement(
                node.ForKeyword,
                node.OpenParenToken,
                transformedVariableDeclaration,
                SeparatedList( transformedInitializers ),
                node.FirstSemicolonToken,
                transformedCondition,
                node.SecondSemicolonToken,
                SeparatedList( transformedIncrementors ),
                node.CloseParenToken,
                transformedStatement );
        }

        public override SyntaxNode? VisitWhileStatement( WhileStatementSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitWhileStatement( node );
        }

        public override SyntaxNode? VisitDoStatement( DoStatementSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitDoStatement( node );
        }

        public override SyntaxNode? VisitGotoStatement( GotoStatementSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitGotoStatement( node );
        }

        public override SyntaxNode? VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitLocalFunctionStatement( node );
        }

        public override SyntaxNode? VisitAnonymousMethodExpression( AnonymousMethodExpressionSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitAnonymousMethodExpression( node );
        }

        public override SyntaxNode? VisitParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitParenthesizedLambdaExpression( node );
        }

        public override SyntaxNode? VisitSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitSimpleLambdaExpression( node );
        }

        public override SyntaxNode? VisitSwitchStatement( SwitchStatementSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitSwitchStatement( node );
        }

        public override SyntaxNode? VisitQueryExpression( QueryExpressionSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitQueryExpression( node );
        }

        public override SyntaxNode? VisitLockStatement( LockStatementSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitLockStatement( node );
        }

        public override SyntaxNode? VisitAwaitExpression( AwaitExpressionSyntax node )
        {
            var diagnostic = TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitAwaitExpression( node );
        }

        public override SyntaxNode? VisitInitializerExpression( InitializerExpressionSyntax node )
        {
            this.Diagnostics.Add( TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node ) );

            return base.VisitInitializerExpression( node );
        }

        public override SyntaxNode? VisitYieldStatement( YieldStatementSyntax node )
        {
            this.Diagnostics.Add( TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node ) );

            return base.VisitYieldStatement( node );
        }

        public override SyntaxNode? VisitUsingStatement( UsingStatementSyntax node )
        {
            this.Diagnostics.Add( TemplatingDiagnostic.CreateLanguageFeatureIsNotSupported( node ) );

            return base.VisitUsingStatement( node );
        }
    }
}