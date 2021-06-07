// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

#pragma warning disable SA1124 // Don't use regions

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// A <see cref="CSharpSyntaxRewriter"/> that adds annotation that distinguish compile-time from
    /// run-time syntax nodes. The input should be a syntax tree annotated with a <see cref="SyntaxTreeAnnotationMap"/>.
    /// </summary>
    internal partial class TemplateAnnotator : CSharpSyntaxRewriter
    {
        private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;
        private readonly IDiagnosticAdder _diagnosticAdder;
        private readonly SerializableTypes _serializableTypes;
        private readonly CancellationToken _cancellationToken;
        private readonly TemplateMemberClassifier _templateMemberClassifier;

        /// <summary>
        /// Scope of locally-defined symbols (local variables, anonymous types, ....).
        /// </summary>
        private readonly Dictionary<ISymbol, TemplatingScope> _localScopes = new();

        private readonly ISymbolClassifier _symbolScopeClassifier;

        private ScopeContext _currentScopeContext;

        private ISymbol? _currentTemplateMember;

        public TemplateAnnotator(
            CSharpCompilation compilation,
            SyntaxTreeAnnotationMap syntaxTreeAnnotationMap,
            IDiagnosticAdder diagnosticAdder,
            IServiceProvider serviceProvider,
            SerializableTypes serializableTypes,
            CancellationToken cancellationToken )
        {
            this._symbolScopeClassifier = serviceProvider.GetService<SymbolClassificationService>().GetClassifier( compilation );
            this._syntaxTreeAnnotationMap = syntaxTreeAnnotationMap;
            this._diagnosticAdder = diagnosticAdder;
            this._serializableTypes = serializableTypes;
            this._cancellationToken = cancellationToken;

            this._templateMemberClassifier = new TemplateMemberClassifier( compilation, syntaxTreeAnnotationMap, serviceProvider );

            // add default values of scope
            this._currentScopeContext = ScopeContext.Default;
        }

        public bool Success { get; private set; } = true;

        /// <summary>
        /// Reports a diagnostic.
        /// </summary>
        /// <param name="descriptor">Diagnostic descriptor.</param>
        /// <param name="targetNode">Node on which the diagnostic should be reported.</param>
        /// <param name="arguments">Arguments of the formatting string.</param>
        /// <typeparam name="T"></typeparam>
        private void ReportDiagnostic<T>( DiagnosticDefinition<T> descriptor, SyntaxNodeOrToken targetNode, T arguments )
        {
            var location = this._syntaxTreeAnnotationMap.GetLocation( targetNode );

            this.ReportDiagnostic( descriptor, location, arguments );
        }

        private void ReportDiagnostic<T>( DiagnosticDefinition<T> descriptor, Location? location, T arguments )
        {
            var diagnostic = descriptor.CreateDiagnostic( location, arguments );
            this._diagnosticAdder.Report( diagnostic );

            if ( diagnostic.Severity == DiagnosticSeverity.Error )
            {
                this.Success = false;
            }
        }

        private void SetLocalSymbolScope( ISymbol symbol, TemplatingScope scope, SyntaxToken identifier )
        {
            if ( this._localScopes.TryGetValue( symbol, out _ ) )
            {
                throw new AssertionFailedException( $"The symbol {symbol} was already assigned to the scope {scope}." );
            }

            this._localScopes.Add( symbol, scope );

            if ( scope == TemplatingScope.CompileTimeOnly && identifier.Kind() != SyntaxKind.None )
            {
                this.ReportDiagnostic( TemplatingDiagnosticDescriptors.VariableIsCompileTime, identifier, symbol );
            }
        }

        /// <summary>
        /// Gets the scope of a symbol.
        /// </summary>
        /// <param name="symbol">A symbol.</param>
        /// <returns></returns>
        private TemplatingScope GetSymbolScope( ISymbol? symbol )
        {
            if ( symbol == null )
            {
                return GetMoreSpecificScope( TemplatingScope.Both );
            }

            // For local variables, we decide based on  _buildTimeLocals only. This collection is updated
            // at each iteration of the algorithm based on inferences from _requireMetaExpressionStack.
            if ( symbol is ILocalSymbol or INamedTypeSymbol { IsAnonymousType: true } )
            {
                if ( this._localScopes.TryGetValue( symbol, out var scope ) )
                {
                    return scope;
                }

                // When a local variable is assigned to an anonymous type, the scope is unknown because the anonymous
                // type is visited after the variable identifier.
                return TemplatingScope.Unknown;
            }
            else if ( symbol is { ContainingType: { IsAnonymousType: true } containingType } )
            {
                return GetMoreSpecificScope( this.GetSymbolScope( containingType ) );
            }

            // The TemplateContext.runTime method must be processed separately. It is a compile-time-only method whose
            // return is run-time-only.
            if ( this._templateMemberClassifier.IsRunTimeMethod( symbol ) )
            {
                return TemplatingScope.RunTimeOnly;
            }

            if ( symbol is IParameterSymbol )
            {
                // Until we support template parameters and local functions, all parameters are parameters
                // of expression lambdas, which are of unknown scope.
                return TemplatingScope.Unknown;
            }

            // Aspect members are processed as compile-time-only by the template compiler even if some members can also
            // be called from run-time code.
            if ( this.IsAspectMember( symbol ) )
            {
                switch ( this._symbolScopeClassifier.GetTemplateMemberKind( symbol ) )
                {
                    case TemplateMemberKind.Introduction:
                        return TemplatingScope.RunTimeOnly;

                    default:
                        return TemplatingScope.CompileTimeOnly;
                }
            }

            // For other symbols, we use the SymbolScopeClassifier.
            return GetMoreSpecificScope( this._symbolScopeClassifier.GetTemplatingScope( symbol ) );

            TemplatingScope GetMoreSpecificScope( TemplatingScope scope )
            {
                if ( scope != TemplatingScope.Both )
                {
                    return scope;
                }
                else
                {
                    if ( this._currentScopeContext.PreferRunTimeExpression )
                    {
                        if ( symbol is ITypeSymbol typeSymbol )
                        {
                            if ( !this._serializableTypes.IsSerializable( typeSymbol ) )
                            {
                                return TemplatingScope.RunTimeOnly;
                            }
                        }
                    }

                    return TemplatingScope.Both;
                }
            }
        }

        /// <summary>
        /// Determines if a symbol is a member of the current template class (or aspect class).
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private bool IsAspectMember( ISymbol symbol )
            => this._currentTemplateMember != null
               && symbol.ContainingType != null
               && symbol.ContainingType.SpecialType != SpecialType.System_Object
               && symbol.IsMemberOf( this._currentTemplateMember.ContainingType );

        /// <summary>
        /// Gets the scope of a <see cref="SyntaxNode"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private TemplatingScope GetNodeScope( SyntaxNode? node )
        {
            if ( node == null )
            {
                return TemplatingScope.Both;
            }

            // If the node is dynamic, it is run-time only.
            if ( this._templateMemberClassifier.IsDynamicType( node ) )
            {
                var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

                // Dynamic local variables are considered compile-time because they must be transformed. 
                return this._templateMemberClassifier.IsCompileTime( symbol ) || symbol is ILocalSymbol
                    ? TemplatingScope.CompileTimeDynamic
                    : TemplatingScope.Dynamic;
            }

            switch ( node )
            {
                case NameSyntax name:
                    // If the node is an identifier, it means it should have a symbol,
                    // and the scope is given by the symbol.

                    var symbol = this._syntaxTreeAnnotationMap.GetSymbol( name );

                    if ( symbol != null )
                    {
                        return this.GetSymbolScope( symbol );
                    }
                    else
                    {
                        // If there is no symbol, the member may be dynamic.
                        return TemplatingScope.Unknown;
                    }

                case NullableTypeSyntax nullableType:
                    return this.GetNodeScope( nullableType.ElementType );

                default:
                    // Otherwise, the scope is given by the annotation given by the deeper
                    // visitor or the previous algorithm iteration.
                    return node.GetScopeFromAnnotation().GetValueOrDefault();
            }
        }

        // ReSharper disable once UnusedMember.Local

        private TemplatingScope GetExpressionTypeScope( SyntaxNode? node )
        {
            if ( node != null && this._syntaxTreeAnnotationMap.GetExpressionType( node ) is { } parentExpressionType )
            {
                return this.GetSymbolScope( parentExpressionType );
            }
            else
            {
                return TemplatingScope.Both;
            }
        }

        private TemplatingScope GetExpressionScope( IEnumerable<SyntaxNode?>? annotatedChildren, SyntaxNode? originalParent = null )
            => this.GetExpressionScope( annotatedChildren?.Select( this.GetNodeScope ), originalParent );

        /// <summary>
        /// Gives the <see cref="TemplatingScope"/> of a parent given the scope of its children.
        /// </summary>
        /// <param name="childrenScopes"></param>
        /// <param name="originalParent"></param>
        /// <returns></returns>
        private TemplatingScope GetExpressionScope( IEnumerable<TemplatingScope>? childrenScopes, SyntaxNode? originalParent = null )
        {
            // Get the scope of type of the parent node.

            if ( originalParent != null )
            {
                var parentExpressionScope = this.GetExpressionTypeScope( originalParent );

                if ( !parentExpressionScope.IsIndeterminate() )
                {
                    return parentExpressionScope;
                }
            }

            if ( childrenScopes == null )
            {
                return TemplatingScope.Both;
            }

            var compileTimeOnlyCount = 0;
            var runtimeCount = 0;

            foreach ( var scope in childrenScopes )
            {
                switch ( scope )
                {
                    case TemplatingScope.CompileTimeDynamic:
                    case TemplatingScope.RunTimeOnly:
                        runtimeCount++;

                        break;

                    case TemplatingScope.CompileTimeOnly:
                        compileTimeOnlyCount++;

                        break;

                    // Unknown is "greedy" it means all can be use at runtime or compile time
                    case TemplatingScope.Unknown:
                        return TemplatingScope.Unknown;
                }
            }

            if ( runtimeCount > 0 )
            {
                // If there is a single run-time-only child, the whole expression is necessarily run-time-only.
                return TemplatingScope.RunTimeOnly;
            }
            else if ( compileTimeOnlyCount > 0 )
            {
                return TemplatingScope.CompileTimeOnly;
            }
            else
            {
                return TemplatingScope.Both;
            }
        }

        private ScopeContextCookie WithScopeContext( ScopeContext? scopeContext )
        {
            if ( scopeContext == this._currentScopeContext || scopeContext == null )
            {
                return default;
            }

            var cookie = new ScopeContextCookie( this, this._currentScopeContext );
            this._currentScopeContext = scopeContext;

            return cookie;
        }

        [return: NotNullIfNotNull( "node" )]
        private T? Visit<T>( T? node )
            where T : SyntaxNode
            => (T) this.DefaultVisitImpl( node )!;

        /// <summary>
        /// Default visitor.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode? Visit( SyntaxNode? node ) => this.DefaultVisitImpl( node );

        [return: NotNullIfNotNull( "node" )]
        private SyntaxNode? DefaultVisitImpl( SyntaxNode? node )
        {
            this._cancellationToken.ThrowIfCancellationRequested();

            // Adds annotations to the children node.
            var transformedNode = base.Visit( node );

            return this.AddScopeAnnotationToVisitedNode( node, transformedNode );
        }

        /// <summary>
        /// Adds scope annotation to a node that has been visited - so children are annotated but not the node itself.
        /// </summary>
        [return: NotNullIfNotNull( "node" )]
        private SyntaxNode? AddScopeAnnotationToVisitedNode( SyntaxNode? node, SyntaxNode? visitedNode )
        {
            if ( visitedNode == null )
            {
                return null;
            }

            if ( this._currentScopeContext.ForceCompileTimeOnlyExpression )
            {
                if ( visitedNode.GetScopeFromAnnotation() == TemplatingScope.RunTimeOnly ||
                     this._templateMemberClassifier.IsDynamicType( visitedNode ) )
                {
                    // The current expression is obliged to be compile-time-only by inference.
                    // Emit an error if the type of the expression is inferred to be runtime-only.
                    this.RequireScope(
                        visitedNode,
                        TemplatingScope.RunTimeOnly,
                        TemplatingScope.CompileTimeOnly,
                        this._currentScopeContext.PreferredScopeReason! );

                    return visitedNode.AddScopeMismatchAnnotation();
                }

                // the current expression can be annotated as unknown (f.e. parameters of lambda expression)
                // that means it can be used as compile time and it doesn't need to be annotated as compileTime.
                if ( visitedNode.GetScopeFromAnnotation() != TemplatingScope.Unknown )
                {
                    return visitedNode.ReplaceScopeAnnotation( TemplatingScope.CompileTimeOnly );
                }
            }

            if ( visitedNode.HasScopeAnnotation() )
            {
                // If the transformed node has already an annotation, it means it has already been classified by
                // a previous run of the algorithm, and there is no need to classify it again.
                return visitedNode;
            }

            // Here is the default implementation for expressions. The scope of the parent is the combined scope of the children.
            var childNodes = visitedNode.ChildNodes().Where( c => c is ExpressionSyntax );

            return visitedNode.AddScopeAnnotation( this.GetExpressionScope( childNodes, node ) );
        }

        #region Anonymous objects

        public override SyntaxNode? VisitAnonymousObjectMemberDeclarator( AnonymousObjectMemberDeclaratorSyntax node )
        {
            var scope = this._currentScopeContext.ForceCompileTimeOnlyExpression ? TemplatingScope.CompileTimeOnly : TemplatingScope.RunTimeOnly;

            return node.Update( node.NameEquals, this.Visit( node.Expression ) ).AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitAnonymousObjectCreationExpression( AnonymousObjectCreationExpressionSyntax node )
        {
            var scope = this._currentScopeContext.ForceCompileTimeOnlyExpression ? TemplatingScope.CompileTimeOnly : TemplatingScope.RunTimeOnly;

            var symbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node );

            if ( symbol != null )
            {
                this.SetLocalSymbolScope( symbol, scope, default );
            }

            // Anonymous objects are currently run-time-only unless they are in a compile-time-only scope -- until we implement more complex rules.
            var transformedMembers =
                node.Initializers.Select( i => this.Visit( i )!.AddScopeAnnotation( scope ) );

            return node.Update(
                    node.NewKeyword,
                    node.OpenBraceToken,
                    SeparatedList( transformedMembers, node.Initializers.GetSeparators() ),
                    node.CloseBraceToken )
                .AddScopeAnnotation( scope );
        }

        #endregion

        public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            var typeScope = this.GetSymbolScope( this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node ) );

            if ( typeScope != TemplatingScope.RunTimeOnly )
            {
                return base.VisitClassDeclaration( node );
            }

            // This is not a build-time class so there's no need to analyze it.
            // The scope annotation is needed for syntax highlighting.
            return node.AddScopeAnnotation( TemplatingScope.RunTimeOnly );
        }

        public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
        {
            var identifierNameSyntax = (IdentifierNameSyntax) base.VisitIdentifierName( node )!;
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

            if ( symbol != null )
            {
                var scope = this.GetSymbolScope( symbol );

                if ( scope == TemplatingScope.CompileTimeOnly )
                {
                    // Template code cannot be referenced in a template until this is implemented.
                    if ( this._symbolScopeClassifier.GetTemplateMemberKind( symbol ) == TemplateMemberKind.Template )
                    {
                        this.ReportDiagnostic(
                            TemplatingDiagnosticDescriptors.TemplateCannotReferenceTemplate,
                            node,
                            (symbol, this._currentTemplateMember!) );
                    }
                }

                var annotatedNode = identifierNameSyntax.AddScopeAnnotation( scope );

                annotatedNode = (IdentifierNameSyntax) this.AddColoringAnnotations( annotatedNode, symbol, scope )!;

                return annotatedNode;
            }

            return identifierNameSyntax;
        }

        private SyntaxNodeOrToken AddColoringAnnotations( SyntaxNodeOrToken nodeOrToken, ISymbol? symbol, TemplatingScope scope )
        {
            switch ( symbol )
            {
                case null:
                    return nodeOrToken;

                case ILocalSymbol when scope == TemplatingScope.CompileTimeOnly:
                    nodeOrToken = nodeOrToken.AddColoringAnnotation( TextSpanClassification.CompileTimeVariable );

                    break;

                default:
                    {
                        if ( this._templateMemberClassifier.HasTemplateKeywordAttribute( symbol ) )
                        {
                            nodeOrToken = nodeOrToken.AddColoringAnnotation( TextSpanClassification.TemplateKeyword );
                        }
                        else
                        {
                            var node = nodeOrToken.AsNode() ?? nodeOrToken.Parent;

                            if ( node != null &&
                                 this._templateMemberClassifier.IsDynamicType( node ) &&
                                 symbol is not ITypeSymbol )
                            {
                                // Annotate dynamic members differently for syntax coloring.
                                nodeOrToken = nodeOrToken.AddColoringAnnotation( TextSpanClassification.Dynamic );
                            }
                        }

                        break;
                    }
            }

            return nodeOrToken;
        }

        public override SyntaxNode? VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
        {
            var transformedName = this.Visit( node.Name )!;

            var nameScope = this.GetNodeScope( transformedName );
            ScopeContext? context = null;
            var scope = TemplatingScope.Both;

            switch ( nameScope )
            {
                case TemplatingScope.CompileTimeOnly:
                    // If the member is compile-time (because of rules on the symbol), the expression on the left MUST be compile-time.
                    context = ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"a compile-time-only member '{node.Name}'" );
                    scope = TemplatingScope.CompileTimeOnly;

                    break;

                case TemplatingScope.CompileTimeDynamic:
                    // This dynamic member is compile-time but the result is run-time.
                    context = ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"a compile-time-only member '{node.Name}'" );
                    scope = TemplatingScope.CompileTimeDynamic;

                    break;

                case TemplatingScope.Dynamic:
                    // A member is run-time dynamic because the left part is dynamic, so there is no need to force it run-time.
                    // It can actually contain build-time subexpressions.
                    scope = TemplatingScope.Dynamic;

                    break;

                case TemplatingScope.Unknown when this._syntaxTreeAnnotationMap.GetExpressionType( node.Expression ) is IDynamicTypeSymbol:
                    // This is a member access of a dynamic receiver.
                    scope = TemplatingScope.RunTimeOnly;
                    context = ScopeContext.CreatePreferredRunTimeScope( this._currentScopeContext, $"a member of the run-time-only '{node.Name}'" );

                    break;

                case TemplatingScope.RunTimeOnly:
                    scope = TemplatingScope.RunTimeOnly;

                    break;
            }

            using ( this.WithScopeContext( context ) )
            {
                var transformedExpression = this.Visit( node.Expression )!;

                if ( scope == TemplatingScope.Both )
                {
                    scope = this.GetNodeScope( transformedExpression );
                }

                return node.Update( transformedExpression, node.OperatorToken, transformedName )
                    .AddScopeAnnotation( scope );
            }
        }

        public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
        {
            // nameof() is always compile-time.
            if ( node.IsNameOf() )
            {
                return node.AddScopeAnnotation( TemplatingScope.Both );
            }

            // If we have any out/ref argument that assigns a compile-time variable, the whole method call is compile-time, and we cannot
            // be in a run-time-conditional block.
            var compileTimeOutArguments = node.ArgumentList.Arguments.Where(
                    a => a.RefKindKeyword.Kind() is SyntaxKind.OutKeyword or SyntaxKind.RefKeyword
                         && this.GetNodeScope( a.Expression ) == TemplatingScope.CompileTimeOnly )
                .ToList();

            ScopeContext? expressionContext = null;

            if ( compileTimeOutArguments.Count > 0 )
            {
                expressionContext =
                    ScopeContext.CreateForcedCompileTimeScope(
                        this._currentScopeContext,
                        $"a call to a method that sets the compile-time variable '{compileTimeOutArguments[0]}'" );

                if ( this._currentScopeContext.IsRuntimeConditionalBlock )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.CannotSetCompileTimeVariableInRunTimeConditionalBlock,
                        compileTimeOutArguments[0],
                        (compileTimeOutArguments[0].ToString(), this._currentScopeContext.IsRuntimeConditionalBlockReason!) );
                }
            }

            // Transform the expression.
            ExpressionSyntax transformedExpression;

            using ( this.WithScopeContext( expressionContext ) )
            {
                transformedExpression = this.Visit( node.Expression )!;
            }

            var expressionScope = this.GetNodeScope( transformedExpression );
            var expressionSymbol = this._syntaxTreeAnnotationMap.GetSymbol( node.Expression );

            var parameters = expressionSymbol switch
            {
                null => default,
                IMethodSymbol method => method.Parameters,
                IPropertySymbol property => property.Parameters,
                IParameterSymbol parameter when parameter.Type.TypeKind == TypeKind.Delegate => ((INamedTypeSymbol) parameter.Type).Constructors.Single()
                    .Parameters,
                ILocalSymbol local when local.Type.TypeKind == TypeKind.Delegate => ((INamedTypeSymbol) local.Type).Constructors.Single().Parameters,
                IEventSymbol @event => ((INamedTypeSymbol) @event.Type).Constructors.Single().Parameters,
                _ => throw new NotImplementedException( $"Don't know how to get the parameters of '{expressionSymbol}'." )
            };

            InvocationExpressionSyntax updatedInvocation;

            if ( !expressionScope.IsIndeterminate() )
            {
                // If the scope of the expression on the left side is known (because of rules on the symbol),
                // we know the scope of arguments upfront. Otherwise, we need to decide of the invocation scope based on arguments (else branch of this if).

                var transformedArguments = new List<ArgumentSyntax>( node.ArgumentList.Arguments.Count );

                for ( var argumentIndex = 0; argumentIndex < node.ArgumentList.Arguments.Count; argumentIndex++ )
                {
                    var argument = node.ArgumentList.Arguments[argumentIndex];

                    // The parameter index can be different than the argument index in case of 'params xx[]'.
                    IParameterSymbol? parameter;

                    if ( !parameters.IsDefault )
                    {
                        var parameterIndex = argumentIndex >= parameters.Length ? parameters.Length - 1 : argumentIndex;
                        parameter = parameters[parameterIndex];
                    }
                    else
                    {
                        parameter = null;
                    }

                    var argumentType = parameter?.Type ?? this._syntaxTreeAnnotationMap.GetParameterSymbol( argument )?.Type;

                    ArgumentSyntax transformedArgument;

                    // dynamic or dynamic[]
                    if ( expressionScope.IsDynamic() || this._templateMemberClassifier.IsDynamicType( argumentType ) )
                    {
                        using ( this.WithScopeContext(
                            ScopeContext.CreatePreferredRunTimeScope(
                                this._currentScopeContext,
                                $"argument of the dynamic parameter '{parameter?.Name ?? argumentIndex.ToString()}'" ) ) )
                        {
                            transformedArgument = (ArgumentSyntax) this.VisitArgument( argument )!;
                        }
                    }
                    else if ( expressionScope.IsRunTime() )
                    {
                        using ( this.WithScopeContext(
                            ScopeContext.CreatePreferredRunTimeScope(
                                this._currentScopeContext,
                                $"argument of the run-time method '{node.Expression}'" ) ) )
                        {
                            transformedArgument = (ArgumentSyntax) this.VisitArgument( argument )!;
                        }
                    }
                    else
                    {
                        using ( this.WithScopeContext(
                            ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"a compile-time expression '{node.Expression}'" ) ) )
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

                updatedInvocation = updatedInvocation.AddScopeAnnotation( expressionScope );
            }
            else
            {
                // If the expression on the left of the parenthesis is not compile-time,
                // we cannot take a decision on the parent expression.

                TemplatingScope invocationScope;

                var transformedArgumentList = (ArgumentListSyntax) this.VisitArgumentList( node.ArgumentList )!;
                transformedArgumentList = transformedArgumentList.WithOpenParenToken( node.ArgumentList.OpenParenToken );
                transformedArgumentList = transformedArgumentList.WithCloseParenToken( node.ArgumentList.CloseParenToken );
                updatedInvocation = node.Update( transformedExpression, transformedArgumentList );

                if ( expressionScope == TemplatingScope.RunTimeOnly || this._templateMemberClassifier.IsRunTimeMethod( node.Expression ) )
                {
                    invocationScope = TemplatingScope.RunTimeOnly;
                }
                else
                {
                    invocationScope = this.GetExpressionScope( transformedArgumentList.Arguments, node );
                }

                updatedInvocation = updatedInvocation.AddScopeAnnotation( invocationScope );
            }

            return updatedInvocation;
        }

        public override SyntaxNode? VisitArgument( ArgumentSyntax node )
        {
            var argument = (ArgumentSyntax) base.VisitArgument( node )!;

            return argument.AddScopeAnnotation( this.GetNodeScope( argument.Expression ).DynamicToRunTimeOnly() );
        }

        public override SyntaxNode? VisitIfStatement( IfStatementSyntax node )
        {
            var annotatedCondition = this.Visit( node.Condition )!;
            var conditionScope = this.GetNodeScope( annotatedCondition );

            if ( conditionScope == TemplatingScope.CompileTimeOnly )
            {
                // We have an if statement where the condition is a compile-time expression. Add annotations
                // to the if and else statements but not to the blocks themselves.

                var annotatedStatement = this.Visit( node.Statement )!;

                var annotatedElse = node.Else != null
                    ? ElseClause(
                            node.Else.ElseKeyword,
                            this.Visit( node.Else.Statement )! )
                        .AddScopeAnnotation( TemplatingScope.CompileTimeOnly )
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
                    .AddScopeAnnotation( TemplatingScope.CompileTimeOnly );
            }

            // We have an if statement where the condition is a runtime expression. Any variable assignment
            // within this statement should make the variable as runtime-only, so we're calling EnterRuntimeConditionalBlock.
            using ( this.WithScopeContext( ScopeContext.CreateRuntimeConditionalScope( this._currentScopeContext, "if ( " + node.Condition + " )" ) ) )
            {
                var annotatedStatement = this.Visit( node.Statement )!;
                var annotatedElse = this.Visit( node.Else )!;

                var result = node.Update( node.IfKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken, annotatedStatement, annotatedElse );

                return result;
            }
        }

        public override SyntaxNode? VisitBreakStatement( BreakStatementSyntax node )
        {
            return node.AddScopeAnnotation( this._currentScopeContext.CurrentBreakOrContinueScope );
        }

        public override SyntaxNode? VisitContinueStatement( ContinueStatementSyntax node )
        {
            return node.AddScopeAnnotation( this._currentScopeContext.CurrentBreakOrContinueScope );
        }

        public override SyntaxNode? VisitForEachStatement( ForEachStatementSyntax node )
        {
            var local = (ILocalSymbol) this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node )!;

            var annotatedExpression = this.Visit( node.Expression )!;

            var forEachScope = this.GetNodeScope( annotatedExpression ).ReplaceIndeterminate( TemplatingScope.RunTimeOnly );

            this.SetLocalSymbolScope( local, forEachScope, node.Identifier );

            this.RequireLoopScope( node.Expression, forEachScope, "foreach" );

            StatementSyntax annotatedStatement;

            using ( this.WithScopeContext(
                ScopeContext.CreateBreakOrContinueScope( this._currentScopeContext, forEachScope, $"foreach ( {node.Type} {node.Identifier} in ... )" ) ) )
            {
                annotatedStatement = this.Visit( node.Statement )!;
            }

            var identifierClassification = forEachScope == TemplatingScope.CompileTimeOnly ? TextSpanClassification.CompileTimeVariable : default;

            var transformedNode =
                ForEachStatement(
                        default,
                        node.ForEachKeyword,
                        node.OpenParenToken,
                        node.Type,
                        node.Identifier.AddColoringAnnotation( identifierClassification ),
                        node.InKeyword,
                        annotatedExpression,
                        node.CloseParenToken,
                        annotatedStatement )
                    .AddScopeAnnotation( forEachScope )
                    .WithSymbolAnnotationsFrom( node );

            return transformedNode;
        }

        #region Pattern Matching

        public override SyntaxNode? VisitDeclarationPattern( DeclarationPatternSyntax node )
        {
            // If the type of a pattern is compile-time-only, the variable is compile-time.

            var transformedType = this.Visit( node.Type );
            var scope = this.GetNodeScope( transformedType );

            var context = scope == TemplatingScope.CompileTimeOnly
                ? ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"local variable of compile-time '{node.Type}'" )
                : null;

            VariableDesignationSyntax transformedDesignation;

            using ( this.WithScopeContext( context ) )
            {
                transformedDesignation = this.Visit( node.Designation )!;
            }

            return node.Update( transformedType, transformedDesignation ).AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitIsPatternExpression( IsPatternExpressionSyntax node )
        {
            // The scope of a pattern expression is given by the expression (left part).
            var transformedExpression = this.Visit( node.Expression )!;
            var scope = this.GetNodeScope( transformedExpression ).DynamicToRunTimeOnly();

            var context = scope == TemplatingScope.CompileTimeOnly
                ? ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"pattern on the compile-time expression '{node.Expression}'" )
                : null;

            PatternSyntax transformedPattern;

            using ( this.WithScopeContext( context ) )
            {
                transformedPattern = this.Visit( node.Pattern )!;
            }

            if ( scope == TemplatingScope.RunTimeOnly )
            {
                this.RequireScope( transformedPattern, TemplatingScope.RunTimeOnly, $"pattern on the run-time expression '{node.Expression}'" );
            }

            return node.Update( transformedExpression, node.IsKeyword, transformedPattern ).AddScopeAnnotation( scope );
        }

        #endregion

        #region Variables

        public override SyntaxNode? VisitSingleVariableDesignation( SingleVariableDesignationSyntax node )
        {
            var symbol = (ILocalSymbol?) this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node );

            var scope = this._currentScopeContext.ForceCompileTimeOnlyExpression ? TemplatingScope.CompileTimeOnly : TemplatingScope.RunTimeOnly;

            var color = TextSpanClassification.Default;

            if ( symbol != null )
            {
                this.SetLocalSymbolScope( symbol, scope, node.Identifier );

                if ( scope == TemplatingScope.CompileTimeOnly )
                {
                    color = TextSpanClassification.CompileTimeVariable;
                }
            }

            var transformedNode = node.WithIdentifier( node.Identifier.AddColoringAnnotation( color ) ).AddScopeAnnotation( scope );

            return transformedNode;
        }

        public override SyntaxNode? VisitDeclarationExpression( DeclarationExpressionSyntax node )
        {
            // This methods processes in-line variable declarations expressions, like in `out var x`.

            var transformedType = this.Visit( node.Type );

            TemplatingScope scope;
            ScopeContext? context = null;

            if ( this._currentScopeContext.ForceCompileTimeOnlyExpression )
            {
                scope = TemplatingScope.CompileTimeOnly;
            }
            else
            {
                scope = this.GetNodeScope( transformedType );

                if ( scope == TemplatingScope.CompileTimeOnly )
                {
                    context = ScopeContext.CreateForcedCompileTimeScope(
                        this._currentScopeContext,
                        $"an inline variable declaration of compile-time type '{transformedType}" );
                }
            }

            VariableDesignationSyntax transformedDesignation;

            using ( this.WithScopeContext( context ) )
            {
                transformedDesignation = this.Visit( node.Designation );
            }

            return node.Update( transformedType, transformedDesignation ).AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitVariableDeclarator( VariableDeclaratorSyntax node )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node )!;

            if ( symbol is not ILocalSymbol local )
            {
                // it's a field, or a field-like event
                return node;
            }

            TemplatingScope localScope;
            var transformedInitializer = node.Initializer?.WithValue( this.Visit( node.Initializer.Value ) );

            // Decide of the scope of the local variable.
            if ( this._currentScopeContext.ForceCompileTimeOnlyExpression )
            {
                localScope = TemplatingScope.CompileTimeOnly;
            }
            else
            {
                // Infer the variable scope from the initializer.

                if ( transformedInitializer != null )
                {
                    localScope = this.GetNodeScope( transformedInitializer.Value ).ReplaceIndeterminate( TemplatingScope.RunTimeOnly ).DynamicToRunTimeOnly();
                }
                else
                {
                    // Variables without initializer have runtime scope.
                    localScope = TemplatingScope.RunTimeOnly;
                }
            }

            // Mark the local variable symbol.
            this.SetLocalSymbolScope( local, localScope, node.Identifier );

            var transformedIdentifier = node.Identifier;

            // Mark the identifier for syntax highlighting.
            if ( localScope == TemplatingScope.CompileTimeOnly )
            {
                transformedIdentifier = transformedIdentifier.AddColoringAnnotation( TextSpanClassification.CompileTimeVariable );
            }

            // Transform arguments.
            ArgumentSyntax[]? transformedArguments = null;

            if ( node.ArgumentList != null )
            {
                using ( this.WithScopeContext(
                    localScope == TemplatingScope.CompileTimeOnly
                        ? ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, "creation of a compile-time object" )
                        : ScopeContext.CreatePreferredRunTimeScope( this._currentScopeContext, "creation of a run-time object" ) ) )
                {
                    transformedArguments = node.ArgumentList.Arguments.Select( a => this.Visit( a )! ).ToArray();
                }
            }

            var transformedArgumentList = transformedArguments != null
                ? BracketedArgumentList(
                    node.ArgumentList!.OpenBracketToken,
                    SeparatedList( transformedArguments, node.ArgumentList.Arguments.GetSeparators() ),
                    node.ArgumentList.CloseBracketToken )
                : null;

            return node.Update( transformedIdentifier, transformedArgumentList, transformedInitializer ).AddScopeAnnotation( localScope );
        }

        public override SyntaxNode? VisitVariableDeclaration( VariableDeclarationSyntax node )
        {
            var transformedType = this.Visit( node.Type )!;

            if ( this.GetNodeScope( transformedType ) == TemplatingScope.CompileTimeOnly )
            {
                using ( this.WithScopeContext(
                    ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"a local variable of compile-time-only type '{node.Type}'" ) ) )
                {
                    var transformedVariableDeclaration = (VariableDeclarationSyntax) base.VisitVariableDeclaration( node )!;

                    return transformedVariableDeclaration.AddScopeAnnotation( TemplatingScope.CompileTimeOnly );
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

        #endregion

        public override SyntaxNode? VisitAttribute( AttributeSyntax node )
        {
            // Don't process attributes.
            return node;
        }

        public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node )!;

            if ( this._symbolScopeClassifier.GetTemplateMemberKind( symbol ) != TemplateMemberKind.None )
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

        private static bool IsMutatingUnaryOperator( SyntaxToken token ) => token.Kind() is SyntaxKind.PlusPlusToken or SyntaxKind.MinusMinusToken;

        public override SyntaxNode? VisitPostfixUnaryExpression( PostfixUnaryExpressionSyntax node )
        {
            var transformedOperand = this.VisitUnaryExpressionOperand( node.Operand, node.OperatorToken );

            return node.Update( transformedOperand, node.OperatorToken ).WithSymbolAnnotationsFrom( node ).WithScopeAnnotationFrom( transformedOperand );
        }

        public override SyntaxNode? VisitPrefixUnaryExpression( PrefixUnaryExpressionSyntax node )
        {
            var transformedOperand = this.VisitUnaryExpressionOperand( node.Operand, node.OperatorToken );

            return node.Update( node.OperatorToken, transformedOperand ).WithSymbolAnnotationsFrom( node ).WithScopeAnnotationFrom( transformedOperand );
        }

        private ExpressionSyntax VisitUnaryExpressionOperand( ExpressionSyntax operand, SyntaxToken @operator )
        {
            var transformedOperand = this.Visit( operand )!;

            var scope = this.GetNodeScope( transformedOperand );

            // We cannot mutate a compile-time expression in a run-time-condition block.
            if ( scope == TemplatingScope.CompileTimeOnly && IsMutatingUnaryOperator( @operator ) )
            {
                if ( this._currentScopeContext.IsRuntimeConditionalBlock )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.CannotSetCompileTimeVariableInRunTimeConditionalBlock,
                        operand,
                        (operand.ToString(), this._currentScopeContext.IsRuntimeConditionalBlockReason!) );
                }
            }

            return transformedOperand;
        }

        public override SyntaxNode? VisitAssignmentExpression( AssignmentExpressionSyntax node )
        {
            // The scope of an assignment is determined by the left side.
            var transformedLeft = this.Visit( node.Left )!;

            var scope = this.GetNodeScope( transformedLeft ).DynamicToRunTimeOnly();
            ExpressionSyntax? transformedRight;

            // If we are in a run-time-conditional block, we cannot assign compile-time variables.
            ScopeContext? context = null;

            if ( scope == TemplatingScope.CompileTimeOnly )
            {
                if ( this._currentScopeContext.IsRuntimeConditionalBlock )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.CannotSetCompileTimeVariableInRunTimeConditionalBlock,
                        node.Left,
                        (node.Left.ToString(), this._currentScopeContext.IsRuntimeConditionalBlockReason!) );
                }

                // The right part must be compile-time.
                context = ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, "the assignment of a compile-time expression" );
            }

            using ( this.WithScopeContext( context ) )
            {
                transformedRight = this.Visit( node.Right )!;
            }

            return node.Update( transformedLeft, node.OperatorToken, transformedRight ).AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitExpressionStatement( ExpressionStatementSyntax node )
        {
            var transformedNode = (ExpressionStatementSyntax) base.VisitExpressionStatement( node )!;

            return transformedNode.WithScopeAnnotationFrom( transformedNode.Expression );
        }

        public override SyntaxNode? VisitCastExpression( CastExpressionSyntax node )
        {
            var annotatedType = this.Visit( node.Type )!;
            var annotatedExpression = this.Visit( node.Expression )!;
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
                    var annotatedExpression = this.Visit( node.Left )!;
                    var transformedNode = node.WithLeft( annotatedExpression ).WithRight( annotatedType );

                    return this.AnnotateCastExpression( transformedNode, annotatedType!, annotatedExpression! );
            }

            var visitedNode = base.VisitBinaryExpression( node );

            return this.AddScopeAnnotationToVisitedNode( node, visitedNode );
        }

        private SyntaxNode? AnnotateCastExpression( SyntaxNode transformedCastNode, TypeSyntax annotatedType, ExpressionSyntax annotatedExpression )
        {
            // TODO: Verify
            var combinedScope = this.GetNodeScope( annotatedType ) == TemplatingScope.Both
                ? this.GetNodeScope( annotatedExpression ).DynamicToRunTimeOnly()
                : this.GetExpressionScope( new[] { annotatedExpression }, transformedCastNode );

            if ( combinedScope != TemplatingScope.Both )
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
                this.RequireScope( node.Declaration.Variables, TemplatingScope.RunTimeOnly, "variable of a 'for' loop" );
            }

            var transformedVariableDeclaration = this.Visit( node.Declaration )!;
            var transformedInitializers = node.Initializers.Select( i => this.Visit( i )! );
            var transformedCondition = this.Visit( node.Condition )!;
            var transformedIncrementors = node.Incrementors.Select( syntax => this.Visit( syntax )! );

            StatementSyntax transformedStatement;

            using ( this.WithScopeContext(
                ScopeContext.CreateBreakOrContinueScope(
                    this._currentScopeContext,
                    TemplatingScope.RunTimeOnly,
                    $"for ( {node.Initializers}; ... )" ) ) )
            {
                transformedStatement = this.Visit( node.Statement )!;
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
            // The scope of a `while` statement is determined by its condition only.

            var annotatedCondition = this.Visit( node.Condition )!;
            var conditionScope = this.GetNodeScope( annotatedCondition );

            this.RequireLoopScope( node.Condition, conditionScope, "white" );

            StatementSyntax annotatedStatement;

            using ( this.WithScopeContext(
                ScopeContext.CreateBreakOrContinueScope( this._currentScopeContext, conditionScope, $"while ( {node.Condition} )" ) ) )
            {
                annotatedStatement = this.Visit( node.Statement )!;
            }

            return node.Update( node.AttributeLists, node.WhileKeyword, node.OpenParenToken, annotatedCondition, node.CloseParenToken, annotatedStatement )
                .AddScopeAnnotation( conditionScope );
        }

        #region Unsupported Features

        private void ReportUnsupportedLanguageFeature( SyntaxNodeOrToken nodeForDiagnostic, string featureName )
        {
            this.ReportDiagnostic( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, nodeForDiagnostic, featureName );
        }

        public override SyntaxNode? VisitDoStatement( DoStatementSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node.DoKeyword, "do" );

            return base.VisitDoStatement( node );
        }

        public override SyntaxNode? VisitGotoStatement( GotoStatementSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node.GotoKeyword, "goto" );

            return base.VisitGotoStatement( node );
        }

        public override SyntaxNode? VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node.Identifier, "local function" );

            return base.VisitLocalFunctionStatement( node );
        }

        public override SyntaxNode? VisitAnonymousMethodExpression( AnonymousMethodExpressionSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node.DelegateKeyword, "anonymous method" );

            return base.VisitAnonymousMethodExpression( node );
        }

        public override SyntaxNode? VisitQueryExpression( QueryExpressionSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node.FromClause.FromKeyword, "from" );

            return base.VisitQueryExpression( node );
        }

        public override SyntaxNode? VisitAwaitExpression( AwaitExpressionSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node.AwaitKeyword, "await" );

            return base.VisitAwaitExpression( node );
        }

        public override SyntaxNode? VisitYieldStatement( YieldStatementSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node.YieldKeyword, "yield" );

            return base.VisitYieldStatement( node );
        }

        #endregion

        #region Lambda expressions

        public override SyntaxNode? VisitParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node )
        {
            if ( node.ExpressionBody != null )
            {
                var annotatedExpression = this.Visit( node.ExpressionBody )!;

                return node.WithExpressionBody( annotatedExpression ).AddScopeAnnotation( TemplatingScope.Unknown );
            }
            else
            {
                // it means Expression is a Block
                this.ReportUnsupportedLanguageFeature( node.ArrowToken, "statement lambda" );

                return base.VisitParenthesizedLambdaExpression( node );
            }
        }

        public override SyntaxNode? VisitSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
        {
            if ( node.ExpressionBody != null )
            {
                var annotatedExpression = this.Visit( node.ExpressionBody )!;

                return node.WithExpressionBody( annotatedExpression ).AddScopeAnnotation( TemplatingScope.Unknown );
            }
            else
            {
                // it means Expression is a Block
                this.ReportUnsupportedLanguageFeature( node.ArrowToken, "statement lambda" );

                return base.VisitSimpleLambdaExpression( node );
            }
        }

        #endregion

        #region Switch

        public override SyntaxNode? VisitSwitchExpressionArm( SwitchExpressionArmSyntax node )
        {
            var transformedPattern = this.Visit( node.Pattern )!;
            var patternScope = this.GetNodeScope( transformedPattern );

            var transformedWhen = this.Visit( node.WhenClause );
            var transformedExpression = this.Visit( node.Expression )!;

            TemplatingScope combinedScope;

            if ( patternScope == TemplatingScope.CompileTimeOnly )
            {
                // If the pattern is build-time only, then the whole arm is build-time only.
                combinedScope = TemplatingScope.CompileTimeOnly;
            }
            else
            {
                combinedScope = this.GetSwitchCaseScope( transformedPattern, transformedWhen, transformedExpression );
            }

            return node.Update(
                    transformedPattern,
                    transformedWhen,
                    node.EqualsGreaterThanToken,
                    transformedExpression )
                .AddScopeAnnotation( combinedScope );
        }

        public override SyntaxNode? VisitSwitchExpression( SwitchExpressionSyntax node )
        {
            var transformedGoverningExpression = this.Visit( node.GoverningExpression )!;
            var governingExpressionScope = transformedGoverningExpression.GetScopeFromAnnotation().GetValueOrDefault();

            if ( (governingExpressionScope == TemplatingScope.CompileTimeOnly
                  && this._templateMemberClassifier.IsDynamicType( transformedGoverningExpression ))
                 || governingExpressionScope != TemplatingScope.CompileTimeOnly )
            {
                governingExpressionScope = TemplatingScope.RunTimeOnly;
            }

            var armContext = governingExpressionScope == TemplatingScope.CompileTimeOnly
                ? ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, "a compile-time switch expression" )
                : null;

            SwitchExpressionArmSyntax[] transformedArms;

            using ( this.WithScopeContext( armContext ) )
            {
                transformedArms = node.Arms.Select( a => this.Visit( a ) ).ToArray();

                this.RequireScope( transformedArms, governingExpressionScope, "a compile-time switch expression" );
            }

            return node.Update(
                    transformedGoverningExpression,
                    node.SwitchKeyword,
                    node.OpenBraceToken,
                    SeparatedList( transformedArms, node.Arms.GetSeparators() ),
                    node.CloseBraceToken )
                .AddScopeAnnotation( governingExpressionScope );
        }

        public override SyntaxNode? VisitSwitchStatement( SwitchStatementSyntax node )
        {
            var annotatedExpression = this.Visit( node.Expression )!;
            var expressionScope = annotatedExpression.GetScopeFromAnnotation().GetValueOrDefault();

            if ( (expressionScope == TemplatingScope.CompileTimeOnly && this._templateMemberClassifier.IsDynamicType( annotatedExpression ))
                 || expressionScope != TemplatingScope.CompileTimeOnly )
            {
                expressionScope = TemplatingScope.RunTimeOnly;
            }

            var transformedSections = new SwitchSectionSyntax[node.Sections.Count];
            var switchReason = $"switch ( {node.Expression} )";

            for ( var i = 0; i < node.Sections.Count; i++ )
            {
                var section = node.Sections[i];

                SwitchLabelSyntax[] transformedLabels;
                StatementSyntax[] transformedStatements;

                const string compileTimeReason = "the 'case' is a part of a compile-time 'switch'";

                var labelContext = expressionScope == TemplatingScope.CompileTimeOnly
                    ? ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, compileTimeReason )
                    : null;

                using ( this.WithScopeContext( labelContext ) )
                {
                    transformedLabels = section.Labels.Select( l => this.Visit( l )! ).ToArray();
                    this.RequireScope( transformedLabels, expressionScope, compileTimeReason );
                }

                using ( this.WithScopeContext( ScopeContext.CreateBreakOrContinueScope( this._currentScopeContext, expressionScope, switchReason ) ) )
                {
                    transformedStatements = section.Statements.Select( s => this.Visit( s )! ).ToArray();
                }

                transformedSections[i] = section.Update( List( transformedLabels ), List( transformedStatements ) ).AddScopeAnnotation( expressionScope );
            }

            if ( expressionScope == TemplatingScope.CompileTimeOnly )
            {
                return node.Update(
                        node.SwitchKeyword,
                        node.OpenParenToken,
                        annotatedExpression,
                        node.CloseParenToken,
                        node.OpenBraceToken,
                        List( transformedSections ),
                        node.CloseBraceToken )
                    .AddScopeAnnotation( TemplatingScope.CompileTimeOnly );
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

        private TemplatingScope GetSwitchCaseScope(
            SyntaxNode transformedPattern,
            SyntaxNode? transformedWhen,
            SyntaxNode? transformedExpression = null )
        {
            return this.GetExpressionScope( new[] { transformedPattern, transformedWhen, transformedExpression } );
        }

        public override SyntaxNode? VisitCasePatternSwitchLabel( CasePatternSwitchLabelSyntax node )
        {
            var transformedPattern = this.Visit( node.Pattern );
            var patternScope = this.GetNodeScope( transformedPattern );
            var transformedWhen = this.Visit( node.WhenClause );

            var combinedScope = patternScope == TemplatingScope.CompileTimeOnly
                ? TemplatingScope.CompileTimeOnly
                : this.GetSwitchCaseScope( transformedPattern, transformedWhen );

            return node.Update( node.Keyword, node.Pattern, node.WhenClause, node.ColonToken ).AddScopeAnnotation( combinedScope );
        }

        #endregion

        private void RequireScope( IEnumerable<SyntaxNode> nodes, TemplatingScope requiredScope, string reason )
        {
            foreach ( var node in nodes )
            {
                this.RequireScope( node, requiredScope, reason );
            }
        }

        private void RequireScope( SyntaxNode? node, TemplatingScope requiredScope, string reason )
            => this.RequireScope( node, this.GetNodeScope( node ), requiredScope, reason );

        private void RequireScope( SyntaxNode? node, TemplatingScope existingScope, TemplatingScope requiredScope, string reason )
        {
            if ( node == null )
            {
                return;
            }

            // TODO: remove the next line (it should come as Dynamic).
            if ( (existingScope == TemplatingScope.CompileTimeOnly && this._templateMemberClassifier.IsDynamicType( node )) ||
                 existingScope == TemplatingScope.CompileTimeDynamic ||
                 existingScope == TemplatingScope.Dynamic )
            {
                existingScope = TemplatingScope.RunTimeOnly;
            }

            if ( existingScope != TemplatingScope.Both && existingScope != requiredScope )
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

        private void RequireLoopScope( SyntaxNode nodeForDiagnostic, TemplatingScope requiredScope, string statementName )
        {
            if ( requiredScope == TemplatingScope.CompileTimeOnly && this._currentScopeContext.IsRuntimeConditionalBlock )
            {
                // It is not allowed to have a loop in a run-time-conditional block because compile-time loops require a compile-time
                // variable, and mutating a compile-time variable is not allowed in a run-time-conditional block. This condition may be
                // removed in the future because the loop variable may actually not be observable from outside the block, this this
                // is not implemented.

                this.ReportDiagnostic(
                    TemplatingDiagnosticDescriptors.CannotHaveCompileTimeLoopInRunTimeConditionalBlock,
                    nodeForDiagnostic,
                    (statementName, this._currentScopeContext.IsRuntimeConditionalBlockReason!) );
            }
        }

        public override SyntaxNode? VisitLockStatement( LockStatementSyntax node )
        {
            var annotatedExpression = this.Visit( node.Expression )!;
            var annotatedStatement = this.Visit( node.Statement );

            this.RequireScope( annotatedExpression, TemplatingScope.RunTimeOnly, "a 'lock' statement" );

            return node.Update(
                    node.LockKeyword,
                    node.OpenParenToken,
                    annotatedExpression,
                    node.CloseParenToken,
                    annotatedStatement )
                .AddScopeAnnotation( TemplatingScope.RunTimeOnly );
        }

        public override SyntaxNode? VisitUsingStatement( UsingStatementSyntax node )
        {
            var annotatedExpression = this.Visit( node.Expression )!;
            var annotatedDeclaration = this.Visit( node.Declaration );
            var annotatedStatement = this.Visit( node.Statement );

            this.RequireScope( annotatedExpression, TemplatingScope.RunTimeOnly, "a 'using' statement" );
            this.RequireScope( annotatedDeclaration, TemplatingScope.RunTimeOnly, "a 'using' statement" );

            return node.Update(
                    node.AwaitKeyword,
                    node.UsingKeyword,
                    node.OpenParenToken,
                    annotatedDeclaration!,
                    annotatedExpression,
                    node.CloseParenToken,
                    annotatedStatement )
                .AddScopeAnnotation( TemplatingScope.RunTimeOnly );
        }

        public override SyntaxNode? VisitGenericName( GenericNameSyntax node )
        {
            var scope = this.GetNodeScope( node );
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

            var transformedNode = (GenericNameSyntax) base.VisitGenericName( node )!;

            // If the method or type is compile-time, all generic arguments must be.
            if ( scope == TemplatingScope.CompileTimeOnly )
            {
                foreach ( var genericArgument in transformedNode.TypeArgumentList.Arguments )
                {
                    this.RequireScope( genericArgument, scope, $"a generic argument of the compile-time method '{node.Identifier}'" );
                }
            }

            var annotatedIdentifier = this.AddColoringAnnotations( node.Identifier, symbol, scope ).AsToken();

            return
                transformedNode.WithIdentifier( annotatedIdentifier ).AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitNullableType( NullableTypeSyntax node )
        {
            var transformedNode = (NullableTypeSyntax) base.VisitNullableType( node )!;

            return transformedNode.WithScopeAnnotationFrom( transformedNode.ElementType );
        }

        public override SyntaxNode? VisitObjectCreationExpression( ObjectCreationExpressionSyntax node )
        {
            var transformedType = this.Visit( node.Type );
            var objectType = this._syntaxTreeAnnotationMap.GetExpressionType( node );
            var objectTypeScope = this.GetSymbolScope( objectType );

            ScopeContext? context = null;

            if ( objectTypeScope == TemplatingScope.CompileTimeOnly )
            {
                context = ScopeContext.CreateForcedCompileTimeScope(
                    this._currentScopeContext,
                    $"the creation of an instance of the compile-time {objectType}" );
            }

            using ( this.WithScopeContext( context ) )
            {
                var transformedArguments = node.ArgumentList?.Arguments.Select( a => this.Visit( a )! ).ToArray();
                var argumentsScope = this.GetExpressionScope( transformedArguments, node );
                var transformedInitializer = this.Visit( node.Initializer );
                var initializerScope = this.GetNodeScope( transformedInitializer );

                var combinedScope = objectTypeScope switch
                {
                    TemplatingScope.CompileTimeOnly => TemplatingScope.CompileTimeOnly,
                    TemplatingScope.RunTimeOnly => TemplatingScope.RunTimeOnly,
                    _ => this.GetExpressionScope( new[] { argumentsScope, initializerScope }, node )
                };

                var transformedArgumentList = transformedArguments != null
                    ? ArgumentList(
                        node.ArgumentList!.OpenParenToken,
                        SeparatedList( transformedArguments, node.ArgumentList.Arguments.GetSeparators() ),
                        node.ArgumentList!.CloseParenToken )
                    : null;

                return node.Update(
                        node.NewKeyword,
                        transformedType,
                        transformedArgumentList,
                        transformedInitializer )
                    .AddScopeAnnotation( combinedScope );
            }
        }

        public override SyntaxNode? VisitThrowExpression( ThrowExpressionSyntax node )
        {
            using ( this.WithScopeContext( ScopeContext.CreatePreferredRunTimeScope( this._currentScopeContext, "an expression of a 'throw' expression" ) ) )
            {
                var transformedExpression = this.Visit( node.Expression )!;

                this.RequireScope( transformedExpression, TemplatingScope.RunTimeOnly, "a 'throw' expression" );

                return node.Update( node.ThrowKeyword, transformedExpression ).AddScopeAnnotation( TemplatingScope.RunTimeOnly );
            }
        }

        public override SyntaxNode? VisitThrowStatement( ThrowStatementSyntax node )
        {
            using ( this.WithScopeContext( ScopeContext.CreatePreferredRunTimeScope( this._currentScopeContext, "an expression of a 'throw' statement" ) ) )
            {
                var transformedExpression = this.Visit( node.Expression )!;

                this.RequireScope( transformedExpression, TemplatingScope.RunTimeOnly, "a 'throw' statement" );

                return node.Update( node.ThrowKeyword, transformedExpression, node.SemicolonToken ).AddScopeAnnotation( TemplatingScope.RunTimeOnly );
            }
        }

        public override SyntaxNode? VisitTryStatement( TryStatementSyntax node )
        {
            var annotatedBlock = this.Visit( node.Block )!;

            var annotatedCatches = new CatchClauseSyntax[node.Catches.Count];

            for ( var i = 0; i < node.Catches.Count; i++ )
            {
                var @catch = node.Catches[i];

                using ( this.WithScopeContext( ScopeContext.CreateRuntimeConditionalScope( this._currentScopeContext, "catch" ) ) )
                {
                    var annotatedCatch = this.Visit( @catch )!;
                    annotatedCatches[i] = annotatedCatch;
                }
            }

            FinallyClauseSyntax? annotatedFinally = null;

            if ( node.Finally != null )
            {
                using ( this.WithScopeContext( ScopeContext.CreateRuntimeConditionalScope( this._currentScopeContext, "finally" ) ) )
                {
                    annotatedFinally = this.Visit( node.Finally )!;
                }
            }

            return node.WithBlock( annotatedBlock )
                .WithCatches( List( annotatedCatches ) )
                .WithFinally( annotatedFinally! )
                .AddScopeAnnotation( TemplatingScope.RunTimeOnly );
        }

        public override SyntaxNode? VisitTypeOfExpression( TypeOfExpressionSyntax node )

            // typeof(.) is always compile-time.
            => node.AddScopeAnnotation( TemplatingScope.CompileTimeOnly );
    }
}