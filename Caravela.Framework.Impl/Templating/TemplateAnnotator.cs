// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Project;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IDiagnosticAdder _diagnosticAdder;

        /// <summary>
        /// Scope of local variables.
        /// </summary>
        private readonly Dictionary<ILocalSymbol, SymbolDeclarationScope> _localScopes = new();

        private readonly ISymbolClassifier _symbolScopeClassifier;

        private ScopeContext _currentScopeContext;

        private ISymbol? _currentTemplateMember;

        public TemplateAnnotator(
            CSharpCompilation compilation,
            SemanticAnnotationMap semanticAnnotationMap,
            IDiagnosticAdder diagnosticAdder )
        {
            this._symbolScopeClassifier = SymbolClassifier.GetInstance( compilation );
            this._semanticAnnotationMap = semanticAnnotationMap;
            this._diagnosticAdder = diagnosticAdder;

            // add default values of scope
            this._currentScopeContext = ScopeContext.CreateHelperScope( SymbolDeclarationScope.Both );
        }

        public bool Success { get; private set; } = true;

        private void ReportUnsupportedLanguageFeature( SyntaxNode node )
        {
            this.ReportDiagnostic( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node, node.Kind().ToString() );
        }

        /// <summary>
        /// Reports a diagnostic.
        /// </summary>
        /// <param name="descriptor">Diagnostic descriptor.</param>
        /// <param name="targetNode">Node on which the diagnostic should be reported.</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        /// <typeparam name="T"></typeparam>
        private void ReportDiagnostic<T>( StrongDiagnosticDescriptor<T> descriptor, SyntaxNode targetNode, T arguments )
        {
            var location = this._semanticAnnotationMap.GetLocation( targetNode );

            var diagnostic = descriptor.CreateDiagnostic( location, arguments );
            this._diagnosticAdder.ReportDiagnostic( diagnostic );

            if ( diagnostic.Severity == DiagnosticSeverity.Error )
            {
                this.Success = false;
            }
        }

        private bool TrySetLocalVariableScope( ILocalSymbol local, SymbolDeclarationScope scope )
        {
            if ( this._localScopes.TryGetValue( local, out var oldScope ) )
            {
                if ( oldScope != scope )
                {
                    var syntaxReference = local.DeclaringSyntaxReferences.FirstOrDefault();

                    if ( syntaxReference != null )
                    {
                        this.ReportDiagnostic(
                            TemplatingDiagnosticDescriptors.LocalVariableAmbiguousCoercion,
                            syntaxReference.GetSyntax(),
                            local.Name );
                    }
                    else
                    {
                        // Don't know how where to report the diagnostic. That should not happen in a valid compilation.
                    }

                    return false;
                }

                // Nothing to do.
                return true;
            }

            this._localScopes.Add( local, scope );

            return true;
        }

        /// <summary>
        /// Gets the scope of a symbol.
        /// </summary>
        /// <param name="symbol">A symbol.</param>
        /// <returns></returns>
        private SymbolDeclarationScope GetSymbolScope( ISymbol? symbol )
        {
            if ( symbol == null )
            {
                return SymbolDeclarationScope.Both;
            }

            // For local variables, we decide based on  _buildTimeLocals only. This collection is updated
            // at each iteration of the algorithm based on inferences from _requireMetaExpressionStack.
            if ( symbol is ILocalSymbol local )
            {
                if ( this._localScopes.TryGetValue( local, out var scope ) )
                {
                    return scope;
                }

                // Variables either run-time-only or compile-time-only. If we get here, it means
                // that the variable has not been classified, and in this case we apply the
                // default value: run-time only.
                return SymbolDeclarationScope.RunTimeOnly;
            }
            else if ( symbol is IParameterSymbol )
            {
                // Until we support template parameters and local functions, all parameters are parameters
                // of expression lambdas, which are of unknown scope.
                return SymbolDeclarationScope.Unknown;
            }

            // Aspect members are processed as compile-time-only by the template compiler even if some members can also
            // be called from run-time code.
            if ( this.IsTemplateMember( symbol ) )
            {
                return SymbolDeclarationScope.CompileTimeOnly;
            }

            // For other symbols, we use the SymbolScopeClassifier.
            return this._symbolScopeClassifier.GetSymbolDeclarationScope( symbol );
        }

        /// <summary>
        /// Determines if a symbol is a member of the current template class (or aspect class).
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private bool IsTemplateMember( ISymbol symbol )
            => this._currentTemplateMember != null
               && (SymbolEqualityComparer.Default.Equals( symbol, this._currentTemplateMember ) 
                    || (symbol.ContainingSymbol != null && SymbolEqualityComparer.Default.Equals( symbol.ContainingSymbol, this._currentTemplateMember )));

        /// <summary>
        /// Determines if a node is of <c>dynamic</c> type.
        /// </summary>
        /// <param name="originalNode"></param>
        /// <returns></returns>
        private bool IsDynamic( SyntaxNode originalNode ) => this._semanticAnnotationMap.GetExpressionType( originalNode ) is IDynamicTypeSymbol;

        /// <summary>
        /// Gets the scope of a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private SymbolDeclarationScope GetNodeScope( SyntaxNode? node )
        {
            if ( node == null )
            {
                return SymbolDeclarationScope.Both;
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
                        return this.GetSymbolScope( symbol );
                    }
                    else
                    {
                        return SymbolDeclarationScope.Both;
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
        private static SymbolDeclarationScope GetCombinedScope( params SymbolDeclarationScope[] scopes )
            => GetCombinedScope( (IEnumerable<SymbolDeclarationScope>) scopes );

        private SymbolDeclarationScope GetCombinedScope( params SyntaxNode?[] nodes ) => this.GetCombinedScope( (IEnumerable<SyntaxNode?>) nodes );

        private SymbolDeclarationScope GetCombinedScope( IEnumerable<SyntaxNode?> nodes ) => GetCombinedScope( nodes.Select( this.GetNodeScope ) );

        /// <summary>
        /// Gives the <see cref="SymbolDeclarationScope"/> of a parent given the scope of its children.
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private static SymbolDeclarationScope GetCombinedScope( IEnumerable<SymbolDeclarationScope> scopes )
        {
            var compileTimeOnlyCount = 0;
            var runtimeCount = 0;

            foreach ( var scope in scopes )
            {
                switch ( scope )
                {
                    case SymbolDeclarationScope.RunTimeOnly:
                        runtimeCount++;
                        break;

                    case SymbolDeclarationScope.CompileTimeOnly:
                        compileTimeOnlyCount++;
                        break;
                    
                    // Unknown is "greedy" it means all can be use at runtime or compile time
                    case SymbolDeclarationScope.Unknown:
                        return SymbolDeclarationScope.Unknown;
                }
            }

            if ( runtimeCount > 0 )
            {
                return SymbolDeclarationScope.RunTimeOnly;
            }
            else if (compileTimeOnlyCount > 0)
            {
                return SymbolDeclarationScope.CompileTimeOnly;
            }
            else
            {
                return SymbolDeclarationScope.Both;
            }
        }

        private ScopeContextCookie WithScopeContext(ScopeContext scopeContext)
        {
            var cookie = new ScopeContextCookie( this, this._currentScopeContext );
            this._currentScopeContext = scopeContext;

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

            if ( this._currentScopeContext.ForceCompileTimeOnlyExpression )
            {
                if ( transformedNode.GetScopeFromAnnotation() == SymbolDeclarationScope.RunTimeOnly ||
                     this.IsDynamic( transformedNode ) )
                {
                    // The current expression is obliged to be compile-time-only by inference.
                    // Emit an error if the type of the expression is inferred to be runtime-only.
                    this.RequireScope(
                        transformedNode,
                        SymbolDeclarationScope.RunTimeOnly,
                        SymbolDeclarationScope.CompileTimeOnly,
                        this._currentScopeContext.ForceCompileTimeOnlyExpressionReason! );

                    return transformedNode.AddScopeMismatchAnnotation();
                }

                // the current expression can be anotated as unknown (f.e. parameters of lambda expression)
                // that means it can be used as compile time and it doesn't need to be annotated as compileTime.
                if ( transformedNode.GetScopeFromAnnotation() != SymbolDeclarationScope.Unknown )
                {
                    return transformedNode.AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
                }
            }

            if ( transformedNode.HasScopeAnnotation() )
            {
                // If the transformed node has already an annotation, it means it has already been classified by
                // a previous run of the algorithm, and there is no need to classify it again.
                return transformedNode;
            }

            if ( node is ExpressionSyntax )
            {
                // Here is the default implementation for expressions. The scope of the parent is the combined scope of the children.
                var childScopes = transformedNode.ChildNodes().Where( c => c is ExpressionSyntax );

                return transformedNode.AddScopeAnnotation( this.GetCombinedScope( childScopes ) );
            }

            return transformedNode;
        }

        public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            var typeScope = this.GetSymbolScope( this._semanticAnnotationMap.GetDeclaredSymbol( node ) );

            if ( typeScope != SymbolDeclarationScope.RunTimeOnly )
            {
                return base.VisitClassDeclaration( node );
            }

            // This is not a build-time class so there's no need to analyze it.
            // The scope annotation is needed for syntax highlighting.
            return node.AddScopeAnnotation( SymbolDeclarationScope.RunTimeOnly );
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
                var scope = this.GetSymbolScope( symbol );
                var annotatedNode = identifierNameSyntax.AddScopeAnnotation( scope );

                // Add annotations for syntax coloring.
                if ( symbol is ILocalSymbol &&
                     scope == SymbolDeclarationScope.CompileTimeOnly )
                {
                    annotatedNode = annotatedNode.AddColoringAnnotation( TextSpanClassification.CompileTimeVariable );
                }
                else if ( symbol.GetAttributes()
                    .Any( a => a.AttributeClass != null && a.AttributeClass.AnyBaseType( t => t.Name == nameof( TemplateKeywordAttribute ) ) ) )
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

            return identifierNameSyntax;
        }

        public override SyntaxNode? VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
        {
            var transformedName = (SimpleNameSyntax) this.Visit( node.Name )!;

            if ( this.GetNodeScope( transformedName ) == SymbolDeclarationScope.CompileTimeOnly )
            {
                // If the member is compile-time (because of rules on the symbol), the expression on the left MUST be compile-time.

                using ( this.WithScopeContext( ScopeContext.CreateForceCompileTimeExpression( $"a compile-time-only member '${node.Name}'" ) ) )
                {
                    var transformedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;

                    return node.Update( transformedExpression, node.OperatorToken, transformedName )
                        .AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
                }
            }
            else
            {
                var transformedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;

                var expressionScope = this.GetNodeScope( transformedExpression );

                return node.Update( transformedExpression, node.OperatorToken, transformedName ).AddScopeAnnotation( expressionScope );
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
                        using ( this.WithScopeContext( ScopeContext.CreateForceCompileTimeExpression( $"a compile-time expression '{node.Expression}'" ) ) )
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

            // TODO: We're not processing ref/out arguments properly. These are possibly
            // local variable declarations and assignments.
            throw new AssertionFailedException();
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
                            (StatementSyntax) this.Visit( node.Else.Statement )! )
                        .AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly )
                        .WithTriviaFrom( node.Else )
                    : null;

                return node.Update(
                        node.AttributeLists,
                        node.IfKeyword,
                        node.OpenParenToken,
                        annotatedCondition,
                        node.CloseParenToken,
                        annotatedStatement,
                        annotatedElse )
                    .AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
            }

            // We have an if statement where the condition is a runtime expression. Any variable assignment
            // within this statement should make the variable as runtime-only, so we're calling EnterRuntimeConditionalBlock.
            using ( this.WithScopeContext( ScopeContext.CreateRuntimeConditionalBlock() ) )
            {
                var annotatedStatement = (StatementSyntax) this.Visit( node.Statement )!;
                var annotatedElse = (ElseClauseSyntax) this.Visit( node.Else )!;

                var result = node.Update( node.IfKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken, annotatedStatement, annotatedElse );

                return result;
            }
        }

        public override SyntaxNode? VisitBreakStatement( BreakStatementSyntax node )
        {
            return base.VisitBreakStatement( node )!.AddScopeAnnotation( this._currentScopeContext.CurrentScope );
        }

        public override SyntaxNode? VisitContinueStatement( ContinueStatementSyntax node )
        {
            return base.VisitContinueStatement( node )!.AddScopeAnnotation( this._currentScopeContext.CurrentScope );
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

                using ( this.WithScopeContext( ScopeContext.CreateHelperScope( SymbolDeclarationScope.CompileTimeOnly ) ) )
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

            // Run-time loop.

            using ( this.WithScopeContext( ScopeContext.CreateRuntimeConditionalBlock() ) )
            {
                StatementSyntax annotatedStatement;

                using ( this.WithScopeContext( ScopeContext.CreateHelperScope( SymbolDeclarationScope.Both ) ) )
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

            if ( this._currentScopeContext.ForceCompileTimeOnlyExpression )
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
                using ( this.WithScopeContext( ScopeContext.CreateForceCompileTimeExpression( $"a local variable of compile-time-only type '{node.Type}'" ) ) )
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

                this.ReportDiagnostic(
                    TemplatingDiagnosticDescriptors.SplitVariables,
                    node,
                    string.Join( ",", node.Variables.Select( v => "'" + v.Identifier.Text + "'" ) ) );

                return transformedVariableDeclaration;
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
            var symbol = this._semanticAnnotationMap.GetDeclaredSymbol( node )!;

            if ( this._symbolScopeClassifier.IsTemplate( symbol ) )
            {
                var previousTemplateMember = this._currentTemplateMember;
                this._currentTemplateMember = symbol;

                try
                {
                    return base.VisitMethodDeclaration( node )!.AddIsTemplateAnnotation();
                }
                finally
                {
                    this._currentTemplateMember = previousTemplateMember;
                }
            }
            else
            {
                return base.VisitMethodDeclaration( node );
            }
        }

        public override SyntaxNode? VisitAssignmentExpression( AssignmentExpressionSyntax node )
        {
            var transformedNode = (AssignmentExpressionSyntax) base.VisitAssignmentExpression( node )!;

            if ( this._currentScopeContext.IsRuntimeConditionalBlock )
            {
                return transformedNode.AddScopeAnnotation( SymbolDeclarationScope.RunTimeOnly );
            }

            var scope = this.GetCombinedScope( transformedNode.Right );

            return transformedNode.AddScopeAnnotation( scope );
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
            var transformedNode = node.WithType( annotatedType ).WithExpression( annotatedExpression );

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
                    var transformedNode = node.WithLeft( annotatedExpression ).WithRight( annotatedType );

                    return this.AnnotateCastExpression( transformedNode, annotatedType!, annotatedExpression! );
            }

            return base.VisitBinaryExpression( node );
        }

        private SyntaxNode? AnnotateCastExpression( SyntaxNode transformedCastNode, TypeSyntax annotatedType, ExpressionSyntax annotatedExpression )
        {
            var combinedScope = this.GetNodeScope( annotatedType ) == SymbolDeclarationScope.Both
                ? this.GetNodeScope( annotatedExpression )
                : this.GetCombinedScope( annotatedExpression );

            if ( combinedScope != SymbolDeclarationScope.Both )
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

            using ( this.WithScopeContext( ScopeContext.CreateRuntimeConditionalBlock() ) )
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

                using ( this.WithScopeContext( ScopeContext.CreateHelperScope( SymbolDeclarationScope.CompileTimeOnly ) ) )
                {
                    annotatedStatement = (StatementSyntax) this.Visit( node.Statement )!;
                }

                return node.Update( node.AttributeLists, node.WhileKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken, annotatedStatement )
                    .AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
            }

            // We have an while statement where the condition is a runtime expression. Any variable assignment
            // within this statement should make the variable as runtime-only, so we're calling EnterRuntimeConditionalBlock.

            using ( this.WithScopeContext( ScopeContext.CreateRuntimeConditionalBlock() ) )
            {
                StatementSyntax annotatedStatement;

                annotatedStatement = (StatementSyntax) this.Visit( node.Statement )!;

                var result = node.Update( node.WhileKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken, annotatedStatement );

                return result;
            }
        }

        public override SyntaxNode? VisitDoStatement( DoStatementSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node );

            return base.VisitDoStatement( node );
        }

        public override SyntaxNode? VisitGotoStatement( GotoStatementSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node );

            return base.VisitGotoStatement( node );
        }

        public override SyntaxNode? VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node );

            return base.VisitLocalFunctionStatement( node );
        }

        public override SyntaxNode? VisitAnonymousMethodExpression( AnonymousMethodExpressionSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node );

            return base.VisitAnonymousMethodExpression( node );
        }

        public override SyntaxNode? VisitParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node )
        {
            if ( node.ExpressionBody != null )
            {
                var annotatedExpression = (ExpressionSyntax) this.Visit( node.ExpressionBody )!;
                return node.WithExpressionBody( annotatedExpression ).AddScopeAnnotation( SymbolDeclarationScope.Unknown );
            }
            else
            {
                // it means Expression is a Block
                // TODO add more specific message, because only part of LanguageFeature is not supported
                this.ReportUnsupportedLanguageFeature( node );

                return base.VisitParenthesizedLambdaExpression( node );
            }
        }

        public override SyntaxNode? VisitSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
        {
            if ( node.ExpressionBody != null )
            {
                var annotatedExpression = (ExpressionSyntax) this.Visit( node.ExpressionBody )!;
                return node.WithExpressionBody( annotatedExpression ).AddScopeAnnotation( SymbolDeclarationScope.Unknown );
            }
            else
            {
                // it means Expression is a Block
                // TODO add more specific message, because only part of LanguageFeature is not supported
                this.ReportUnsupportedLanguageFeature( node );

                return base.VisitSimpleLambdaExpression( node );
            }
        }

        private void RequireScope( SwitchSectionSyntax section, SymbolDeclarationScope requiredScope )
        {
            // check label scope
            if ( section.Labels.Any() )
            {
                switch ( section.Labels[0] )
                {
                    case CasePatternSwitchLabelSyntax pattern:
                        this.ReportUnsupportedLanguageFeature( pattern );

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
                                this.ReportDiagnostic(
                                    TemplatingDiagnosticDescriptors.ScopeMismatch,
                                    oldLabel,
                                    (oldLabel.ToString(),
                                     existingScope.ToDisplayString(),
                                     requiredScope.ToDisplayString(),
                                     "a case") );
                            }
                        }

                        break;
                }
            }

            // check statement scope
            foreach ( var expressionStatement in section.Statements.OfType<ExpressionStatementSyntax>() )
            {
                var annotatedExpression = (ExpressionSyntax) this.Visit( expressionStatement?.Expression )!;
                this.RequireScope( annotatedExpression, requiredScope, "a case statement" );
            }
        }

        public override SyntaxNode? VisitSwitchStatement( SwitchStatementSyntax node )
        {
            var annotatedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;
            var expressionScope = annotatedExpression.GetScopeFromAnnotation();

            if ( (expressionScope == SymbolDeclarationScope.CompileTimeOnly && this.IsDynamic( annotatedExpression )) 
                || expressionScope != SymbolDeclarationScope.CompileTimeOnly )
            {
                expressionScope = SymbolDeclarationScope.RunTimeOnly;
            }

            var transformedSections = new SwitchSectionSyntax[node.Sections.Count];

            for ( var i = 0; i < node.Sections.Count; i++ )
            {
                var section = node.Sections[i];
                this.RequireScope( section, expressionScope );

                using ( this.WithScopeContext( ScopeContext.CreateHelperScope( expressionScope, isRuntimeConditionalBlock: expressionScope == SymbolDeclarationScope.RunTimeOnly ) ) )
                {
                    transformedSections[i] = (SwitchSectionSyntax) this.Visit( section )!.AddScopeAnnotation( expressionScope );
                }
            }

            if ( expressionScope == SymbolDeclarationScope.CompileTimeOnly )
            {
                return node.Update(
                        node.SwitchKeyword,
                        node.OpenParenToken,
                        annotatedExpression,
                        node.CloseParenToken,
                        node.OpenBraceToken,
                        List( transformedSections ),
                        node.CloseBraceToken )
                    .AddScopeAnnotation( SymbolDeclarationScope.CompileTimeOnly );
            }
            else
            {

                return node.Update(
                    node.SwitchKeyword,
                    node.OpenParenToken,
                    annotatedExpression,
                    node.CloseParenToken,
                    node.OpenBraceToken,
                    List( transformedSections ),
                    node.CloseBraceToken );
            }
        }

        public override SyntaxNode? VisitQueryExpression( QueryExpressionSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node );

            return base.VisitQueryExpression( node );
        }

        private void RequireScope( SyntaxNode node, SymbolDeclarationScope requiredScope, string reason )
            => this.RequireScope( node, this.GetNodeScope( node ), requiredScope, reason );
        
        private void RequireScope( BlockSyntax node, SymbolDeclarationScope requiredScope, string reason )
        {
            foreach ( var statement in node.Statements )
            {
                this.RequireScope( statement, requiredScope, reason );
            }
        }

        private void RequireScope( SyntaxNode node, SymbolDeclarationScope existingScope, SymbolDeclarationScope requiredScope, string reason )
        {
            if ( existingScope == SymbolDeclarationScope.CompileTimeOnly && this.IsDynamic( node ) )
            {
                existingScope = SymbolDeclarationScope.RunTimeOnly;
            }

            if ( existingScope != SymbolDeclarationScope.Both && existingScope != requiredScope )
            {
                // Don't emit an error if any descendant node already has an error because this creates redundant messages.
                if ( !node.DescendantNodes().Any( n => n.HasScopeMismatchAnnotation() ) )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.ScopeMismatch,
                        node,
                        (node.ToString(), existingScope.ToDisplayString(), requiredScope.ToDisplayString(), reason) );
                }
            }
        }

        public override SyntaxNode? VisitLockStatement( LockStatementSyntax node )
        {
            var annotatedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;

            this.RequireScope( annotatedExpression, SymbolDeclarationScope.RunTimeOnly, "a 'lock' statement" );

            return node.WithExpression( annotatedExpression ).AddScopeAnnotation( SymbolDeclarationScope.RunTimeOnly );
        }

        public override SyntaxNode? VisitAwaitExpression( AwaitExpressionSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node );

            return base.VisitAwaitExpression( node );
        }

        public override SyntaxNode? VisitInitializerExpression( InitializerExpressionSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node );

            return base.VisitInitializerExpression( node );
        }

        public override SyntaxNode? VisitYieldStatement( YieldStatementSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node );

            return base.VisitYieldStatement( node );
        }

        public override SyntaxNode? VisitUsingStatement( UsingStatementSyntax node )
        {
            var annotatedExpression = (ExpressionSyntax) this.Visit( node.Expression )!;

            this.RequireScope( annotatedExpression, SymbolDeclarationScope.RunTimeOnly, "a 'using' statement" );

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
                    this.RequireScope( genericArgument, scope, $"a generic argument of the compile-time method '{node.Identifier}'" );
                }
            }

            return base.VisitGenericName( node )!.AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitNullableType( NullableTypeSyntax node )
        {
            var transformedNode = (NullableTypeSyntax) base.VisitNullableType( node )!;

            return transformedNode.WithScopeAnnotationFrom( transformedNode.ElementType );
        }

        public override SyntaxNode? VisitTryStatement( TryStatementSyntax node )
        {
            var annotatedBlock = (BlockSyntax) this.Visit( node.Block )!;
            this.RequireScope( annotatedBlock, SymbolDeclarationScope.RunTimeOnly, "a 'try' statement" );

            var annotatedCatches = new CatchClauseSyntax[node.Catches.Count];
            for ( var i = 0; i < node.Catches.Count; i++ )
            {
                var @catch = node.Catches[i];
                var annotatedCatch = (CatchClauseSyntax) this.Visit( @catch )!;
                this.RequireScope( annotatedCatch.Block, SymbolDeclarationScope.RunTimeOnly, "a 'catch' statement" );
                annotatedCatches[i] = annotatedCatch;
            }

            FinallyClauseSyntax? annotatedFinally = null;
            if ( node.Finally != null )
            {
                annotatedFinally = (FinallyClauseSyntax) this.Visit( node.Finally )!;
                this.RequireScope( annotatedFinally.Block, SymbolDeclarationScope.RunTimeOnly, "a 'finally' statement" );
            }

            return node.WithBlock( annotatedBlock ).WithCatches(List(annotatedCatches)).WithFinally(annotatedFinally!).AddScopeAnnotation( SymbolDeclarationScope.RunTimeOnly );
        }
    }
}