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
        private readonly Dictionary<ILocalSymbol, SymbolDeclarationScope> _localScopes = new();

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

        private bool TrySetLocalVariableScope( ILocalSymbol local, SymbolDeclarationScope scope )
        {
            if ( this._localScopes.TryGetValue( local, out var oldScope ) )
            {
                if ( oldScope != scope )
                {
                    this.Diagnostics.Add(
                        TemplatingDiagnosticDescriptors.LocalVariableAmbiguousCoercion.CreateDiagnostic(
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
                    // Variables either run-time-only or compile-time-only. If we get here, it means
                    // that the variable has not been classified, and in this case we apply the
                    // default value: run-time only.
                    return SymbolDeclarationScope.RunTimeOnly;
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
                        this.RequireScope( nodeForDiagnostic, scopeFromClassifier, SymbolDeclarationScope.CompileTimeOnly, "a compile-time expression" );

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
                case NameSyntax name:
                    // If the node is an identifier, it means it should have a symbol,
                    // and the scope is given by the symbol.

                    var symbol = this._semanticAnnotationMap.GetSymbol( name );
                    if ( symbol != null )
                    {
                        return this.GetSymbolScope( symbol, node );
                    }
                    else
                    {
                        return SymbolDeclarationScope.Default;
                    }

                case NullableTypeSyntax nullableType:
                    return this.GetNodeScope( nullableType.ElementType );

                default:
                    // Otherwise, the scope is given by the annotation given by the deeper
                    // visitor or the previous algorithm iteration.
                    return node.GetScopeFromAnnotation();
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static SymbolDeclarationScope GetCombinedScope( SyntaxNode nodeForDiagnostic, params SymbolDeclarationScope[] scopes )
            => GetCombinedScope( nodeForDiagnostic, (IEnumerable<SymbolDeclarationScope>) scopes );

        private SymbolDeclarationScope GetCombinedScope( SyntaxNode nodeForDiagnostic, params SyntaxNode?[] nodes )
            => this.GetCombinedScope( nodeForDiagnostic, (IEnumerable<SyntaxNode?>) nodes );

        private SymbolDeclarationScope GetCombinedScope( SyntaxNode nodeForDiagnostic, IEnumerable<SyntaxNode?> nodes )
            => GetCombinedScope( nodeForDiagnostic, nodes.Select( this.GetNodeScope ) );

        /// <summary>
        /// Gives the <see cref="SymbolDeclarationScope"/> of a parent given the scope of its children.
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private static SymbolDeclarationScope GetCombinedScope( SyntaxNode nodeForDiagnostic, IEnumerable<SymbolDeclarationScope> scopes )
        {
            var compileTimeOnlyCount = 0;

            foreach ( var scope in scopes )
            {
                switch ( scope )
                {
                    case SymbolDeclarationScope.RunTimeOnly:
                        return SymbolDeclarationScope.RunTimeOnly;

                    case SymbolDeclarationScope.CompileTimeOnly:
                        compileTimeOnlyCount++;
                        break;
                }
            }

            return compileTimeOnlyCount > 0 ? SymbolDeclarationScope.CompileTimeOnly : SymbolDeclarationScope.Default;
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
                    this.RequireScope( node, SymbolDeclarationScope.CompileTimeOnly, SymbolDeclarationScope.RunTimeOnly, "in a run-time expression" );

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
                return transformedNode.AddScopeAnnotation( this.GetCombinedScope( node, childScopes ) );
            }
            else
            {
                return transformedNode;
            }
        }

        public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            var typeScope = this.GetSymbolScope( this._semanticAnnotationMap.GetDeclaredSymbol( node ), node );

            if ( typeScope != SymbolDeclarationScope.RunTimeOnly )
            {
                return base.VisitClassDeclaration( node );
            }
            else
            {
                // This is not a build-time class so there's no need to analyze it.
                // The scope annotation is needed for syntax highlighting.
                return node.AddScopeAnnotation( SymbolDeclarationScope.RunTimeOnly );
            }
        }

        public override SyntaxNode? VisitLiteralExpression( LiteralExpressionSyntax node ) => node;
        /*
        {
            
            // Literals are always compile-time (not really compile-time only but it does not matter), unless they are converted to dynamic.
            var scope = this.IsDynamic( node ) ? SymbolDeclarationScope.RunTimeOnly : SymbolDeclarationScope.CompileTimeOnly;
            return base.VisitLiteralExpression( node )!.AddScopeAnnotation( scope );
        }
        */

        public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
        {
            var identifierNameSyntax = (IdentifierNameSyntax) base.VisitIdentifierName( node )!;
            var symbol = this._semanticAnnotationMap.GetSymbol( node );

            if ( symbol != null )
            {
                var scope = this.GetSymbolScope( symbol, node );
                var annotatedNode = identifierNameSyntax.AddScopeAnnotation( scope );

                // Add annotations for syntax coloring.
                if ( symbol is ILocalSymbol &&
                     scope == SymbolDeclarationScope.CompileTimeOnly )
                {
                    annotatedNode = annotatedNode.AddColoringAnnotation( TextSpanClassification.CompileTimeVariable );
                }
                else if ( symbol.GetAttributes().Any(
                    a => a.AttributeClass != null && a.AttributeClass.AnyBaseType( t => t.Name == nameof( TemplateKeywordAttribute ) ) ) )
                {
                    annotatedNode = annotatedNode.AddColoringAnnotation( TextSpanClassification.TemplateKeyword );
                }
                else if ( scope == SymbolDeclarationScope.RunTimeOnly &&
                          (symbol.Kind == SymbolKind.Property || symbol.Kind == SymbolKind.Method) && this.IsDynamic( node ) )
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
                // If the expression on the left side is compile-time (because of rules on the symbol),
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

            if ( argument.RefKindKeyword.Kind() == SyntaxKind.None )
            {
                return argument.AddScopeAnnotation( this.GetNodeScope( argument.Expression ) );
            }
            else
            {
                // TODO: We're not processing ref/out arguments properly. These are possibly
                // local variable declarations and assignments.
                throw new AssertionFailedException();
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

            var local = (ILocalSymbol) this._semanticAnnotationMap.GetDeclaredSymbol( node )!;

            var annotatedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;

            var ifScope = this.GetNodeScope( annotatedExpression ).ReplaceDefault( SymbolDeclarationScope.RunTimeOnly );

            if ( !this.TrySetLocalVariableScope( local, ifScope ) )
            {
                throw new AssertionFailedException();
            }

            if ( ifScope == SymbolDeclarationScope.CompileTimeOnly )
            {
                // This is a build-time loop.

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
                        .AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly )
                        .WithSymbolAnnotationsFrom( node );

                return transformedNode;
            }
            else
            {
                // Run-time loop.

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
                        .AddScopeAnnotation( SymbolDeclarationScope.RunTimeOnly )
                        .WithSymbolAnnotationsFrom( node );
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

            SymbolDeclarationScope localScope;

            if ( this._forceCompileTimeOnlyExpression )
            {
                localScope = SymbolDeclarationScope.CompileTimeOnly;
            }
            else
            {
                // Infer the variable scope from the initializer.
                var transformedInitializerValue = transformedNode.Initializer?.Value;
                if ( transformedInitializerValue != null )
                {
                    localScope = this.GetNodeScope( transformedInitializerValue ).ReplaceDefault( SymbolDeclarationScope.RunTimeOnly );
                    transformedNode = transformedNode.WithInitializer( node.Initializer!.WithValue( transformedInitializerValue ) );
                }
                else
                {
                    // Variables without initializer have runtime scope.
                    localScope = SymbolDeclarationScope.RunTimeOnly;
                }
            }

            // Mark the local variable symbol.
            if ( !this.TrySetLocalVariableScope( local, localScope ) )
            {
                throw new AssertionFailedException();
            }

            // Mark the identifier for syntax highlighting.
            if ( localScope == SymbolDeclarationScope.CompileTimeOnly )
            {
                transformedNode = transformedNode.WithIdentifier(
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

                var variableScopes = transformedVariableDeclaration.Variables.Select( v => v.GetScopeFromAnnotation() ).Distinct().ToList();

                if ( variableScopes.Count == 1 )
                {
                    return transformedVariableDeclaration.AddScopeAnnotation( variableScopes.Single() );
                }
                else
                {
                    this.Diagnostics.Add(
                        TemplatingDiagnosticDescriptors.SplitVariables.CreateDiagnostic(
                        node.GetLocation(),
                        string.Join( ",", node.Variables.Select( v => "'" + v.Identifier.Text + "'" ) ) ) );
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

            if ( this._symbolScopeClassifier.IsTemplate( symbol ) )
            {
                node = node.AddIsTemplateAnnotation();
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
            var annotatedType = (TypeSyntax) this.Visit( node.Type )!;
            var annotatedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;
            var transformedNode = node.WithType( annotatedType ?? node.Type ).WithExpression( annotatedExpression ?? node.Expression );

            return this.AnnotateCastExpression( transformedNode, annotatedType!, annotatedExpression! );
        }

        public override SyntaxNode? VisitBinaryExpression( BinaryExpressionSyntax node )
        {
            switch ( node.Kind() )
            {
                case SyntaxKind.IsExpression:
                case SyntaxKind.AsExpression:
                    var annotatedType = (TypeSyntax) this.Visit( node.Right )!;
                    var annotatedExpression = (ExpressionSyntax) this.Visit( node.Left )!;
                    var transformedNode = node.WithLeft( annotatedExpression ?? node.Left ).WithRight( annotatedType ?? node.Right );

                    return this.AnnotateCastExpression( transformedNode, annotatedType!, annotatedExpression !);
            }

            return base.VisitBinaryExpression( node );
        }

        private SyntaxNode? AnnotateCastExpression( SyntaxNode transformedCastNode, TypeSyntax annotatedType, ExpressionSyntax annotatedExpression )
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
                        _ = this.TrySetLocalVariableScope( local, SymbolDeclarationScope.RunTimeOnly );
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
            var annotatedCondition = (ExpressionSyntax) this.Visit( node.Condition )!;
            var conditionScope = this.GetNodeScope( annotatedCondition );

            if ( conditionScope == SymbolDeclarationScope.CompileTimeOnly )
            {
                // We have an while statement where the condition is a compile-time expression. Add annotations
                // to the while but not to the statement or block itself.

                StatementSyntax annotatedStatement;
                using ( this.EnterBreakOrContinueScope( SymbolDeclarationScope.CompileTimeOnly ) )
                {
                    annotatedStatement = (StatementSyntax) this.Visit( node.Statement )!;
                }

                return node.Update( node.AttributeLists, node.WhileKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken, annotatedStatement )
                    .AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
            }
            else
            {
                // We have an while statement where the condition is a runtime expression. Any variable assignment
                // within this statement should make the variable as runtime-only, so we're calling EnterRuntimeConditionalBlock.

                using ( this.EnterRuntimeConditionalBlock() )
                {
                    StatementSyntax annotatedStatement;
                    using ( this.EnterBreakOrContinueScope( SymbolDeclarationScope.Default ) )
                    {
                        annotatedStatement = (StatementSyntax) this.Visit( node.Statement )!;
                    }

                    var result = node.Update( node.WhileKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken, annotatedStatement );

                    return result;
                }
            }
        }

        public override SyntaxNode? VisitDoStatement( DoStatementSyntax node )
        {
            var diagnostic = TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitDoStatement( node );
        }

        public override SyntaxNode? VisitGotoStatement( GotoStatementSyntax node )
        {
            var diagnostic = TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitGotoStatement( node );
        }

        public override SyntaxNode? VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
        {
            var diagnostic = TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitLocalFunctionStatement( node );
        }

        public override SyntaxNode? VisitAnonymousMethodExpression( AnonymousMethodExpressionSyntax node )
        {
            var diagnostic = TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitAnonymousMethodExpression( node );
        }

        public override SyntaxNode? VisitParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node )
        {
            var diagnostic = TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitParenthesizedLambdaExpression( node );
        }

        public override SyntaxNode? VisitSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
        {
            var diagnostic = TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitSimpleLambdaExpression( node );
        }

        private void RequireScope( SwitchSectionSyntax section, SymbolDeclarationScope requiredScope )
        {
            // check label scope
            if ( section.Labels.Any() )
            {

                switch ( section.Labels[0] )
                {
                    case CasePatternSwitchLabelSyntax:
                        var diagnostic = TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( section );
                        this.Diagnostics.Add( diagnostic );

                        break;
                    case CaseSwitchLabelSyntax oldLabel:
                        if ( oldLabel.Value != null )
                        {
                            SymbolDeclarationScope existingScope;
                            if ( oldLabel.Value is LiteralExpressionSyntax )
                            {
                                existingScope = requiredScope;
                            }
                            else
                            {
                                var annotatedCaseValue = (ExpressionSyntax) this.Visit( oldLabel.Value )!;
                                existingScope = annotatedCaseValue.GetScopeFromAnnotation();
                            }

                            if ( existingScope != requiredScope )
                            {
                                this.Diagnostics.Add(
                                        TemplatingDiagnosticDescriptors.ScopeMismatch.CreateDiagnostic(
                                        oldLabel.GetLocation(),
                                        (
                                        oldLabel.ToString(),
                                        existingScope.ToDisplayString(),
                                        requiredScope.ToDisplayString(),
                                        "a case") ) );
                            }
                        }

                        break;
                }
            }

            // check statement scope
            foreach ( ExpressionStatementSyntax expressionStatement in section.Statements.Where( s => s is ExpressionStatementSyntax ) )
            {
                var annotatedExpression = (ExpressionSyntax) this.Visit( expressionStatement?.Expression )!;
                this.RequireScope( annotatedExpression, requiredScope, "a case statement" );
            }
        }

        public override SyntaxNode? VisitSwitchStatement( SwitchStatementSyntax node )
        {
            var annotatedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;
            var expressionScope = annotatedExpression.GetScopeFromAnnotation();
            if ( expressionScope == SymbolDeclarationScope.CompileTimeOnly && this.IsDynamic( annotatedExpression ) )
            {
                expressionScope = SymbolDeclarationScope.RunTimeOnly;
            }

            if ( expressionScope == SymbolDeclarationScope.CompileTimeOnly )
            {
                var transformedSections = new SwitchSectionSyntax[node.Sections.Count];
                for ( var i = 0; i < node.Sections.Count; i++ )
                {
                    var section = node.Sections[i];
                    this.RequireScope( section, SymbolDeclarationScope.CompileTimeOnly );
                    using ( this.EnterBreakOrContinueScope( SymbolDeclarationScope.CompileTimeOnly ) )
                    {
                        transformedSections[i] = (SwitchSectionSyntax) this.Visit( section )!.AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
                    }
                }

                return node.Update( node.SwitchKeyword, node.OpenParenToken, annotatedExpression, node.CloseParenToken, node.OpenBraceToken, List( transformedSections ), node.CloseBraceToken )
                    .AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
            }
            else
            {
                var transformedSections = new SwitchSectionSyntax[node.Sections.Count];
                for ( var i = 0; i < node.Sections.Count; i++ )
                {
                    var section = node.Sections[i];
                    this.RequireScope( section, expressionScope );
                    using ( this.EnterRuntimeConditionalBlock() )
                    {
                        using ( this.EnterBreakOrContinueScope( expressionScope ) )
                        {
                            transformedSections[i] = (SwitchSectionSyntax) this.Visit( section )!.AddScopeAnnotation( expressionScope );
                        }
                    }
                }

                return node.Update( node.SwitchKeyword, node.OpenParenToken, annotatedExpression, node.CloseParenToken, node.OpenBraceToken, List( transformedSections ), node.CloseBraceToken );
            }
        }

        public override SyntaxNode? VisitQueryExpression( QueryExpressionSyntax node )
        {
            var diagnostic = TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitQueryExpression( node );
        }

        private void RequireScope( SyntaxNode node, SymbolDeclarationScope requiredScope, string reason )
            => this.RequireScope( node, this.GetNodeScope( node ), requiredScope, reason );

        private void RequireScope( SyntaxNode node, SymbolDeclarationScope existingScope, SymbolDeclarationScope requiredScope, string reason )
        {
            if ( existingScope == SymbolDeclarationScope.CompileTimeOnly && this.IsDynamic( node ) )
            {
                existingScope = SymbolDeclarationScope.RunTimeOnly;
            }

            if ( existingScope != SymbolDeclarationScope.Default && existingScope != requiredScope )
            {
                if ( node != null )
                {
                    this.Diagnostics.Add(
                        TemplatingDiagnosticDescriptors.ScopeMismatch.CreateDiagnostic(
                            node.GetLocation(),
                            (node.ToString(), existingScope.ToDisplayString(), requiredScope.ToDisplayString(), reason) ) );
                }
            }
        }

        public override SyntaxNode? VisitLockStatement( LockStatementSyntax node )
        {
            var annotatedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;

            this.RequireScope( annotatedExpression, SymbolDeclarationScope.RunTimeOnly, "a lock" );

            return node.WithExpression( annotatedExpression ).AddScopeAnnotation( SymbolDeclarationScope.RunTimeOnly );
        }

        public override SyntaxNode? VisitAwaitExpression( AwaitExpressionSyntax node )
        {
            var diagnostic = TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( node );
            this.Diagnostics.Add( diagnostic );

            return base.VisitAwaitExpression( node );
        }

        public override SyntaxNode? VisitInitializerExpression( InitializerExpressionSyntax node )
        {
            this.Diagnostics.Add( TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( node ) );

            return base.VisitInitializerExpression( node );
        }

        public override SyntaxNode? VisitYieldStatement( YieldStatementSyntax node )
        {
            this.Diagnostics.Add( TemplatingDiagnosticDescriptors.CreateLanguageFeatureIsNotSupported( node ) );

            return base.VisitYieldStatement( node );
        }

        public override SyntaxNode? VisitUsingStatement( UsingStatementSyntax node )
        {
            var annotatedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;

            this.RequireScope( annotatedExpression, SymbolDeclarationScope.RunTimeOnly, "a using" );

            return node.WithExpression( annotatedExpression ).AddScopeAnnotation( SymbolDeclarationScope.RunTimeOnly );
        }

        public override SyntaxNode? VisitGenericName( GenericNameSyntax node )
        {
            var scope = this.GetNodeScope( node );

            // If the method or type is compile-time, all generic arguments must be.
            if ( scope == SymbolDeclarationScope.CompileTimeOnly )
            {
                foreach ( var genericArgument in node.TypeArgumentList.Arguments )
                {
                    this.RequireScope( genericArgument, scope, "a generic argument of a compile-time method" );
                }
            }

            return base.VisitGenericName( node )!.AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitNullableType( NullableTypeSyntax node )
        {
            var transformedNode = (NullableTypeSyntax) base.VisitNullableType( node )!;
            return transformedNode.WithScopeAnnotationFrom( transformedNode.ElementType );
        }
    }
}