// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
            where T : notnull
        {
            var location = this._syntaxTreeAnnotationMap.GetLocation( targetNode );

            this.ReportDiagnostic( descriptor, location, arguments );
        }

        private void ReportDiagnostic<T>( DiagnosticDefinition<T> descriptor, Location? location, T arguments )
            where T : notnull
        {
            var diagnostic = descriptor.CreateDiagnostic( location, arguments );
            this._diagnosticAdder.Report( diagnostic );

            if ( diagnostic.Severity == DiagnosticSeverity.Error )
            {
                this.Success = false;
            }
        }

        private void SetLocalSymbolScope( ISymbol symbol, TemplatingScope scope )
        {
            if ( scope != TemplatingScope.CompileTimeOnly && scope != TemplatingScope.RunTimeOnly )
            {
                throw new ArgumentOutOfRangeException( nameof(scope) );
            }

            if ( this._localScopes.TryGetValue( symbol, out _ ) )
            {
                throw new AssertionFailedException( $"The symbol {symbol} was already assigned to the scope {scope}." );
            }

            this._localScopes.Add( symbol, scope );
        }

        /// <summary>
        /// Gets the scope of a symbol.
        /// </summary>
        /// <param name="symbol">A symbol.</param>
        /// <returns></returns>
        private TemplatingScope GetSymbolScope( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IDiscardSymbol:
                    return TemplatingScope.Both;

                // For local variables, we decide based on  _buildTimeLocals only. This collection is updated
                // at each iteration of the algorithm based on inferences from _requireMetaExpressionStack.
                case ILocalSymbol or INamedTypeSymbol { IsAnonymousType: true } when this._localScopes.TryGetValue( symbol, out var scope ):
                    return scope;

                // When a local variable is assigned to an anonymous type, the scope is unknown because the anonymous
                // type is visited after the variable identifier.
                case ILocalSymbol or INamedTypeSymbol { IsAnonymousType: true }:
                    return TemplatingScope.Unknown;

                case { ContainingType: { IsAnonymousType: true } containingType }:
                    return GetMoreSpecificScope( this.GetSymbolScope( containingType ) );

                case IParameterSymbol parameter when this._currentTemplateMember != null &&
                                                     (SymbolEqualityComparer.Default.Equals( parameter.ContainingSymbol, this._currentTemplateMember ) ||
                                                      (parameter.ContainingSymbol is IMethodSymbol { AssociatedSymbol: { } associatedSymbol }
                                                       && SymbolEqualityComparer.Default.Equals( this._currentTemplateMember, associatedSymbol ))):
                    // In the future, we may have parameters on the template parameters changing their meaning. However, now, all template
                    // parameters map to run-time parameters of the same name.

                    return TemplatingScope.RunTimeOnly;
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
                switch ( this._symbolScopeClassifier.GetTemplateInfo( symbol ).AttributeType )
                {
                    case TemplateAttributeType.Introduction:
                    case TemplateAttributeType.InterfaceMember:
                        return TemplatingScope.RunTimeOnly;

                    default:
                        return TemplatingScope.CompileTimeOnly;
                }
            }

            // For other symbols, we use the SymbolScopeClassifier.
            var templatingScope = this._symbolScopeClassifier.GetTemplatingScope( symbol );

            return GetMoreSpecificScope( templatingScope );

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
                return this._templateMemberClassifier.RequiresCompileTimeExecution( symbol ) || (symbol is ILocalSymbol local && local.Type.IsDynamic( true ))
                    ? TemplatingScope.CompileTimeOnlyReturningRuntimeOnly
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

            var parentExpressionScope = TemplatingScope.Unknown;

            if ( originalParent != null )
            {
                parentExpressionScope = this.GetExpressionTypeScope( originalParent );
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
                    case TemplatingScope.RunTimeOnly:
                    case TemplatingScope.Dynamic:
                    case TemplatingScope.CompileTimeOnlyReturningRuntimeOnly:
                        runtimeCount++;

                        break;

                    case TemplatingScope.CompileTimeOnly:
                    case TemplatingScope.CompileTimeOnlyReturningBoth:
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
                // However, Unknown wins, so we need do this test after we visit all nodes.
                return TemplatingScope.RunTimeOnly;
            }
            else if ( compileTimeOnlyCount > 0 )
            {
                switch ( parentExpressionScope )
                {
                    case TemplatingScope.Dynamic:
                    case TemplatingScope.RunTimeOnly:
                        return TemplatingScope.CompileTimeOnlyReturningRuntimeOnly;

                    case TemplatingScope.Both:
                        return TemplatingScope.CompileTimeOnlyReturningBoth;

                    default:
                        // Coverage: ignore (could not find a case).
                        return TemplatingScope.CompileTimeOnly;
                }
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
            if ( node == null )
            {
                return null;
            }

            this._cancellationToken.ThrowIfCancellationRequested();

            // Adds annotations to the children node.
            var transformedNode = base.Visit( node );

            return this.AddScopeAnnotationToVisitedNode( node, transformedNode );
        }

        /// <summary>
        /// Adds scope annotation to a node that has been visited - so children are annotated but not the node itself.
        /// </summary>
        private SyntaxNode AddScopeAnnotationToVisitedNode( SyntaxNode node, SyntaxNode visitedNode )
        {
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
                this.SetLocalSymbolScope( symbol, scope );
            }

            // Anonymous objects are currently run-time-only unless they are in a compile-time-only scope -- until we implement more complex rules.
            var transformedMembers =
                node.Initializers.Select( i => this.Visit( i ).AddScopeAnnotation( scope ) );

            return node.Update(
                    node.NewKeyword,
                    node.OpenBraceToken,
                    SeparatedList( transformedMembers, node.Initializers.GetSeparators() ),
                    node.CloseBraceToken )
                .AddScopeAnnotation( scope );
        }

        #endregion

        public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            => this.VisitTypeDeclaration( node, n => base.VisitClassDeclaration( n ) );

        public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node )
            => this.VisitTypeDeclaration( node, n => base.VisitStructDeclaration( n ) );

        public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            => this.VisitTypeDeclaration( node, n => base.VisitRecordDeclaration( n ) );

        public override SyntaxNode? VisitDelegateDeclaration( DelegateDeclarationSyntax node )
            => this.VisitTypeDeclaration( node, n => base.VisitDelegateDeclaration( n ) );

        public override SyntaxNode? VisitEnumDeclaration( EnumDeclarationSyntax node )
            => this.VisitTypeDeclaration( node, n => base.VisitEnumDeclaration( n ) );

        private T VisitTypeDeclaration<T>( T node, Func<T, SyntaxNode?> callBase )
            where T : SyntaxNode
        {
            var typeScope = this.GetSymbolScope( this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node ).AssertNotNull() );

            if ( typeScope != TemplatingScope.RunTimeOnly )
            {
                return ((T) callBase( node )!).AddScopeAnnotation( typeScope );
            }
            else
            {
                // This is not a build-time type so there's no need to analyze it.
                // The scope annotation is needed for syntax highlighting.
                return node.AddScopeAnnotation( typeScope );
            }
        }

        public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
        {
            var identifierNameSyntax = (IdentifierNameSyntax) base.VisitIdentifierName( node )!;
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

            if ( symbol != null )
            {
                var scope = this.GetSymbolScope( symbol );

                if ( scope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
                {
                    // Template code cannot be referenced in a template until this is implemented.
                    if ( this._symbolScopeClassifier.GetTemplateInfo( symbol ).AttributeType == TemplateAttributeType.Template )
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
                    // Coverage: ignore.
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

        public override SyntaxNode? VisitMemberBindingExpression( MemberBindingExpressionSyntax node )
        {
            var transformedName = this.Visit( node.Name );

            return node.WithName( transformedName ).WithScopeAnnotationFrom( transformedName );
        }

        public override SyntaxNode? VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
        {
            this.VisitAccessExceptionCore(
                node.Expression,
                node.Name,
                node.OperatorToken,
                out var transformedExpression,
                out var transformedName,
                out var transformedOperator,
                out var scope );

            return node.Update( transformedExpression, transformedOperator, (SimpleNameSyntax) transformedName ).AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitConditionalAccessExpression( ConditionalAccessExpressionSyntax node )
        {
            this.VisitAccessExceptionCore(
                node.Expression,
                node.WhenNotNull,
                node.OperatorToken,
                out var transformedExpression,
                out var transformedWhenNotNull,
                out var transformedOperator,
                out var scope );

            return node.Update( transformedExpression, transformedOperator, transformedWhenNotNull ).AddScopeAnnotation( scope );
        }

        private void VisitAccessExceptionCore(
            ExpressionSyntax left,
            ExpressionSyntax right,
            SyntaxToken operatorToken,
            out ExpressionSyntax transformedLeft,
            out ExpressionSyntax transformedRight,
            out SyntaxToken transformedOperator,
            out TemplatingScope scope )
        {
            transformedRight = this.Visit( right );

            var nameScope = this.GetNodeScope( transformedRight );
            ScopeContext? context = null;
            scope = TemplatingScope.Both;

            switch ( nameScope )
            {
                case TemplatingScope.CompileTimeOnly:
                case TemplatingScope.CompileTimeOnlyReturningBoth:
                case TemplatingScope.CompileTimeOnlyReturningRuntimeOnly:
                    // If the member is compile-time (because of rules on the symbol), the expression on the left MUST be compile-time.
                    context = ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"a compile-time-only member '{right}'" );
                    scope = nameScope;

                    break;

                case TemplatingScope.Dynamic:
                    // A member is run-time dynamic because the left part is dynamic, so there is no need to force it run-time.
                    // It can actually contain build-time subexpressions.
                    scope = TemplatingScope.Dynamic;

                    break;

                case TemplatingScope.Unknown when this._syntaxTreeAnnotationMap.GetExpressionType( left ) is IDynamicTypeSymbol:
                    // This is a member access of a dynamic receiver.
                    scope = TemplatingScope.RunTimeOnly;
                    context = ScopeContext.CreatePreferredRunTimeScope( this._currentScopeContext, $"a member of the run-time-only '{right}'" );

                    break;

                case TemplatingScope.RunTimeOnly:
                    scope = TemplatingScope.RunTimeOnly;

                    break;
            }

            using ( this.WithScopeContext( context ) )
            {
                transformedLeft = this.Visit( left );

                if ( scope == TemplatingScope.Both )
                {
                    scope = this.GetNodeScope( transformedLeft );
                }

                // If both sides of the member are template keywords, display the . as a template keyword too.
                transformedOperator = operatorToken;

                if ( transformedLeft.GetColorFromAnnotation() == TextSpanClassification.TemplateKeyword &&
                     transformedRight.GetColorFromAnnotation() == TextSpanClassification.TemplateKeyword )
                {
                    transformedOperator = transformedOperator.AddColoringAnnotation( TextSpanClassification.TemplateKeyword );
                }
            }
        }

        public override SyntaxNode? VisitElementAccessExpression( ElementAccessExpressionSyntax node )
        {
            // In an element access (such as Tags[x]), the scope is given by the expression.

            var transformedExpression = this.Visit( node.Expression );
            var scope = this.GetNodeScope( transformedExpression );

            ScopeContext? context;

            if ( scope == TemplatingScope.CompileTimeOnly )
            {
                context = ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"element of the compile-time collection '{node.Expression}'" );
            }
            else if ( scope.IsDynamic() )
            {
                scope = TemplatingScope.Dynamic;

                context = ScopeContext.CreatePreferredRunTimeScope( this._currentScopeContext, $"element of the run-time-only collection '{node.Expression}'" );
            }
            else
            {
                context = null;
            }

            BracketedArgumentListSyntax transformedArguments;

            using ( this.WithScopeContext( context ) )
            {
                transformedArguments = this.Visit( node.ArgumentList );
            }

            return node.Update( transformedExpression, transformedArguments ).AddScopeAnnotation( scope );
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
                transformedExpression = this.Visit( node.Expression );
            }

            var expressionScope = this.GetNodeScope( transformedExpression );

            ImmutableArray<IParameterSymbol> parameters;

            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node.Expression );

            switch ( symbol )
            {
                case IMethodSymbol method:
                    parameters = method.Parameters;

                    break;

                default:
                    var expressionType = this._syntaxTreeAnnotationMap.GetExpressionType( node.Expression );
                    
                    switch ( expressionType )
                    {
                        case null when symbol == null:
                            // This seems to happen when one of the argument is dynamic.
                            // Roslyn then stops doing the symbol analysis for the whole downstream syntax tree.
                            parameters = default;
                            
                            break;
                        
                        case { TypeKind: TypeKind.Delegate }:
                            parameters = ((INamedTypeSymbol) expressionType).Constructors.Single().Parameters;

                            break;

                        case { TypeKind: TypeKind.Error }:
                            return node;

                        case { TypeKind: TypeKind.Dynamic }:
                            // In case of invocation of a dynamic location, there is no list of parameters, only arguments.
                            parameters = default;

                            break;

                        default:
                            throw new AssertionFailedException( $"Don't know how to get the parameters of '{node.Expression}'." );
                    }

                    break;
            }

            InvocationExpressionSyntax updatedInvocation;

            if ( expressionScope.GetExpressionExecutionScope() != TemplatingScope.Both )
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

                    ExpressionSyntax transformedArgumentValue;

                    // Transform the argument value.
                    if ( expressionScope.IsDynamic() || argumentType.IsDynamic() )
                    {
                        // dynamic or dynamic[]

                        using ( this.WithScopeContext(
                            ScopeContext.CreatePreferredRunTimeScope(
                                this._currentScopeContext,
                                $"argument of the dynamic parameter '{parameter?.Name ?? argumentIndex.ToString( CultureInfo.InvariantCulture )}'" ) ) )
                        {
                            transformedArgumentValue = this.Visit( argument.Expression );
                        }
                    }
                    else if ( expressionScope.IsRunTime() )
                    {
                        using ( this.WithScopeContext(
                            ScopeContext.CreatePreferredRunTimeScope(
                                this._currentScopeContext,
                                $"argument of the run-time method '{node.Expression}'" ) ) )
                        {
                            transformedArgumentValue = this.Visit( argument.Expression );
                        }
                    }
                    else
                    {
                        using ( this.WithScopeContext(
                            ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"a compile-time expression '{node.Expression}'" ) ) )
                        {
                            transformedArgumentValue = this.Visit( argument.Expression );
                        }
                    }

                    // The scope of the argument itself copies the scope of the method.
                    var transformedArgument = argument.WithExpression( transformedArgumentValue )
                        .WithTriviaFrom( argument )
                        .AddScopeAnnotation( expressionScope );

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
            // We don't add an annotation to the argument because it needs to be inherited from the parent.
            var transformedExpression = this.Visit( node.Expression );

            return node.WithExpression( transformedExpression );
        }

        public override SyntaxNode? VisitIfStatement( IfStatementSyntax node )
        {
            var annotatedCondition = this.Visit( node.Condition );
            var conditionScope = this.GetNodeScope( annotatedCondition );

            TemplatingScope ifScope;
            StatementSyntax annotatedStatement;
            StatementSyntax? annotatedElseStatement;
            ScopeContext? scopeContext;

            if ( conditionScope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
            {
                // We have an if statement where the condition is a compile-time expression. Add annotations
                // to the if and else statements but not to the blocks themselves.

                scopeContext = null;
                ifScope = TemplatingScope.CompileTimeOnly;
            }
            else
            {
                // We have an if statement where the condition is a runtime expression. Any variable assignment
                // within this statement should make the variable as runtime-only, so we're calling EnterRuntimeConditionalBlock.

                scopeContext = ScopeContext.CreateRuntimeConditionalScope( this._currentScopeContext, "if ( " + node.Condition + " )" );
                ifScope = TemplatingScope.RunTimeOnly;
            }

            using ( this.WithScopeContext( scopeContext ) )
            {
                // Statements of a compile-time control block must have an explicitly-set scope otherwise the template compiler
                // will look at the scope in the parent node, which is here incorrect.
                annotatedStatement = this.Visit( node.Statement ).AddRunTimeOnlyAnnotationIfUndetermined();
                annotatedElseStatement = this.Visit( node.Else?.Statement )?.AddRunTimeOnlyAnnotationIfUndetermined();
            }

            return node.Update(
                    node.AttributeLists,
                    node.IfKeyword,
                    node.OpenParenToken,
                    annotatedCondition.AddTargetScopeAnnotation( ifScope ),
                    node.CloseParenToken,
                    annotatedStatement,
                    node.Else?.Update( node.Else.ElseKeyword, annotatedElseStatement! ).AddScopeAnnotation( ifScope ) )
                .AddScopeAnnotation( ifScope );
        }

        public override SyntaxNode? VisitBreakStatement( BreakStatementSyntax node )
            => node.AddScopeAnnotation( this._currentScopeContext.CurrentBreakOrContinueScope );

        public override SyntaxNode? VisitContinueStatement( ContinueStatementSyntax node )
            => node.AddScopeAnnotation( this._currentScopeContext.CurrentBreakOrContinueScope );

        public override SyntaxNode? VisitForEachStatement( ForEachStatementSyntax node )
        {
            var local = (ILocalSymbol) this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node )!;

            var annotatedExpression = this.Visit( node.Expression );

            TemplatingScope forEachScope;
            string reason;

            if ( node.AwaitKeyword.Kind() == SyntaxKind.None )
            {
                forEachScope = this.GetNodeScope( annotatedExpression ).GetExpressionValueScope( true ).ReplaceIndeterminate( TemplatingScope.RunTimeOnly );
                reason = $"foreach ( {node.Type} {node.Identifier} in ... )";
            }
            else
            {
                forEachScope = TemplatingScope.RunTimeOnly;
                reason = $"await foreach ( {node.Type} {node.Identifier} in ... )";
            }

            this.SetLocalSymbolScope( local, forEachScope );

            this.RequireLoopScope( node.Expression, forEachScope, "foreach" );

            StatementSyntax annotatedStatement;

            using ( this.WithScopeContext( ScopeContext.CreateBreakOrContinueScope( this._currentScopeContext, forEachScope, reason ) ) )
            {
                // Statements of a compile-time control block must have an explicitly-set scope otherwise the template compiler
                // will look at the scope in the parent node, which is here incorrect.
                annotatedStatement = this.Visit( node.Statement ).AddRunTimeOnlyAnnotationIfUndetermined();
            }

            var identifierClassification = forEachScope == TemplatingScope.CompileTimeOnly ? TextSpanClassification.CompileTimeVariable : default;

            var transformedNode =
                ForEachStatement(
                        node.AwaitKeyword,
                        node.ForEachKeyword,
                        node.OpenParenToken,
                        node.Type.AddTargetScopeAnnotation( forEachScope ),
                        node.Identifier.AddColoringAnnotation( identifierClassification ),
                        node.InKeyword,
                        annotatedExpression.AddTargetScopeAnnotation( forEachScope ),
                        node.CloseParenToken,
                        annotatedStatement.AddTargetScopeAnnotation( forEachScope ) )
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
                transformedDesignation = this.Visit( node.Designation );
            }

            return node.Update( transformedType, transformedDesignation ).AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitIsPatternExpression( IsPatternExpressionSyntax node )
        {
            // The scope of a pattern expression is given by the expression (left part).
            var transformedExpression = this.Visit( node.Expression );
            var scope = this.GetNodeScope( transformedExpression ).GetExpressionValueScope();

            var context = scope == TemplatingScope.CompileTimeOnly
                ? ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"pattern on the compile-time expression '{node.Expression}'" )
                : null;

            PatternSyntax transformedPattern;

            using ( this.WithScopeContext( context ) )
            {
                transformedPattern = this.Visit( node.Pattern );
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
                this.SetLocalSymbolScope( symbol, scope );

                if ( scope == TemplatingScope.CompileTimeOnly )
                {
                    color = TextSpanClassification.CompileTimeVariable;
                }
            }

            var coloredIdentifier = node.Identifier.AddColoringAnnotation( color );
            var transformedNode = node.WithIdentifier( coloredIdentifier ).AddScopeAnnotation( scope );

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
                    localScope = this.GetNodeScope( transformedInitializer.Value )
                        .GetExpressionValueScope( true )
                        .ReplaceIndeterminate( TemplatingScope.RunTimeOnly )
                        .GetExpressionValueScope();
                }
                else
                {
                    // Variables without initializer have runtime scope.
                    localScope = TemplatingScope.RunTimeOnly;
                }
            }

            // Mark the local variable symbol.
            this.SetLocalSymbolScope( local, localScope );

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
                    transformedArguments = node.ArgumentList.Arguments.Select( a => this.Visit( a ) ).ToArray();
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
            var transformedType = this.Visit( node.Type );

            if ( this._templateMemberClassifier.IsDynamicType( transformedType )
                 && !(node.Type is IdentifierNameSyntax { Identifier: { Text: "var" } }) )
            {
                foreach ( var variable in node.Variables.Where( v => v.Initializer == null ) )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.CannotUseDynamicInUninitializedLocal,
                        variable.Identifier,
                        variable.Identifier.Text );
                }
            }

            if ( this.GetNodeScope( transformedType ) == TemplatingScope.CompileTimeOnly )
            {
                using ( this.WithScopeContext(
                    ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"a local variable of compile-time-only type '{node.Type}'" ) ) )
                {
                    // ReSharper disable once RedundantSuppressNullableWarningExpression
                    var transformedVariables = node.Variables.Select( v => this.Visit( v )! );

                    return node.Update( transformedType, SeparatedList( transformedVariables ) ).AddScopeAnnotation( TemplatingScope.CompileTimeOnly );
                }
            }
            else
            {
                // ReSharper disable once RedundantSuppressNullableWarningExpression
                var transformedVariables = node.Variables.Select( v => this.Visit( v )! ).ToList();

                var variableScopes = transformedVariables.Select( v => v.GetScopeFromAnnotation() ).Distinct().ToList();

                if ( variableScopes.Count != 1 )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.SplitVariables,
                        node,
                        string.Join( ",", node.Variables.Select( v => "'" + v.Identifier.Text + "'" ) ) );
                }

                return node.Update( transformedType, SeparatedList( transformedVariables ) ).AddScopeAnnotation( variableScopes.Single() );
            }
        }

        public override SyntaxNode? VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
        {
            var transformedNode = (LocalDeclarationStatementSyntax) base.VisitLocalDeclarationStatement( node )!;

            return transformedNode.AddScopeAnnotation( this.GetNodeScope( transformedNode.Declaration ) );
        }

        #endregion

        public override SyntaxNode? VisitAttribute( AttributeSyntax node )
            =>

                // Don't process attributes.
                node;

        private T? VisitMemberDeclaration<T>( T node, Func<T, SyntaxNode?> callBase )
            where T : SyntaxNode
        {
            var symbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node )!;

            // Detect if the current member is a template.
            var isTemplate = !this._symbolScopeClassifier.GetTemplateInfo( symbol ).IsNone
                             || (symbol is IMethodSymbol { AssociatedSymbol: { } associatedSymbol }
                                 && !this._symbolScopeClassifier.GetTemplateInfo( associatedSymbol ).IsNone);

            // If it is a template, update the currentTemplateMember field.
            if ( isTemplate )
            {
                var previousTemplateMember = this._currentTemplateMember;
                this._currentTemplateMember = symbol;

                try
                {
                    return (T) callBase( node )!.AddIsTemplateAnnotation();
                }
                finally
                {
                    this._currentTemplateMember = previousTemplateMember;
                }
            }
            else
            {
                return (T?) callBase( node );
            }
        }

        public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            => this.VisitMemberDeclaration( node, n => base.VisitMethodDeclaration( n ) );

        public override SyntaxNode? VisitAccessorDeclaration( AccessorDeclarationSyntax node )
            => this.VisitMemberDeclaration( node, n => base.VisitAccessorDeclaration( n ) );

        public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            => this.VisitMemberDeclaration( node, n => base.VisitPropertyDeclaration( n ) );

        public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
            => this.VisitMemberDeclaration( node, n => base.VisitEventDeclaration( n ) );

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
            var transformedOperand = this.Visit( operand );

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
            var transformedLeft = this.Visit( node.Left );

            var scope = this.GetNodeScope( transformedLeft ).GetExpressionValueScope();
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
                transformedRight = this.Visit( node.Right );
            }

            // If we have a discard assignment, take the scope from the right.
            if ( scope == TemplatingScope.Both
                 && this._syntaxTreeAnnotationMap.GetSymbol( node.Left ) is IDiscardSymbol )
            {
                scope = this.GetNodeScope( transformedRight );
            }

            return node.Update( transformedLeft, node.OperatorToken, transformedRight ).AddScopeAnnotation( scope );
        }

        public override SyntaxNode? VisitExpressionStatement( ExpressionStatementSyntax node )
        {
            var transformedExpression = this.Visit( node.Expression );
            var expressionScope = this.GetNodeScope( transformedExpression );
            var statementScope = expressionScope.GetExpressionExecutionScope();

            return node.WithExpression( transformedExpression ).AddScopeAnnotation( expressionScope ).AddTargetScopeAnnotation( statementScope );
        }

        public override SyntaxNode? VisitCastExpression( CastExpressionSyntax node )
        {
            TypeSyntax annotatedType;
            var annotatedExpression = this.Visit( node.Expression );
            var expressionScope = this.GetNodeScope( annotatedExpression );
            TemplatingScope castScope;

            if ( expressionScope.GetExpressionValueScope() == TemplatingScope.RunTimeOnly )
            {
                // The whole cast is run-time only.
                using ( this.WithScopeContext(
                    ScopeContext.CreatePreferredRunTimeScope(
                        this._currentScopeContext,
                        $"cast of the run-time-only expression '{node.Expression}'" ) ) )
                {
                    annotatedType = this.Visit( node.Type );
                }

                castScope = TemplatingScope.RunTimeOnly;
            }
            else
            {
                annotatedType = this.Visit( node.Type );
                castScope = expressionScope;
            }

            return node.Update( node.OpenParenToken, annotatedType, node.CloseParenToken, annotatedExpression )
                .AddScopeAnnotation( castScope );
        }

        public override SyntaxNode? VisitBinaryExpression( BinaryExpressionSyntax node )
        {
            switch ( node.Kind() )
            {
                case SyntaxKind.IsExpression:
                case SyntaxKind.AsExpression:
                    var annotatedType = (TypeSyntax) this.Visit( node.Right );
                    var annotatedExpression = this.Visit( node.Left );
                    var transformedNode = node.WithLeft( annotatedExpression ).WithRight( annotatedType );

                    return this.AnnotateCastExpression( transformedNode, annotatedType, annotatedExpression );

                case SyntaxKind.CoalesceExpression:
                    return this.VisitCoalesceExpression( node );
            }

            var visitedNode = base.VisitBinaryExpression( node );

            return this.AddScopeAnnotationToVisitedNode( node, visitedNode );
        }

        private SyntaxNode? AnnotateCastExpression( SyntaxNode transformedCastNode, TypeSyntax annotatedType, ExpressionSyntax annotatedExpression )
        {
            // TODO: Verify
            var combinedScope = this.GetNodeScope( annotatedType ) == TemplatingScope.Both
                ? this.GetNodeScope( annotatedExpression ).GetExpressionValueScope()
                : this.GetExpressionScope( new[] { annotatedExpression }, transformedCastNode );

            if ( combinedScope != TemplatingScope.Both )
            {
                return transformedCastNode.AddScopeAnnotation( combinedScope );
            }

            return transformedCastNode;
        }

        private SyntaxNode? VisitCoalesceExpression( BinaryExpressionSyntax node )
        {
            // The scope is determined by the left part. The right part must follow.

            var annotatedLeft = this.Visit( node.Left );
            var leftScope = this.GetNodeScope( annotatedLeft );

            ScopeContext context;

            if ( leftScope == TemplatingScope.CompileTimeOnly )
            {
                context = ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, $"right part of the compile-time '{node.Left} ??'" );
            }
            else if ( leftScope.IsRunTime() )
            {
                context = ScopeContext.CreatePreferredRunTimeScope( this._currentScopeContext, $"right part of the run-time '{node.Left} ??'" );
                leftScope = TemplatingScope.RunTimeOnly;
            }
            else
            {
                // Use the default rule.
                var visitedNode = base.VisitBinaryExpression( node )!;

                return this.AddScopeAnnotationToVisitedNode( node, visitedNode );
            }

            using ( this.WithScopeContext( context ) )
            {
                var annotatedRight = this.Visit( node.Right );

                return node.Update( annotatedLeft, node.OperatorToken, annotatedRight ).AddScopeAnnotation( leftScope );
            }
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
                transformedStatement = this.Visit( node.Statement );
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

            var annotatedCondition = this.Visit( node.Condition );
            var conditionScope = this.GetNodeScope( annotatedCondition );

            this.RequireLoopScope( node.Condition, conditionScope, "while" );

            StatementSyntax annotatedStatement;

            using ( this.WithScopeContext(
                ScopeContext.CreateBreakOrContinueScope( this._currentScopeContext, conditionScope, $"while ( {node.Condition} )" ) ) )
            {
                annotatedStatement = this.Visit( node.Statement ).ReplaceScopeAnnotation( conditionScope );
            }

            return node.Update(
                    node.AttributeLists,
                    node.WhileKeyword,
                    node.OpenParenToken,
                    annotatedCondition,
                    node.CloseParenToken,
                    annotatedStatement )
                .AddScopeAnnotation( conditionScope );
        }

        public override SyntaxNode? VisitReturnStatement( ReturnStatementSyntax node )
            => base.VisitReturnStatement( node )!.AddScopeAnnotation( TemplatingScope.RunTimeOnly );

        #region Unsupported Features

        private void ReportUnsupportedLanguageFeature( SyntaxNodeOrToken nodeForDiagnostic, string featureName )
            => this.ReportDiagnostic( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, nodeForDiagnostic, featureName );

        public override SyntaxNode? VisitDoStatement( DoStatementSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node.DoKeyword, "do" );

            return base.VisitDoStatement( node );
        }

        public override SyntaxNode? VisitUnsafeStatement( UnsafeStatementSyntax node )
        {
            this.ReportUnsupportedLanguageFeature( node.UnsafeKeyword, "unsafe" );

            return base.VisitUnsafeStatement( node );
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
            // Await is always run-time.

            ExpressionSyntax transformedExpression;

            using ( this.WithScopeContext( ScopeContext.CreatePreferredRunTimeScope( this._currentScopeContext, "'await' expression" ) ) )
            {
                transformedExpression = this.Visit( node.Expression );
            }

            return node.WithExpression( transformedExpression ).AddScopeAnnotation( TemplatingScope.RunTimeOnly );
        }

        public override SyntaxNode? VisitYieldStatement( YieldStatementSyntax node )
        {
            // Yield is always run-time.

            ExpressionSyntax? transformedExpression;

            if ( node.Expression != null )
            {
                using ( this.WithScopeContext( ScopeContext.CreatePreferredRunTimeScope( this._currentScopeContext, "'yield' expression" ) ) )
                {
                    transformedExpression = this.Visit( node.Expression );
                }
            }
            else
            {
                transformedExpression = null;
            }

            return node.WithExpression( transformedExpression ).AddScopeAnnotation( TemplatingScope.RunTimeOnly );
        }

        #endregion

        #region Lambda expressions

        public override SyntaxNode? VisitParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node )
        {
            if ( node.ExpressionBody != null )
            {
                var annotatedExpression = this.Visit( node.ExpressionBody );

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
                var annotatedExpression = this.Visit( node.ExpressionBody );

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
            var transformedPattern = this.Visit( node.Pattern );
            var patternScope = this.GetNodeScope( transformedPattern );

            var transformedWhen = this.Visit( node.WhenClause );
            var transformedExpression = this.Visit( node.Expression );

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
            var transformedGoverningExpression = this.Visit( node.GoverningExpression );
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
            var annotatedExpression = this.Visit( node.Expression );
            var expressionScope = annotatedExpression.GetScopeFromAnnotation().GetValueOrDefault();

            TemplatingScope switchScope;
            string scopeReason;

            if ( (expressionScope == TemplatingScope.CompileTimeOnly && this._templateMemberClassifier.IsDynamicType( annotatedExpression ))
                 || expressionScope != TemplatingScope.CompileTimeOnly )
            {
                switchScope = TemplatingScope.RunTimeOnly;
                scopeReason = $"the run-time 'switch( {node.Expression} )'";
            }
            else
            {
                switchScope = TemplatingScope.CompileTimeOnly;
                scopeReason = $"the compile-time 'switch( {node.Expression} )'";
            }

            var transformedSections = new SwitchSectionSyntax[node.Sections.Count];

            for ( var i = 0; i < node.Sections.Count; i++ )
            {
                var section = node.Sections[i];

                SwitchLabelSyntax[] transformedLabels;
                StatementSyntax[] transformedStatements;

                var labelContext = switchScope == TemplatingScope.CompileTimeOnly
                    ? ScopeContext.CreateForcedCompileTimeScope( this._currentScopeContext, scopeReason )
                    : ScopeContext.CreatePreferredRunTimeScope( this._currentScopeContext, scopeReason );

                using ( this.WithScopeContext( labelContext ) )
                {
                    transformedLabels = section.Labels.Select( l => this.Visit( l ) ).ToArray();

                    if ( this.RequireScope( transformedLabels, switchScope, scopeReason ) )
                    {
                        transformedLabels = transformedLabels.Select( l => l.ReplaceScopeAnnotation( switchScope ) ).ToArray();
                    }
                    else
                    {
                        // We would have an error if we replace the annotation.
                    }
                }

                using ( this.WithScopeContext( ScopeContext.CreateBreakOrContinueScope( this._currentScopeContext, switchScope, scopeReason ) ) )
                {
                    // Statements of a compile-time control block must have an explicitly-set scope otherwise the template compiler
                    // will look at the scope in the parent node, which is here incorrect.
                    transformedStatements = section.Statements.Select( s => this.Visit( s ).AddRunTimeOnlyAnnotationIfUndetermined() ).ToArray();
                }

                transformedSections[i] = section.Update( List( transformedLabels ), List( transformedStatements ) ).AddScopeAnnotation( switchScope );
            }

            return node.Update(
                    node.SwitchKeyword,
                    node.OpenParenToken,
                    annotatedExpression.ReplaceScopeAnnotation( switchScope ),
                    node.CloseParenToken,
                    node.OpenBraceToken,
                    List( transformedSections ),
                    node.CloseBraceToken )
                .AddScopeAnnotation( switchScope );
        }

        private TemplatingScope GetSwitchCaseScope(
            SyntaxNode transformedPattern,
            SyntaxNode? transformedWhen,
            SyntaxNode? transformedExpression = null )
            => this.GetExpressionScope( new[] { transformedPattern, transformedWhen, transformedExpression } );

        public override SyntaxNode? VisitCasePatternSwitchLabel( CasePatternSwitchLabelSyntax node )
        {
            var transformedPattern = this.Visit( node.Pattern );
            var patternScope = this.GetNodeScope( transformedPattern );
            var transformedWhen = this.Visit( node.WhenClause );

            var combinedScope = patternScope == TemplatingScope.CompileTimeOnly
                ? TemplatingScope.CompileTimeOnly
                : this.GetSwitchCaseScope( transformedPattern, transformedWhen );

            return node.Update( node.Keyword, transformedPattern, transformedWhen, node.ColonToken ).AddScopeAnnotation( combinedScope );
        }

        #endregion

        private bool RequireScope( IEnumerable<SyntaxNode> nodes, TemplatingScope requiredScope, string reason )
        {
            foreach ( var node in nodes )
            {
                if ( !this.RequireScope( node, requiredScope, reason ) )
                {
                    return false;
                }
            }

            return true;
        }

        private bool RequireScope( SyntaxNode? node, TemplatingScope requiredScope, string reason )
            => this.RequireScope( node, this.GetNodeScope( node ), requiredScope, reason );

        private bool RequireScope( SyntaxNode? node, TemplatingScope existingScope, TemplatingScope requiredScope, string reason )
        {
            if ( node == null )
            {
                return true;
            }

            existingScope = existingScope.GetExpressionValueScope();
            requiredScope = requiredScope.GetExpressionExecutionScope();

            if ( existingScope != TemplatingScope.Both && existingScope != requiredScope )
            {
                // Don't emit an error if any descendant node already has an error because this creates redundant messages.
                if ( !node.DescendantNodes().Any( n => n.HasScopeMismatchAnnotation() ) )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.ScopeMismatch,
                        node,
                        (node.ToString(), existingScope.ToDisplayString(), requiredScope.ToDisplayString(), reason) );

                    return false;
                }
            }

            return true;
        }

        private void RequireLoopScope( SyntaxNode nodeForDiagnostic, TemplatingScope requiredScope, string statementName )
        {
            if ( requiredScope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly && this._currentScopeContext.IsRuntimeConditionalBlock )
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
            var annotatedExpression = this.Visit( node.Expression );
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

            if ( scope == TemplatingScope.Conflict )
            {
                this.ReportDiagnostic( TemplatingDiagnosticDescriptors.ScopeConflict, node, node.ToString() );

                // We continue with an unknown scope because other methods don't handle the Conflict scope.
                scope = TemplatingScope.Unknown;
            }

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

            if ( objectType == null )
            {
                throw new AssertionFailedException( $"Cannot get the expression type for '{node}'." );
            }

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
                var transformedArguments = node.ArgumentList?.Arguments.Select( a => this.Visit( a ) ).ToArray();
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
                var transformedExpression = this.Visit( node.Expression );

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
            var annotatedBlock = this.Visit( node.Block );

            var annotatedCatches = new CatchClauseSyntax[node.Catches.Count];

            for ( var i = 0; i < node.Catches.Count; i++ )
            {
                var @catch = node.Catches[i];

                using ( this.WithScopeContext( ScopeContext.CreateRuntimeConditionalScope( this._currentScopeContext, "catch" ) ) )
                {
                    var annotatedCatch = this.Visit( @catch );
                    annotatedCatches[i] = annotatedCatch;
                }
            }

            FinallyClauseSyntax? annotatedFinally = null;

            if ( node.Finally != null )
            {
                using ( this.WithScopeContext( ScopeContext.CreateRuntimeConditionalScope( this._currentScopeContext, "finally" ) ) )
                {
                    annotatedFinally = this.Visit( node.Finally );
                }
            }

            return node.WithBlock( annotatedBlock )
                .WithCatches( List( annotatedCatches ) )
                .WithFinally( annotatedFinally! )
                .AddScopeAnnotation( TemplatingScope.RunTimeOnly );
        }

        public override SyntaxNode? VisitCatchDeclaration( CatchDeclarationSyntax node )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node );

            if ( symbol != null )
            {
                this.SetLocalSymbolScope( symbol, TemplatingScope.RunTimeOnly );
            }

            return base.VisitCatchDeclaration( node );
        }

        public override SyntaxNode? VisitTypeOfExpression( TypeOfExpressionSyntax node )

            // typeof(.) is always compile-time.
            => node.AddScopeAnnotation( TemplatingScope.CompileTimeOnly );

        public override SyntaxNode? VisitArrayRankSpecifier( ArrayRankSpecifierSyntax node )
        {
            var transformedSizes = node.Sizes.Select( syntax => this.Visit( syntax ) ).ToList();
            var sizeScope = this.GetExpressionScope( transformedSizes, node );

            var arrayRankScope = sizeScope.GetExpressionValueScope() switch
            {
                TemplatingScope.RunTimeOnly => TemplatingScope.RunTimeOnly,
                TemplatingScope.CompileTimeOnly => TemplatingScope.Both,
                TemplatingScope.Both => TemplatingScope.Both,
                _ => throw new AssertionFailedException( $"Unexpected scope: {sizeScope}." )
            };

            var transformedRank =
                node.WithSizes( SeparatedList( transformedSizes ) ).AddScopeAnnotation( arrayRankScope );

            return transformedRank;
        }

        public override SyntaxNode? VisitThisExpression( ThisExpressionSyntax node )
        {
            if ( node.Parent is MemberAccessExpressionSyntax )
            {
                return base.VisitThisExpression( node );
            }
            else
            {
                return node.AddScopeAnnotation( TemplatingScope.CompileTimeOnly );
            }
        }
    }
}