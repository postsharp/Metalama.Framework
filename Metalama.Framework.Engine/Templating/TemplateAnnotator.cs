// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.CompileTime.Manifest;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Utilities.Roslyn;
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

namespace Metalama.Framework.Engine.Templating;

/// <summary>
/// A <see cref="CSharpSyntaxRewriter"/> that adds annotation that distinguish compile-time from
/// run-time syntax nodes. The input should be a syntax tree annotated with a <see cref="SyntaxTreeAnnotationMap"/>.
/// </summary>
internal sealed partial class TemplateAnnotator : SafeSyntaxRewriter, IDiagnosticAdder
{
    private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;
    private readonly IDiagnosticAdder _diagnosticAdder;
    private readonly SerializableTypes _serializableTypes;
    private readonly CancellationToken _cancellationToken;
    private readonly TemplateMemberClassifier _templateMemberClassifier;
    private readonly TypeParameterDetectionVisitor _typeParameterDetectionVisitor;
    private readonly TemplateProjectManifestBuilder? _templateProjectManifestBuilder;

    /// <summary>
    /// Scope of locally-defined symbols (local variables, anonymous types, ....).
    /// </summary>
    private readonly Dictionary<ISymbol, TemplatingScope> _localScopes = new( SymbolEqualityComparer.Default );

    private readonly ISymbolClassifier _symbolScopeClassifier;

    private ScopeContext _currentScopeContext;

    private ISymbol? _currentTemplateMember;

    public TemplateAnnotator(
        ClassifyingCompilationContext compilationContext,
        SyntaxTreeAnnotationMap syntaxTreeAnnotationMap,
        IDiagnosticAdder diagnosticAdder,
        SerializableTypes serializableTypes,
        TemplateProjectManifestBuilder? templateProjectManifestBuilder,
        CancellationToken cancellationToken )
    {
        this._symbolScopeClassifier = compilationContext.SymbolClassifier;
        this._syntaxTreeAnnotationMap = syntaxTreeAnnotationMap;
        this._diagnosticAdder = diagnosticAdder;
        this._serializableTypes = serializableTypes;
        this._cancellationToken = cancellationToken;
        this._templateProjectManifestBuilder = templateProjectManifestBuilder;

        this._templateMemberClassifier = new TemplateMemberClassifier( compilationContext, syntaxTreeAnnotationMap );
        this._typeParameterDetectionVisitor = new TypeParameterDetectionVisitor( this );

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
        var diagnostic = descriptor.CreateRoslynDiagnostic( location, arguments );
        this._diagnosticAdder.Report( diagnostic );

        if ( diagnostic.Severity == DiagnosticSeverity.Error )
        {
            this.Success = false;
        }
    }

    void IDiagnosticAdder.Report( Diagnostic diagnostic )
    {
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

        if ( scope == TemplatingScope.CompileTimeOnly )
        {
            scope = this.FixCompileTimeReturningBothScopeWithSerializers( symbol );
        }

        this._localScopes.Add( symbol, scope );
    }

    private TemplatingScope FixCompileTimeReturningBothScopeWithSerializers( ISymbol symbol )
    {
        var valueType = symbol.GetExpressionType();

        if ( valueType != null )
        {
            var valueScope = this._symbolScopeClassifier.GetTemplatingScope( valueType );

            return valueScope switch
            {
                TemplatingScope.RunTimeOrCompileTime when this._serializableTypes.IsSerializable( valueType ) => TemplatingScope
                    .CompileTimeOnlyReturningBoth,
                TemplatingScope.RunTimeOrCompileTime => TemplatingScope.CompileTimeOnly,
                TemplatingScope.CompileTimeOnly => TemplatingScope.CompileTimeOnly,

                // This should not happen in valid code., but we can have RuntimeOnly in case of invalid code.
                // In this case, an error is emitted.
                // However, the the continuation of the control flow, we fall back to this value anyway.
                _ => TemplatingScope.CompileTimeOnlyReturningRuntimeOnly
            };
        }

        return TemplatingScope.CompileTimeOnly;
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
                return TemplatingScope.RunTimeOrCompileTime;

            // For local variables, we decide based on  _buildTimeLocals only. This collection is updated
            // at each iteration of the algorithm based on inferences from _requireMetaExpressionStack.
            case ILocalSymbol or INamedTypeSymbol { IsAnonymousType: true } when this._localScopes.TryGetValue( symbol, out var scope ):
                return scope;

            // When a local variable is assigned to an anonymous type, the scope is unknown because the anonymous
            // type is visited after the variable identifier.
            case ILocalSymbol or INamedTypeSymbol { IsAnonymousType: true }:
                return TemplatingScope.LateBound;

            case { ContainingType: { IsAnonymousType: true } containingType }:
                return GetMoreSpecificScope( this.GetSymbolScope( containingType ) );

            // Template parameters are always evaluated at compile-time, but run-time template parameters return a run-time value.
            case IParameterSymbol templateParameter when TemplateMemberSymbolClassifier.IsTemplateParameter( templateParameter ):
                var parameterScope = this._symbolScopeClassifier.GetTemplatingScope( templateParameter );

                return parameterScope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly
                    ? parameterScope
                    : TemplatingScope.RunTimeTemplateParameter;

            // Template type parameters can be run-time or compile-time. If a template type parameter is not marked as compile-time, it is run-time (there is no scope-neutral).
            case ITypeParameterSymbol typeParameter when TemplateMemberSymbolClassifier.IsTemplateTypeParameter( typeParameter ):
                var typeParameterScope = this._symbolScopeClassifier.GetTemplatingScope( typeParameter );

                return typeParameterScope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly
                    ? typeParameterScope
                    : TemplatingScope.RunTimeOnly;

            case IMethodSymbol { MethodKind: MethodKind.LocalFunction }:
                return TemplatingScope.RunTimeOnly;

            case IMethodSymbol method when this._templateMemberClassifier.IsRunTimeMethod( method ):
                // The TemplateContext.runTime method must be processed separately. It is a compile-time-only method whose
                // return is run-time-only.

                return TemplatingScope.RunTimeOnly;
        }

        if ( symbol is IParameterSymbol )
        {
            // Until we support template parameters and local functions, all parameters are parameters
            // of expression lambdas, which are of unknown scope.
            return TemplatingScope.LateBound;
        }

        // Aspect members are processed as compile-time-only by the template compiler even if some members can also
        // be called from run-time code.
        if ( this.IsAspectMember( symbol ) )
        {
            var templateInfo = this._symbolScopeClassifier.GetTemplateInfo( symbol );

            if ( templateInfo.CanBeReferencedAsRunTimeCode )
            {
                return TemplatingScope.RunTimeOnly;
            }
            else
            {
                var valueScope = this._symbolScopeClassifier.GetTemplatingScope( symbol ).GetExpressionValueScope();

                return valueScope switch
                {
                    TemplatingScope.CompileTimeOnly => TemplatingScope.CompileTimeOnly,
                    TemplatingScope.RunTimeOrCompileTime => this.FixCompileTimeReturningBothScopeWithSerializers( symbol ),
                    TemplatingScope.RunTimeOnly => TemplatingScope.CompileTimeOnlyReturningRuntimeOnly,
                    _ => throw new AssertionFailedException( $"Unexpected templating scope: {valueScope}." )
                };
            }
        }

        // For other symbols, we use the SymbolScopeClassifier.
        var templatingScope = this._symbolScopeClassifier.GetTemplatingScope( symbol );

        return GetMoreSpecificScope( templatingScope );

        TemplatingScope GetMoreSpecificScope( TemplatingScope scope )
        {
            if ( scope == TemplatingScope.RunTimeOrCompileTime )
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

                return TemplatingScope.RunTimeOrCompileTime;
            }
            else if ( scope == TemplatingScope.CompileTimeOnlyReturningBoth )
            {
                return this.FixCompileTimeReturningBothScopeWithSerializers( symbol );
            }
            else
            {
                return scope;
            }
        }
    }

    /// <summary>
    /// Gets the common scope of many symbols (all candidates of for the same name), or null if
    /// there is no common scope.
    /// </summary>
    private TemplatingScope? GetCommonSymbolScope( IEnumerable<ISymbol> symbols )
    {
        var scope = default(TemplatingScope?);

        foreach ( var symbol in symbols )
        {
            var thisScope = this.GetSymbolScope( symbol );

            if ( scope == null )
            {
                scope = thisScope;
            }
            else if ( scope != thisScope )
            {
                return null;
            }
        }

        return scope;
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

    private TemplatingScope[] GetNodeScopes( IReadOnlyCollection<SyntaxNode?> nodes )
    {
        var scopes = new TemplatingScope[nodes.Count];

        var i = 0;

        foreach ( var node in nodes )
        {
            scopes[i] = this.GetNodeScope( node );
            i++;
        }

        return scopes;
    }

    /// <summary>
    /// Gets the scope of a <see cref="SyntaxNode"/>.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private TemplatingScope GetNodeScope( SyntaxNode? node ) => this.GetNodeScope( node, false );

    /// <summary>
    /// Gets the scope of a <see cref="SyntaxNode"/>.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private TemplatingScope GetNodeScope( SyntaxNode? node, bool forAssignment )
    {
        if ( node == null )
        {
            return TemplatingScope.RunTimeOrCompileTime;
        }

        // If the node is dynamic, it is run-time only.
        if ( this._templateMemberClassifier.IsNodeOfDynamicType( node ) )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

            // Dynamic local variables are considered compile-time because they must be transformed. 
            if ( !forAssignment && this._templateMemberClassifier.RequiresCompileTimeExecution( symbol ) )
            {
                return TemplatingScope.CompileTimeOnlyReturningRuntimeOnly;
            }
            else
            {
                return TemplatingScope.Dynamic;
            }
        }

        switch ( node )
        {
            case NameSyntax:
            case TupleTypeSyntax:
                // If the node is an identifier, it means it should have a symbol (or at least candidates)
                // and the scope is given by the symbol.
                // The same applies to tuples.
                return this.GetCommonSymbolScope( this._syntaxTreeAnnotationMap.GetCandidateSymbols( node ) )
                    .GetValueOrDefault( TemplatingScope.RunTimeOrCompileTime );

            case NullableTypeSyntax nullableType:
                return this.GetNodeScope( nullableType.ElementType );

            default:
                // Otherwise, the scope is given by the annotation given by the deeper
                // visitor or the previous algorithm iteration.
                return node.GetScopeFromAnnotation().GetValueOrDefault();
        }
    }

    private void ReportScopeError( SyntaxNode node )
    {
        var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node ).AssertNotNull();

        this._symbolScopeClassifier.ReportScopeError( node, symbol, this );
    }

    private TemplatingScope GetAssignmentScope( SyntaxNode node )
    {
        var scope = this.GetNodeScope( node, true );

        if ( scope == TemplatingScope.CompileTimeOnlyReturningBoth && node is TupleExpressionSyntax )
        {
            return TemplatingScope.CompileTimeOnly;
        }
        else
        {
            return scope.GetExpressionExecutionScope();
        }
    }

    // ReSharper disable once UnusedMember.Local

    private TemplatingScope GetExpressionTypeScope( ExpressionSyntax? node )
    {
        if ( node != null && this._syntaxTreeAnnotationMap.GetExpressionType( node ) is { } parentExpressionType )
        {
            return this.GetSymbolScope( parentExpressionType );
        }
        else
        {
            return TemplatingScope.RunTimeOrCompileTime;
        }
    }

    private TemplatingScope GetExpressionScope( IReadOnlyList<SyntaxNode?>? annotatedChildren, SyntaxNode originalParent, bool reportError = true )
    {
        if ( annotatedChildren == null || annotatedChildren.Count == 0 )
        {
            return TemplatingScope.RunTimeOrCompileTime;
        }

        var scopes = this.GetNodeScopes( annotatedChildren );

        return this.GetExpressionScope( annotatedChildren, scopes, originalParent, reportError );
    }

    /// <summary>
    /// Gives the <see cref="TemplatingScope"/> of a parent given the scope of its children.
    /// </summary>
    /// <param name="childrenScopes"></param>
    /// <param name="originalParent"></param>
    /// <returns></returns>
    private TemplatingScope GetExpressionScope(
        IReadOnlyList<SyntaxNode?> children,
        IReadOnlyList<TemplatingScope> childrenScopes,
        SyntaxNode originalParent,
        bool reportError = true,
        bool preferCompileTime = true )
    {
        // Get the scope of type of the parent node.

        var parentExpressionScope = TemplatingScope.RunTimeOrCompileTime;

        if ( originalParent is ExpressionSyntax originalExpression )
        {
            parentExpressionScope = this.GetExpressionTypeScope( originalExpression );
        }

        var combinedExecutionScope = parentExpressionScope.GetExpressionExecutionScope();
        var combinedValueScope = parentExpressionScope.GetExpressionValueScope();
        var useCompileTimeIfPossible = false;
        var lastNonNeutralNodeIndex = -1;

        for ( var i = 0; i < childrenScopes.Count; i++ )
        {
            var childScope = childrenScopes[i];
            var childExecutionScope = childScope.GetExpressionExecutionScope();
            var childValueScope = childScope.GetExpressionValueScope();

            if ( childExecutionScope == TemplatingScope.CompileTimeOnly )
            {
                // If a child executes at compile time, if we can, we prefer to evaluate the whole expression at compile time.
                useCompileTimeIfPossible = true;
            }

            combinedExecutionScope = combinedExecutionScope.GetCombinedExecutionScope( childExecutionScope );
            combinedValueScope = combinedValueScope.GetCombinedValueScope( childValueScope );

            if ( combinedExecutionScope == TemplatingScope.Conflict ||
                 combinedValueScope == TemplatingScope.Conflict )
            {
                if ( reportError )
                {
                    // Report an error.
                    if ( lastNonNeutralNodeIndex >= 0 )
                    {
                        this.ReportDiagnostic(
                            TemplatingDiagnosticDescriptors.ExpressionScopeConflictBecauseOfChildren,
                            originalParent,
                            (originalParent.ToString(), children[lastNonNeutralNodeIndex].AssertNotNull().ToString(),
                             childrenScopes[lastNonNeutralNodeIndex].ToDisplayString(),
                             children[i].AssertNotNull().ToString(), childrenScopes[i].ToDisplayString()) );
                    }
                    else
                    {
                        this.ReportDiagnostic(
                            TemplatingDiagnosticDescriptors.ExpressionScopeConflictBecauseOfParent,
                            originalParent,
                            (originalParent.ToString(), parentExpressionScope.ToDisplayString(), children[i].AssertNotNull().ToString(),
                             childrenScopes[i].ToDisplayString()) );
                    }

                    // We don't propagate the conflict state after we report the error, because this would cause the reporting of more errors and be more confusing.
                    return TemplatingScope.RunTimeOrCompileTime;
                }
                else
                {
                    return TemplatingScope.Conflict;
                }
            }

            if ( childExecutionScope != TemplatingScope.RunTimeOrCompileTime )
            {
                lastNonNeutralNodeIndex = i;
            }
        }

        var resultingScope = (combinedExecutionScope, combinedValueScope) switch
        {
            (_, TemplatingScope.LateBound) => TemplatingScope.LateBound,
            (TemplatingScope.LateBound, _) => TemplatingScope.LateBound,
            (_, TemplatingScope.Conflict) => TemplatingScope.Conflict,
            (TemplatingScope.Conflict, _) => TemplatingScope.Conflict,
            (TemplatingScope.CompileTimeOnly, TemplatingScope.CompileTimeOnly) => TemplatingScope.CompileTimeOnly,
            (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOnly) => TemplatingScope.RunTimeOnly,
            (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.RunTimeOrCompileTime,
            (TemplatingScope.RunTimeOrCompileTime, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.RunTimeOrCompileTime,
            (TemplatingScope.RunTimeOnly, _) => TemplatingScope.RunTimeOnly,
            _ => throw new AssertionFailedException( $"Unexpected combination: ({combinedExecutionScope}, {combinedValueScope})." )
        };

        if ( resultingScope == TemplatingScope.RunTimeOrCompileTime && useCompileTimeIfPossible && preferCompileTime )
        {
            resultingScope = TemplatingScope.CompileTimeOnlyReturningBoth;
        }

        return resultingScope;
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
        => (T?) this.DefaultVisitImpl( node );

    /// <summary>
    /// Default visitor.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override SyntaxNode? VisitCore( SyntaxNode? node ) => this.DefaultVisitImpl( node );

    [return: NotNullIfNotNull( "node" )]
    private SyntaxNode? DefaultVisitImpl( SyntaxNode? node )
    {
        if ( node == null )
        {
            return null;
        }

        this._cancellationToken.ThrowIfCancellationRequested();

        // Adds annotations to the children node.
        var transformedNode = base.VisitCore( node )!;

        return this.AddScopeAnnotationToVisitedNode( node, transformedNode );
    }

    /// <summary>
    /// Adds scope annotation to a node that has been visited - so children are annotated but not the node itself.
    /// </summary>
    private SyntaxNode AddScopeAnnotationToVisitedNode( SyntaxNode node, SyntaxNode visitedNode )
    {
        if ( this._currentScopeContext.ForceCompileTimeOnlyExpression )
        {
            var currentScope = visitedNode.GetScopeFromAnnotation();

            if ( currentScope is TemplatingScope.RunTimeOnly or TemplatingScope.Dynamic ||
                 this._templateMemberClassifier.IsNodeOfDynamicType( visitedNode ) )
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
            if ( currentScope != TemplatingScope.LateBound && currentScope != TemplatingScope.TypeOfRunTimeType )
            {
                return visitedNode.ReplaceScopeAnnotation( TemplatingScope.CompileTimeOnly );
            }
        }

        if ( visitedNode.HasScopeAnnotation() || visitedNode is StatementSyntax )
        {
            // If the transformed node has already an annotation, it means it has already been classified by
            // a previous run of the algorithm, and there is no need to classify it again.
            return visitedNode;
        }

        // Here is the default implementation for expressions. The scope of the parent is the combined scope of the children.
        var childNodes = visitedNode.ChildNodes().Where( n => n is ExpressionSyntax or InterpolationSyntax );

        var combinedScope = this.GetExpressionScope( childNodes.ToList(), node );

        return visitedNode.AddScopeAnnotation( combinedScope );
    }

    #region Anonymous objects

    public override SyntaxNode VisitAnonymousObjectMemberDeclarator( AnonymousObjectMemberDeclaratorSyntax node )
    {
        var scope = this._currentScopeContext.ForceCompileTimeOnlyExpression ? TemplatingScope.CompileTimeOnly : TemplatingScope.RunTimeOnly;

        return node.Update( node.NameEquals, this.Visit( node.Expression ) ).AddScopeAnnotation( scope );
    }

    public override SyntaxNode VisitAnonymousObjectCreationExpression( AnonymousObjectCreationExpressionSyntax node )
    {
        var scope = this._currentScopeContext.ForceCompileTimeOnlyExpression ? TemplatingScope.CompileTimeOnly : TemplatingScope.RunTimeOnly;

        var symbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node );

        if ( symbol != null )
        {
            this.SetLocalSymbolScope( symbol, scope );
        }

        // Anonymous objects are currently run-time-only unless they are in a compile-time-only scope -- until we implement more complex rules.
        var transformedMembers =
            node.Initializers.SelectAsEnumerable( i => this.Visit( i ).AddScopeAnnotation( scope ) );

        return node.Update(
                node.NewKeyword,
                node.OpenBraceToken,
                SeparatedList( transformedMembers, node.Initializers.GetSeparators() ),
                node.CloseBraceToken )
            .AddScopeAnnotation( scope );
    }

    #endregion

    public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node, n => base.VisitClassDeclaration( n ) );

    public override SyntaxNode VisitStructDeclaration( StructDeclarationSyntax node )
        => this.VisitTypeDeclaration( node, n => base.VisitStructDeclaration( n ) );

    public override SyntaxNode VisitRecordDeclaration( RecordDeclarationSyntax node )
        => this.VisitTypeDeclaration( node, n => base.VisitRecordDeclaration( n ) );

    public override SyntaxNode VisitDelegateDeclaration( DelegateDeclarationSyntax node )
        => this.VisitTypeDeclaration( node, n => base.VisitDelegateDeclaration( n ) );

    public override SyntaxNode VisitEnumDeclaration( EnumDeclarationSyntax node ) => this.VisitTypeDeclaration( node, n => base.VisitEnumDeclaration( n ) );

    private T VisitTypeDeclaration<T>( T node, Func<T, SyntaxNode?> callBase )
        where T : SyntaxNode
    {
        var typeScope = this.GetSymbolScope( this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node ).AssertNotNull() );

        if ( typeScope == TemplatingScope.Conflict )
        {
            return node;
        }
        else if ( typeScope != TemplatingScope.RunTimeOnly )
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

    public override SyntaxNode VisitIdentifierName( IdentifierNameSyntax node )
    {
        if ( this._currentScopeContext.IsDynamicTypingForbidden && node.Identifier.Text == "dynamic" )
        {
            this.ReportDiagnostic( TemplatingDiagnosticDescriptors.CannotUseDynamicTypingInLocalFunction, node.GetDiagnosticLocation(), default );
        }

        var identifierNameSyntax = (IdentifierNameSyntax) base.VisitIdentifierName( node )!;
        var symbols = this._syntaxTreeAnnotationMap.GetCandidateSymbols( node ).ToList();
        var scope = this.GetCommonSymbolScope( symbols );

        if ( scope is null or TemplatingScope.DynamicTypeConstruction )
        {
            // An error should be emitted elsewhere, so we continue considering it is run-time.
            scope = TemplatingScope.RunTimeOrCompileTime;
        }
        else if ( scope == TemplatingScope.Conflict )
        {
            this._symbolScopeClassifier.ReportScopeError( node, symbols.First(), this );

            scope = TemplatingScope.RunTimeOrCompileTime;
        }

        if ( scope.Value.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
        {
            // Template code cannot be referenced in a template until this is implemented.

            var symbol = symbols[0];

            if ( this._symbolScopeClassifier.GetTemplateInfo( symbol ).AttributeType == TemplateAttributeType.Template )
            {
                this.ReportDiagnostic(
                    TemplatingDiagnosticDescriptors.TemplateCannotReferenceTemplate,
                    node,
                    (symbol, this._currentTemplateMember!) );
            }
        }

        var annotatedNode = identifierNameSyntax.AddScopeAnnotation( scope );

        annotatedNode = (IdentifierNameSyntax) this.AddColoringAnnotations( annotatedNode, symbols.FirstOrDefault(), scope.Value )!;

        return annotatedNode;
    }

    private SyntaxNodeOrToken AddColoringAnnotations( SyntaxNodeOrToken nodeOrToken, ISymbol? symbol, TemplatingScope scope )
    {
        switch ( symbol )
        {
            case null:
                // Coverage: ignore.
                return nodeOrToken;

            case ILocalSymbol when scope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly:
                nodeOrToken = nodeOrToken.AddColoringAnnotation( TextSpanClassification.CompileTimeVariable );

                break;

            default:
                {
                    if ( TemplateMemberSymbolClassifier.HasTemplateKeywordAttribute( symbol ) )
                    {
                        nodeOrToken = nodeOrToken.AddColoringAnnotation( TextSpanClassification.TemplateKeyword );
                    }
                    else
                    {
                        var node = nodeOrToken.AsNode() ?? nodeOrToken.Parent;

                        if ( node != null &&
                             symbol is not (ITypeSymbol or ILocalSymbol) &&
                             this._templateMemberClassifier.IsNodeOfDynamicType( node ) )
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

    public override SyntaxNode VisitMemberBindingExpression( MemberBindingExpressionSyntax node )
    {
        var transformedName = this.Visit( node.Name );

        return node.WithName( transformedName ).WithScopeAnnotationFrom( transformedName );
    }

    public override SyntaxNode VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
    {
        this.VisitAccessExpressionCore(
            node.Expression,
            node.Name,
            node.OperatorToken,
            out var transformedExpression,
            out var transformedName,
            out var transformedOperator,
            out var scope );

        return node
            .Update( transformedExpression, transformedOperator, (SimpleNameSyntax) transformedName )
            .AddScopeAnnotation( scope );
    }

    public override SyntaxNode VisitConditionalAccessExpression( ConditionalAccessExpressionSyntax node )
    {
        this.VisitAccessExpressionCore(
            node.Expression,
            node.WhenNotNull,
            node.OperatorToken,
            out var transformedExpression,
            out var transformedWhenNotNull,
            out var transformedOperator,
            out var scope );

        return node
            .Update( transformedExpression, transformedOperator, transformedWhenNotNull )
            .AddScopeAnnotation( scope );
    }

    private void VisitAccessExpressionCore(
        ExpressionSyntax left,
        ExpressionSyntax right,
        SyntaxToken operatorToken,
        out ExpressionSyntax transformedLeft,
        out ExpressionSyntax transformedRight,
        out SyntaxToken transformedOperatorToken,
        out TemplatingScope scope )
    {
        transformedRight = this.Visit( right );

        var rightScope = this.GetNodeScope( transformedRight );

        ScopeContext? context = null;
        scope = TemplatingScope.RunTimeOrCompileTime;

        switch ( rightScope )
        {
            case TemplatingScope.CompileTimeOnly:
            case TemplatingScope.CompileTimeOnlyReturningBoth:
            case TemplatingScope.CompileTimeOnlyReturningRuntimeOnly:
                // If the member is compile-time (because of rules on the symbol), the expression on the left MUST be compile-time.
                context = this._currentScopeContext.CompileTimeOnly( $"a compile-time-only member '{right}'" );
                scope = rightScope;

                break;

            case TemplatingScope.Dynamic:
                // A member is run-time dynamic because the left part is dynamic, so there is no need to force it run-time.
                // It can actually contain build-time subexpressions.
                scope = TemplatingScope.Dynamic;

                break;

            case TemplatingScope.LateBound when this._syntaxTreeAnnotationMap.GetExpressionType( left ) is IDynamicTypeSymbol:
                // This is a member access of a dynamic receiver.
                scope = TemplatingScope.RunTimeOnly;
                context = this._currentScopeContext.RunTimePreferred( $"a member of the run-time-only '{right}'" );

                break;

            case TemplatingScope.RunTimeOnly:
                scope = TemplatingScope.RunTimeOnly;

                break;
        }

        using ( this.WithScopeContext( context ) )
        {
            transformedLeft = this.Visit( left );

            if ( scope == TemplatingScope.RunTimeOrCompileTime )
            {
                var leftScope = this.GetNodeScope( transformedLeft );
                scope = TemplatingScopeExtensions.GetAccessMemberScope( leftScope, rightScope );
            }

            // If both sides of the member are template keywords, display the . as a template keyword too.
            transformedOperatorToken = operatorToken;

            if ( transformedLeft.GetColorFromAnnotation() == TextSpanClassification.TemplateKeyword &&
                 transformedRight.GetColorFromAnnotation() == TextSpanClassification.TemplateKeyword )
            {
                transformedOperatorToken = transformedOperatorToken.AddColoringAnnotation( TextSpanClassification.TemplateKeyword );
            }
        }
    }

    public override SyntaxNode VisitElementAccessExpression( ElementAccessExpressionSyntax node )
    {
        // In an element access (such as Tags[x]), the scope is given by the expression.

        var transformedExpression = this.Visit( node.Expression );
        var scope = this.GetNodeScope( transformedExpression );

        ScopeContext? context;

        if ( scope is TemplatingScope.CompileTimeOnly or TemplatingScope.CompileTimeOnlyReturningBoth )
        {
            context = this._currentScopeContext.CompileTimeOnly( $"element of the compile-time collection '{node.Expression}'" );
        }
        else if ( scope.IsCompileTimeMemberReturningRunTimeValue() )
        {
            scope = TemplatingScope.Dynamic;

            context = this._currentScopeContext.RunTimePreferred( $"element of the run-time-only collection '{node.Expression}'" );
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

    public override SyntaxNode VisitInvocationExpression( InvocationExpressionSyntax node )
    {
        // nameof() is always compile-time.
        if ( node.IsNameOf() )
        {
            return node.AddScopeAnnotation( TemplatingScope.RunTimeOrCompileTime );
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
                this._currentScopeContext.CompileTimeOnly( $"a call to a method that sets the compile-time variable '{compileTimeOutArguments[0]}'" );

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

                    case { TypeKind: TypeKind.Dynamic }:
                        // In case of invocation of a dynamic location, there is no list of parameters, only arguments.
                        parameters = default;

                        break;

                    default:
                        // We can get here in case of syntax error.
                        return node;
                }

                break;
        }

        InvocationExpressionSyntax updatedInvocation;

        if ( expressionScope.GetExpressionExecutionScope() != TemplatingScope.RunTimeOrCompileTime )
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

                var parameterType = parameter?.Type ?? this._syntaxTreeAnnotationMap.GetParameterSymbol( argument )?.Type;

                ExpressionSyntax transformedArgumentValue;

                // Transform the argument value.
                if ( expressionScope.IsCompileTimeMemberReturningRunTimeValue() || TemplateMemberSymbolClassifier.IsDynamicParameter( parameterType ) )
                {
                    // dynamic or dynamic[]

                    using ( this.WithScopeContext(
                               this._currentScopeContext.RunTimePreferred(
                                   $"argument of the dynamic parameter '{parameter?.Name ?? argumentIndex.ToString( CultureInfo.InvariantCulture )}'" ) ) )
                    {
                        transformedArgumentValue = this.Visit( argument.Expression );
                    }
                }
                else if ( expressionScope.EvaluatesToRunTimeValue() )
                {
                    using ( this.WithScopeContext( this._currentScopeContext.RunTimePreferred( $"argument of the run-time method '{node.Expression}'" ) ) )
                    {
                        transformedArgumentValue = this.Visit( argument.Expression );
                    }

                    var argumentType = this._syntaxTreeAnnotationMap.GetExpressionType( argument.Expression );

                    if ( argumentType != null && this._symbolScopeClassifier.GetTemplatingScope( argumentType ) == TemplatingScope.CompileTimeOnly )
                    {
                        this.ReportDiagnostic(
                            TemplatingDiagnosticDescriptors.CompileTimeTypeInInvocationOfRuntimeMethod,
                            argument.Expression,
                            (argumentType, node.Expression.ToString()) );
                    }
                }
                else
                {
                    using ( this.WithScopeContext( this._currentScopeContext.CompileTimeOnly( $"a compile-time expression '{node.Expression}'" ) ) )
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
                var children = new List<SyntaxNode>( transformedArgumentList.Arguments.Count + 1 ) { transformedExpression };

                children.AddRange( transformedArgumentList.Arguments );

                var childScopes = this.GetNodeScopes( children );

                // We do not prefer compile-time when the invocation is a statement because this is most
                // likely a run-time statement invocation. All compile-time methods that can be used as statements are explicitly marked.
                var preferCompileTime = !node.Parent.IsKind( SyntaxKind.ExpressionStatement );

                invocationScope = this.GetExpressionScope( children, childScopes, node, preferCompileTime: preferCompileTime );
            }

            updatedInvocation = updatedInvocation.AddScopeAnnotation( invocationScope );
        }

        return updatedInvocation;
    }

    public override SyntaxNode VisitArgument( ArgumentSyntax node )
    {
        // We don't add an annotation to the argument because it needs to be inherited from the parent.
        var transformedExpression = this.Visit( node.Expression );

        return node.WithExpression( transformedExpression );
    }

    public override SyntaxNode VisitIfStatement( IfStatementSyntax node )
    {
        var annotatedCondition = this.Visit( node.Condition );
        var conditionScope = this.GetNodeScope( annotatedCondition );

        TemplatingScope ifScope;
        StatementSyntax annotatedStatement;
        StatementSyntax? annotatedElseStatement;
        ScopeContext? scopeContext;

        if ( conditionScope.GetExpressionExecutionScope( true ) == TemplatingScope.CompileTimeOnly )
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

            scopeContext = this._currentScopeContext.RunTimeConditional( "if ( " + node.Condition + " )" );
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

    public override SyntaxNode VisitBreakStatement( BreakStatementSyntax node )
        => node.AddScopeAnnotation( this._currentScopeContext.CurrentBreakOrContinueScope );

    public override SyntaxNode VisitContinueStatement( ContinueStatementSyntax node )
        => node.AddScopeAnnotation( this._currentScopeContext.CurrentBreakOrContinueScope );

    public override SyntaxNode VisitForEachStatement( ForEachStatementSyntax node )
    {
        var local = (ILocalSymbol) this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node )!;

        var annotatedExpression = this.Visit( node.Expression );

        if ( this._templateMemberClassifier.IsNodeOfDynamicType( node.Type ) &&
             !this._templateMemberClassifier.IsNodeOfDynamicType( annotatedExpression ) )
        {
            this.ReportDiagnostic(
                TemplatingDiagnosticDescriptors.DynamicVariableSetToNonDynamic,
                node.Identifier,
                node.Identifier.Text );
        }

        TemplatingScope forEachScope;
        string reason;

        if ( node.AwaitKeyword.IsKind( SyntaxKind.None ) )
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

        using ( this.WithScopeContext( this._currentScopeContext.BreakOrContinue( forEachScope, reason ) ) )
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

    public override SyntaxNode VisitDeclarationPattern( DeclarationPatternSyntax node )
    {
        // If the type of a pattern is compile-time-only, the variable is compile-time.

        var transformedType = this.Visit( node.Type );
        var scope = this.GetNodeScope( transformedType );

        var typeSymbol = this._syntaxTreeAnnotationMap.GetExpressionType( node.Type )!;

        var context = scope == TemplatingScope.CompileTimeOnly
            ? this._currentScopeContext.CompileTimeOnly( $"local variable of compile-time type '{typeSymbol}'" )
            : null;

        VariableDesignationSyntax transformedDesignation;

        using ( this.WithScopeContext( context ) )
        {
            transformedDesignation = this.Visit( node.Designation );
        }

        return node.Update( transformedType, transformedDesignation ).AddScopeAnnotation( scope );
    }

    public override SyntaxNode VisitIsPatternExpression( IsPatternExpressionSyntax node )
    {
        // The scope of a pattern expression is given by the expression (left part).
        var transformedExpression = this.Visit( node.Expression );
        var scope = this.GetNodeScope( transformedExpression ).GetExpressionValueScope();

        var context = scope == TemplatingScope.CompileTimeOnly
            ? this._currentScopeContext.CompileTimeOnly( $"pattern on the compile-time expression '{node.Expression}'" )
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

    public override SyntaxNode VisitSingleVariableDesignation( SingleVariableDesignationSyntax node )
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

    public override SyntaxNode VisitDeclarationExpression( DeclarationExpressionSyntax node )
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
                context = this._currentScopeContext.CompileTimeOnly( $"an inline variable declaration of compile-time type '{transformedType}" );
            }
        }

        VariableDesignationSyntax transformedDesignation;

        using ( this.WithScopeContext( context ) )
        {
            transformedDesignation = this.Visit( node.Designation );
        }

        return node.Update( transformedType, transformedDesignation ).AddScopeAnnotation( scope );
    }

    public override SyntaxNode VisitVariableDeclarator( VariableDeclaratorSyntax node )
    {
        var symbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node )!;

        if ( symbol is not ILocalSymbol local )
        {
            // It's a field, or a field-like event.

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
                    // We don't want to visit the whole member because only the implementation must be annotated and transformed
                    // as a template.
                    return node.WithInitializer( this.Visit( node.Initializer ) ).AddIsTemplateAnnotation();
                }
                finally
                {
                    this._currentTemplateMember = previousTemplateMember;
                }
            }
            else
            {
                // Don't visit members that are not templates.
                return node;
            }
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
                    .GetExpressionValueScope( true );

                if ( localScope.IsUndetermined() )
                {
                    localScope = TemplatingScope.RunTimeOnly;
                }
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
                           ? this._currentScopeContext.CompileTimeOnly( "creation of a compile-time object" )
                           : this._currentScopeContext.RunTimePreferred( "creation of a run-time object" ) ) )
            {
                transformedArguments = node.ArgumentList.Arguments.SelectAsArray( a => this.Visit( a ) );
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

    public override SyntaxNode VisitVariableDeclaration( VariableDeclarationSyntax node )
    {
        var transformedType = this.Visit( node.Type );

        if ( this._templateMemberClassifier.IsNodeOfDynamicType( transformedType ) )
        {
            foreach ( var variable in node.Variables )
            {
                if ( variable.Initializer == null )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.CannotUseDynamicInUninitializedLocal,
                        variable.Identifier,
                        variable.Identifier.Text );
                }
                else
                {
                    if ( !this._templateMemberClassifier.IsNodeOfDynamicType( variable.Initializer.Value ) )
                    {
                        this.ReportDiagnostic(
                            TemplatingDiagnosticDescriptors.DynamicVariableSetToNonDynamic,
                            variable.Identifier,
                            variable.Identifier.Text );
                    }
                }
            }
        }

        if ( this.GetNodeScope( transformedType ) == TemplatingScope.CompileTimeOnly )
        {
            var typeSymbol = this._syntaxTreeAnnotationMap.GetExpressionType( node.Type )!;

            using ( this.WithScopeContext( this._currentScopeContext.CompileTimeOnly( $"a local variable of compile-time-only type '{typeSymbol}'" ) ) )
            {
                // ReSharper disable once RedundantSuppressNullableWarningExpression
                var transformedVariables = node.Variables.SelectAsEnumerable( v => this.Visit( v )! );

                return node.Update( transformedType, SeparatedList( transformedVariables ) ).AddScopeAnnotation( TemplatingScope.CompileTimeOnly );
            }
        }
        else
        {
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            var transformedVariables = node.Variables.SelectAsImmutableArray( v => this.Visit( v )! );

            var variableScopes = transformedVariables.SelectAsEnumerable( v => v.GetScopeFromAnnotation() ).Distinct().ToList();

            if ( variableScopes.Count != 1 )
            {
                this.ReportDiagnostic(
                    TemplatingDiagnosticDescriptors.SplitVariables,
                    node,
                    string.Join( ",", node.Variables.SelectAsEnumerable( v => "'" + v.Identifier.Text + "'" ) ) );
            }

            var variableScope = variableScopes.Single();

            // We don't use transformedType because we want to replace the type annotation to strictly RunTime and not, for instance, CompileTimeReturningRunTime.
            return node.Update( node.Type.AddScopeAnnotation( variableScope ), SeparatedList( transformedVariables ) ).AddScopeAnnotation( variableScope );
        }
    }

    public override SyntaxNode VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
    {
        var transformedNode = (LocalDeclarationStatementSyntax) base.VisitLocalDeclarationStatement( node )!;

        return transformedNode.AddScopeAnnotation( this.GetNodeScope( transformedNode.Declaration ) );
    }

    #endregion

    public override SyntaxNode VisitAttribute( AttributeSyntax node )
    {
        var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node.Name );

        if ( symbol is IMethodSymbol { ContainingNamespace: { } } constructor
             && (constructor.ContainingNamespace.ToString()?.StartsWith( "Metalama.Framework", StringComparison.Ordinal ) ?? false) )
        {
            node = node.AddColoringAnnotation( TextSpanClassification.CompileTime );
        }

        // Otherwise, don't process attributes.
        return node;
    }

    public override SyntaxNode VisitAttributeList( AttributeListSyntax node )
    {
        var annotatedList = (AttributeListSyntax) base.VisitAttributeList( node )!;

        if ( annotatedList.Attributes.All( a => a.GetColorFromAnnotation() == TextSpanClassification.CompileTime ) )
        {
            return annotatedList.AddColoringAnnotation( TextSpanClassification.CompileTime );
        }
        else
        {
            return annotatedList;
        }
    }

    private T VisitMemberDeclaration<T>( T node, Func<T, T> visitImplementation )
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
                // We don't want to visit the whole member because only the implementation must be annotated and transformed
                // as a template.
                return visitImplementation( node ).AddIsTemplateAnnotation();
            }
            finally
            {
                this._currentTemplateMember = previousTemplateMember;
            }
        }
        else
        {
            // Don't visit members that are not templates.
            return node;
        }
    }

    // This method is called from the classification service. Constructors are never templates, so it can be skipped.
    public override SyntaxNode VisitConstructorDeclaration( ConstructorDeclarationSyntax node ) => node;

    public override SyntaxNode VisitMethodDeclaration( MethodDeclarationSyntax node )
        => this.VisitMemberDeclaration(
            node,
            n => node
                .WithBody( this.Visit( n.Body ) )
                .WithExpressionBody( this.Visit( n.ExpressionBody ) )
                .WithAttributeLists( this.VisitList( node.AttributeLists ) )
                .WithParameterList( this.Visit( node.ParameterList ) )
                .WithReturnType( this.Visit( node.ReturnType ) )
                .WithTypeParameterList( this.Visit( node.TypeParameterList ) ) );

    public override SyntaxNode VisitParameter( ParameterSyntax node )
    {
        var annotatedNode = (ParameterSyntax) base.VisitParameter( node )!;

        if ( this._currentTemplateMember != null )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node );

            if ( symbol != null )
            {
                var scope = this.GetSymbolScope( symbol );

                if ( scope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
                {
                    annotatedNode = annotatedNode.AddColoringAnnotation( TextSpanClassification.CompileTime );
                }

                this._templateProjectManifestBuilder?.AddOrUpdateSymbol( symbol, scope );
            }
        }

        return annotatedNode;
    }

    public override SyntaxNode VisitTypeParameter( TypeParameterSyntax node )
    {
        var annotatedNode = base.VisitTypeParameter( node )!;

        if ( this._currentTemplateMember != null )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node );

            if ( symbol != null )
            {
                var scope = this.GetSymbolScope( symbol );

                if ( scope == TemplatingScope.CompileTimeOnlyReturningRuntimeOnly )
                {
                    annotatedNode = annotatedNode.AddColoringAnnotation( TextSpanClassification.CompileTime );
                }

                this._templateProjectManifestBuilder?.AddOrUpdateSymbol( symbol, scope );
            }
        }

        return annotatedNode;
    }

    public override SyntaxNode VisitAccessorDeclaration( AccessorDeclarationSyntax node )
        => this.VisitMemberDeclaration(
            node,
            n => node.WithBody( this.Visit( n.Body ) )
                .WithExpressionBody( this.Visit( n.ExpressionBody ) )
                .WithAttributeLists( this.VisitList( node.AttributeLists ) ) );

    public override SyntaxNode VisitPropertyDeclaration( PropertyDeclarationSyntax node )
        => this.VisitMemberDeclaration(
            node,
            n => n.WithAccessorList( this.Visit( n.AccessorList ) )
                .WithExpressionBody( this.Visit( n.ExpressionBody ) )
                .WithInitializer( this.Visit( n.Initializer ) )
                .WithAttributeLists( this.VisitList( node.AttributeLists ) ) );

    public override SyntaxNode VisitEventDeclaration( EventDeclarationSyntax node )
        => this.VisitMemberDeclaration( node, n => n.WithAccessorList( this.Visit( n.AccessorList ) ) );

    private static bool IsMutatingUnaryOperator( SyntaxToken token ) => token.Kind() is SyntaxKind.PlusPlusToken or SyntaxKind.MinusMinusToken;

    public override SyntaxNode VisitPostfixUnaryExpression( PostfixUnaryExpressionSyntax node )
    {
        var transformedOperand = this.VisitUnaryExpressionOperand( node.Operand, node.OperatorToken );

        return node.Update( transformedOperand, node.OperatorToken ).WithSymbolAnnotationsFrom( node ).WithScopeAnnotationFrom( transformedOperand );
    }

    public override SyntaxNode VisitPrefixUnaryExpression( PrefixUnaryExpressionSyntax node )
    {
        var transformedOperand = this.VisitUnaryExpressionOperand( node.Operand, node.OperatorToken );

        return node.Update( node.OperatorToken, transformedOperand ).WithSymbolAnnotationsFrom( node ).WithScopeAnnotationFrom( transformedOperand );
    }

    private ExpressionSyntax VisitUnaryExpressionOperand( ExpressionSyntax operand, SyntaxToken @operator )
    {
        var transformedOperand = this.Visit( operand );

        var scope = this.GetNodeScope( transformedOperand );

        // We cannot mutate a compile-time expression in a run-time-condition block.
        if ( scope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly && IsMutatingUnaryOperator( @operator ) )
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

    public override SyntaxNode VisitAssignmentExpression( AssignmentExpressionSyntax node )
    {
        if ( node.Parent is InitializerExpressionSyntax )
        {
            // In an initializer assignment, the scope is determined by the right side but the transformation always proceed according to the parent.
            var transformedRight = this.Visit( node.Right );
            var scope = this.GetNodeScope( transformedRight );

            return node.Update( node.Left, node.OperatorToken, transformedRight )
                .AddScopeAnnotation( scope )
                .AddTargetScopeAnnotation( TemplatingScope.MustFollowParent );
        }
        else
        {
            // The scope of a classical assignment is determined by the left side.
            var transformedLeft = this.Visit( node.Left );

            var scope = this.GetAssignmentScope( transformedLeft );
            ExpressionSyntax? transformedRight;

            // If we are in a run-time-conditional block, we cannot assign compile-time variables.
            ScopeContext? context = null;

            if ( scope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
            {
                if ( this._currentScopeContext.IsRuntimeConditionalBlock )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.CannotSetCompileTimeVariableInRunTimeConditionalBlock,
                        node.Left,
                        (node.Left.ToString(), this._currentScopeContext.IsRuntimeConditionalBlockReason!) );
                }

                // The right part must be compile-time.
                context = this._currentScopeContext.CompileTimeOnly( "the assignment of a compile-time expression" );
            }

            using ( this.WithScopeContext( context ) )
            {
                transformedRight = this.Visit( node.Right );
            }

            // If we have a discard assignment, take the scope from the right.
            if ( scope == TemplatingScope.RunTimeOrCompileTime
                 && this._syntaxTreeAnnotationMap.GetSymbol( node.Left ) is IDiscardSymbol )
            {
                scope = this.GetNodeScope( transformedRight );
            }

            return node.Update( transformedLeft, node.OperatorToken, transformedRight ).AddScopeAnnotation( scope );
        }
    }

    public override SyntaxNode VisitExpressionStatement( ExpressionStatementSyntax node )
    {
        var transformedExpression = this.Visit( node.Expression );
        var expressionScope = this.GetNodeScope( transformedExpression );
        var statementScope = expressionScope.GetExpressionExecutionScope().ReplaceIndeterminate( TemplatingScope.RunTimeOnly );

        return node.WithExpression( transformedExpression ).AddScopeAnnotation( expressionScope ).AddTargetScopeAnnotation( statementScope );
    }

    public override SyntaxNode VisitCastExpression( CastExpressionSyntax node )
    {
        if ( this._templateMemberClassifier.IsNodeOfDynamicType( node.Type ) )
        {
            this.ReportDiagnostic(
                TemplatingDiagnosticDescriptors.ForbiddenDynamicUseInTemplate,
                node.Type.GetLocation(),
                default );
        }

        TypeSyntax annotatedType;
        var annotatedExpression = this.Visit( node.Expression );
        var expressionScope = this.GetNodeScope( annotatedExpression );
        TemplatingScope castScope;

        if ( expressionScope.GetExpressionValueScope() == TemplatingScope.RunTimeOnly )
        {
            // The whole cast is run-time only.
            using ( this.WithScopeContext( this._currentScopeContext.RunTimePreferred( $"cast of the run-time-only expression '{node.Expression}'" ) ) )
            {
                annotatedType = this.Visit( node.Type );
            }

            castScope = TemplatingScope.RunTimeOnly;
        }
        else
        {
            annotatedType = this.Visit( node.Type );
            var typeScope = annotatedType.GetScopeFromAnnotation().GetValueOrDefault( TemplatingScope.RunTimeOrCompileTime ).GetExpressionValueScope();

            castScope = this.GetExpressionScope( new SyntaxNode[] { annotatedExpression, annotatedType }, new[] { expressionScope, typeScope }, node );
        }

        return node.Update( node.OpenParenToken, annotatedType, node.CloseParenToken, annotatedExpression )
            .AddScopeAnnotation( castScope );
    }

    public override SyntaxNode VisitBinaryExpression( BinaryExpressionSyntax node )
    {
        switch ( node.Kind() )
        {
            case SyntaxKind.IsExpression:
            case SyntaxKind.AsExpression:
                if ( this._templateMemberClassifier.IsNodeOfDynamicType( node.Right ) )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.ForbiddenDynamicUseInTemplate,
                        node.Right.GetLocation(),
                        default );
                }

                var annotatedType = (TypeSyntax) this.Visit( node.Right );
                var annotatedExpression = this.Visit( node.Left );
                var transformedNode = node.WithLeft( annotatedExpression ).WithRight( annotatedType );

                return this.AnnotateCastExpression( transformedNode, annotatedType, annotatedExpression );

            case SyntaxKind.CoalesceExpression:
                return this.VisitCoalesceExpression( node );
        }

        var visitedNode = base.VisitBinaryExpression( node )!;

        return this.AddScopeAnnotationToVisitedNode( node, visitedNode );
    }

    private SyntaxNode AnnotateCastExpression( ExpressionSyntax transformedCastNode, TypeSyntax annotatedType, ExpressionSyntax annotatedExpression )
    {
        var combinedScope = this.GetNodeScope( annotatedType ) == TemplatingScope.RunTimeOrCompileTime
            ? this.GetNodeScope( annotatedExpression ).GetExpressionValueScope()
            : this.GetExpressionScope( new[] { annotatedExpression }, transformedCastNode );

        return transformedCastNode.AddScopeAnnotation( combinedScope );
    }

    private SyntaxNode VisitCoalesceExpression( BinaryExpressionSyntax node )
    {
        // The scope is determined by the left part, unless the left part is indeterminate. The right part must follow.

        var annotatedLeft = this.Visit( node.Left );
        var leftScope = this.GetNodeScope( annotatedLeft );

        ExpressionSyntax annotatedRight;
        TemplatingScope combinedScope;

        if ( !leftScope.IsUndetermined() )
        {
            ScopeContext context;

            if ( leftScope.EvaluatesToRunTimeValue() )
            {
                context = this._currentScopeContext.RunTimePreferred( $"right part of the run-time '{node.Left} ??'" );
                combinedScope = TemplatingScope.RunTimeOnly;
            }
            else
            {
                context = this._currentScopeContext.CompileTimeOnly( $"right part of the compile-time '{node.Left} ??'" );
                combinedScope = leftScope;
            }

            using ( this.WithScopeContext( context ) )
            {
                annotatedRight = this.Visit( node.Right );
            }
        }
        else
        {
            // Use the default rule.
            annotatedRight = this.Visit( node.Right );
            var rightScope = this.GetNodeScope( annotatedRight );
            combinedScope = this.GetExpressionScope( new SyntaxNode[] { annotatedLeft, annotatedRight }, new[] { leftScope, rightScope }, node );
        }

        return node.Update( annotatedLeft, node.OperatorToken, annotatedRight ).AddScopeAnnotation( combinedScope );
    }

    public override SyntaxNode VisitConditionalExpression( ConditionalExpressionSyntax node )
    {
        var annotatedCondition = this.Visit( node.Condition );
        var conditionScope = this.GetNodeScope( annotatedCondition );

        ExpressionSyntax annotatedWhenTrue;
        ExpressionSyntax annotatedWhenFalse;
        ScopeContext? scopeContext;

        if ( conditionScope.GetExpressionExecutionScope( true ) == TemplatingScope.CompileTimeOnly )
        {
            scopeContext = null;
        }
        else
        {
            scopeContext = this._currentScopeContext.RunTimePreferred( $"run-time conditional expression with the condition '{node.Condition}'" );
        }

        using ( this.WithScopeContext( scopeContext ) )
        {
            annotatedWhenTrue = this.Visit( node.WhenTrue );
            annotatedWhenFalse = this.Visit( node.WhenFalse );
        }

        var combinedScope = TemplatingScope.RunTimeOnly;

        // Mark the whole expression as compile-time only if all three sub-expressions are compile-time
        if ( scopeContext == null &&
             this.GetNodeScope( annotatedWhenTrue ).GetExpressionExecutionScope( true ) == TemplatingScope.CompileTimeOnly &&
             this.GetNodeScope( annotatedWhenFalse ).GetExpressionExecutionScope( true ) == TemplatingScope.CompileTimeOnly )
        {
            combinedScope = TemplatingScope.CompileTimeOnly;
        }

        return node.Update(
                annotatedCondition,
                node.QuestionToken,
                annotatedWhenTrue,
                node.ColonToken,
                annotatedWhenFalse )
            .AddScopeAnnotation( combinedScope );
    }

    public override SyntaxNode VisitForStatement( ForStatementSyntax node )
    {
        // This is a quick-and-dirty implementation that all for statements runtime.

        if ( node.Declaration != null )
        {
            this.RequireScope( node.Declaration.Variables, TemplatingScope.RunTimeOnly, "variable of a 'for' loop" );
        }

        var transformedVariableDeclaration = this.Visit( node.Declaration );

        // ReSharper disable once RedundantSuppressNullableWarningExpression
        var transformedInitializers = node.Initializers.SelectAsEnumerable( i => this.Visit( i )! );
        var transformedCondition = this.Visit( node.Condition );

        // ReSharper disable once RedundantSuppressNullableWarningExpression
        var transformedIncrementors = node.Incrementors.SelectAsEnumerable( syntax => this.Visit( syntax )! );

        StatementSyntax transformedStatement;

        using ( this.WithScopeContext(
                   this._currentScopeContext.BreakOrContinue(
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

    public override SyntaxNode VisitWhileStatement( WhileStatementSyntax node )
    {
        // The scope of a `while` statement is determined by its condition only.

        var annotatedCondition = this.Visit( node.Condition );
        var conditionScope = this.GetNodeScope( annotatedCondition ).GetExpressionExecutionScope().ReplaceIndeterminate( TemplatingScope.CompileTimeOnly );

        this.RequireLoopScope( node.Condition, conditionScope, "while" );

        StatementSyntax annotatedStatement;

        using ( this.WithScopeContext( this._currentScopeContext.BreakOrContinue( conditionScope, $"while ( {node.Condition} )" ) ) )
        {
            annotatedStatement = this.Visit( node.Statement ).ReplaceScopeAnnotation( conditionScope );
        }

        return node.Update(
                node.AttributeLists,
                node.WhileKeyword,
                node.OpenParenToken,
                annotatedCondition,
                node.CloseParenToken,
                annotatedStatement.AddTargetScopeAnnotation( conditionScope ) )
            .AddScopeAnnotation( conditionScope );
    }

    public override SyntaxNode VisitReturnStatement( ReturnStatementSyntax node )
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

    public override SyntaxNode VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
    {
        var reason = $"local function '{node.Identifier.Text}'";

        using ( this.WithScopeContext( this._currentScopeContext.ForbidDynamic( reason ) ) )
        {
            this.Visit( node.ReturnType );
            this.Visit( node.ParameterList );
        }

        using ( this.WithScopeContext( this._currentScopeContext.RunTimeConditional( reason ) ) )
        {
            if ( node.ExpressionBody != null )
            {
                var transformedExpression = this.Visit( node.ExpressionBody.Expression );

                return node.WithExpressionBody( node.ExpressionBody.WithExpression( transformedExpression ) ).AddScopeAnnotation( TemplatingScope.RunTimeOnly );
            }
            else
            {
                var transformedBody = this.Visit( node.Body );

                return node.WithBody( transformedBody ).AddScopeAnnotation( TemplatingScope.RunTimeOnly );
            }
        }
    }

    public override SyntaxNode? VisitAnonymousMethodExpression( AnonymousMethodExpressionSyntax node )
    {
        this.ReportUnsupportedLanguageFeature( node.DelegateKeyword, "anonymous method" );

        return base.VisitAnonymousMethodExpression( node );
    }

    public override SyntaxNode VisitQueryExpression( QueryExpressionSyntax node )
    {
        this.ReportUnsupportedLanguageFeature( node.FromClause.FromKeyword, "LINQ" );

        return node;
    }

    public override SyntaxNode VisitAwaitExpression( AwaitExpressionSyntax node )
    {
        // Await is always run-time.

        ExpressionSyntax transformedExpression;

        using ( this.WithScopeContext( this._currentScopeContext.RunTimePreferred( "'await' expression" ) ) )
        {
            transformedExpression = this.Visit( node.Expression );
        }

        return node.WithExpression( transformedExpression ).AddScopeAnnotation( TemplatingScope.RunTimeOnly );
    }

    public override SyntaxNode VisitYieldStatement( YieldStatementSyntax node )
    {
        // Yield is always run-time.

        ExpressionSyntax? transformedExpression;

        if ( node.Expression != null )
        {
            using ( this.WithScopeContext( this._currentScopeContext.RunTimePreferred( "'yield' expression" ) ) )
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

    private SyntaxNode? VisitLambdaExpression<T>( T node, Func<T, SyntaxNode?> callBase )
        where T : LambdaExpressionSyntax
    {
        if ( node.ExpressionBody != null )
        {
            var annotatedExpression = this.Visit( node.ExpressionBody );

            return node.WithExpressionBody( annotatedExpression ).WithScopeAnnotationFrom( annotatedExpression );
        }
        else
        {
            // it means Expression is a Block
            this.ReportUnsupportedLanguageFeature( node.ArrowToken, "statement lambda" );

            return callBase( node );
        }
    }

    public override SyntaxNode? VisitParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node )
    {
        if ( node.ReturnType != null && this._templateMemberClassifier.IsNodeOfDynamicType( node.ReturnType ) )
        {
            this.ReportDiagnostic(
                TemplatingDiagnosticDescriptors.ForbiddenDynamicUseInTemplate,
                node.ReturnType.GetLocation(),
                default );
        }

        foreach ( var parameter in node.ParameterList.Parameters )
        {
            if ( parameter.Type != null && this._templateMemberClassifier.IsNodeOfDynamicType( parameter.Type ) )
            {
                this.ReportDiagnostic(
                    TemplatingDiagnosticDescriptors.ForbiddenDynamicUseInTemplate,
                    parameter.Type.GetLocation(),
                    default );
            }
        }

        return this.VisitLambdaExpression( node, base.VisitParenthesizedLambdaExpression );
    }

    public override SyntaxNode? VisitSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
        => this.VisitLambdaExpression( node, base.VisitSimpleLambdaExpression );

    #endregion

    #region Switch

    public override SyntaxNode VisitSwitchExpressionArm( SwitchExpressionArmSyntax node )
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
            combinedScope = this.GetSwitchCaseScope( transformedPattern, transformedWhen, transformedExpression, node );
        }

        return node.Update(
                transformedPattern,
                transformedWhen,
                node.EqualsGreaterThanToken,
                transformedExpression )
            .AddScopeAnnotation( combinedScope )
            .AddTargetScopeAnnotation( TemplatingScope.MustFollowParent );
    }

    public override SyntaxNode VisitSwitchExpression( SwitchExpressionSyntax node )
    {
        var transformedGoverningExpression = this.Visit( node.GoverningExpression );
        var governingExpressionScope = transformedGoverningExpression.GetScopeFromAnnotation().GetValueOrDefault();

        if ( (governingExpressionScope == TemplatingScope.CompileTimeOnly
              && this._templateMemberClassifier.IsNodeOfDynamicType( transformedGoverningExpression ))
             || governingExpressionScope != TemplatingScope.CompileTimeOnly )
        {
            governingExpressionScope = TemplatingScope.RunTimeOnly;
        }

        var armContext = governingExpressionScope == TemplatingScope.CompileTimeOnly
            ? this._currentScopeContext.CompileTimeOnly( "a compile-time switch expression" )
            : null;

        SwitchExpressionArmSyntax[] transformedArms;

        using ( this.WithScopeContext( armContext ) )
        {
            transformedArms = node.Arms.SelectAsArray( a => this.Visit( a ) );

            var reason = governingExpressionScope == TemplatingScope.RunTimeOnly
                ? "a run-time switch expression"
                : "a compile-time switch expression";

            this.RequireScope( transformedArms, governingExpressionScope, reason );
        }

        return node.Update(
                transformedGoverningExpression,
                node.SwitchKeyword,
                node.OpenBraceToken,
                SeparatedList( transformedArms, node.Arms.GetSeparators() ),
                node.CloseBraceToken )
            .AddScopeAnnotation( governingExpressionScope );
    }

    public override SyntaxNode VisitSwitchStatement( SwitchStatementSyntax node )
    {
        var annotatedExpression = this.Visit( node.Expression );
        var expressionScope = annotatedExpression.GetScopeFromAnnotation().GetValueOrDefault();

        TemplatingScope switchScope;
        string scopeReason;

        if ( (expressionScope == TemplatingScope.CompileTimeOnly && this._templateMemberClassifier.IsNodeOfDynamicType( annotatedExpression ))
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
                ? this._currentScopeContext.CompileTimeOnly( scopeReason )
                : this._currentScopeContext.RunTimePreferred( scopeReason );

            using ( this.WithScopeContext( labelContext ) )
            {
                transformedLabels = section.Labels.SelectAsArray( this.Visit )!;

                if ( this.RequireScope( transformedLabels, switchScope, scopeReason ) )
                {
                    transformedLabels = transformedLabels.SelectAsArray( l => l.ReplaceScopeAnnotation( switchScope ) );
                }
                else
                {
                    // We would have an error if we replace the annotation.
                }
            }

            using ( this.WithScopeContext( this._currentScopeContext.BreakOrContinue( switchScope, scopeReason ) ) )
            {
                // Statements of a compile-time control block must have an explicitly-set scope otherwise the template compiler
                // will look at the scope in the parent node, which is here incorrect.
                transformedStatements = section.Statements.SelectAsArray( s => this.Visit( s ).AddRunTimeOnlyAnnotationIfUndetermined() );
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
        SyntaxNode? transformedExpression,
        SyntaxNode originalNode )
        => this.GetExpressionScope( new[] { transformedPattern, transformedWhen, transformedExpression }, originalNode );

    public override SyntaxNode VisitCasePatternSwitchLabel( CasePatternSwitchLabelSyntax node )
    {
        var transformedPattern = this.Visit( node.Pattern );
        var patternScope = this.GetNodeScope( transformedPattern );
        var transformedWhen = this.Visit( node.WhenClause );

        var combinedScope = patternScope == TemplatingScope.CompileTimeOnly
            ? TemplatingScope.CompileTimeOnly
            : this.GetSwitchCaseScope( transformedPattern, transformedWhen, null, node );

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

        if ( existingScope != TemplatingScope.RunTimeOrCompileTime && existingScope != requiredScope )
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

    public override SyntaxNode VisitLockStatement( LockStatementSyntax node )
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

    public override SyntaxNode VisitUsingStatement( UsingStatementSyntax node )
    {
        var annotatedExpression = this.Visit( node.Expression );
        var annotatedDeclaration = this.Visit( node.Declaration );
        var annotatedStatement = this.Visit( node.Statement );

        this.RequireScope( annotatedExpression, TemplatingScope.RunTimeOnly, "a 'using' statement" );
        this.RequireScope( annotatedDeclaration, TemplatingScope.RunTimeOnly, "a 'using' statement" );

        return node.Update(
                node.AwaitKeyword,
                node.UsingKeyword,
                node.OpenParenToken,
                annotatedDeclaration,
                annotatedExpression,
                node.CloseParenToken,
                annotatedStatement )
            .AddScopeAnnotation( TemplatingScope.RunTimeOnly );
    }

    public override SyntaxNode VisitArrayType( ArrayTypeSyntax node )
    {
        var transformedNode = (ArrayTypeSyntax) base.VisitArrayType( node )!;

        var scope = this.GetNodeScope( transformedNode.ElementType );

        if ( scope == TemplatingScope.Dynamic )
        {
            // We cannot have an array of dynamic.
            this.ReportDiagnostic( TemplatingDiagnosticDescriptors.InvalidDynamicTypeConstruction, node, node.ToString() );
        }

        return transformedNode;
    }

    public override SyntaxNode VisitRefType( RefTypeSyntax node )
    {
        var transformedNode = (RefTypeSyntax) base.VisitRefType( node )!;

        var scope = this.GetNodeScope( transformedNode.Type );

        if ( scope == TemplatingScope.Dynamic )
        {
            // We cannot have a ref to dynamic.
            this.ReportDiagnostic( TemplatingDiagnosticDescriptors.InvalidDynamicTypeConstruction, node, node.ToString() );
        }

        return transformedNode;
    }

    private T VisitGenericNameCore<T>( T node, Func<T, SyntaxNode> callBase, Func<TemplatingScope, T, T>? addColor = null )
        where T : SyntaxNode
    {
        addColor ??= ( _, n ) => n;

        var scope = this.GetNodeScope( node );
        T transformedNode;

        switch ( scope )
        {
            case TemplatingScope.Conflict:
                this.ReportScopeError( node );

                // We continue with an unknown scope because other methods don't handle the Conflict scope.
                scope = TemplatingScope.RunTimeOrCompileTime;
                transformedNode = node;

                break;

            case TemplatingScope.DynamicTypeConstruction:
                // We cannot have generic type instances of dynamic.
                this.ReportDiagnostic( TemplatingDiagnosticDescriptors.InvalidDynamicTypeConstruction, node, node.ToString() );

                return node;

            default:
                if ( scope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
                {
                    var context = this._currentScopeContext.CompileTimeOnly( "a generic argument of compile-time declaration" );

                    using ( this.WithScopeContext( context ) )
                    {
                        transformedNode = (T) callBase( node );
                    }
                }
                else if ( scope.GetExpressionExecutionScope() == TemplatingScope.RunTimeOnly )
                {
                    var context = this._currentScopeContext.RunTimePreferred( "a generic argument of run-time declaration" );

                    using ( this.WithScopeContext( context ) )
                    {
                        transformedNode = (T) callBase( node );
                    }
                }
                else
                {
                    transformedNode = (T) callBase( node );
                }

                break;
        }

        transformedNode = addColor( scope, transformedNode );

        return transformedNode.AddScopeAnnotation( scope );
    }

    public override SyntaxNode VisitGenericName( GenericNameSyntax node )
    {
        var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

        // If any type parameters on a member are explicitly 'dynamic', report error.
        // (Types have their own error, LAMA0227.)
        if ( symbol is not ITypeSymbol )
        {
            foreach ( var argument in node.TypeArgumentList.Arguments )
            {
                if ( this._templateMemberClassifier.IsNodeOfDynamicType( argument ) )
                {
                    this.ReportDiagnostic(
                        TemplatingDiagnosticDescriptors.ForbiddenDynamicUseInTemplate,
                        argument.GetLocation(),
                        default );
                }
            }
        }

        return this.VisitGenericNameCore(
            node,
            base.VisitGenericName!,
            ( scope, transformedNode ) =>
            {
                var annotatedIdentifier = this.AddColoringAnnotations( node.Identifier, symbol, scope ).AsToken();

                return transformedNode.WithIdentifier( annotatedIdentifier );
            } );
    }

    public override SyntaxNode VisitTupleType( TupleTypeSyntax node ) => this.VisitGenericNameCore( node, base.VisitTupleType! );

    public override SyntaxNode VisitNullableType( NullableTypeSyntax node )
    {
        var transformedElementType = this.Visit( node.ElementType );
        var transformedNode = node.WithElementType( transformedElementType );

        var elementScope = transformedElementType.GetScopeFromAnnotation();

        if ( elementScope != null )
        {
            transformedNode = transformedNode.AddScopeAnnotation( elementScope.Value.GetExpressionValueScope() );
        }

        return transformedNode;
    }

    public override SyntaxNode VisitObjectCreationExpression( ObjectCreationExpressionSyntax node )
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
            context = this._currentScopeContext.CompileTimeOnly( $"the creation of an instance of the compile-time {objectType}" );
        }

        using ( this.WithScopeContext( context ) )
        {
            var transformedArguments = node.ArgumentList?.Arguments.SelectAsArray( a => this.Visit( a ) );
            var argumentsScope = this.GetExpressionScope( transformedArguments, node );
            var transformedInitializer = this.Visit( node.Initializer );
            var initializerScope = this.GetNodeScope( transformedInitializer );

            var combinedScope = objectTypeScope switch
            {
                TemplatingScope.CompileTimeOnly => TemplatingScope.CompileTimeOnly,
                TemplatingScope.RunTimeOnly => TemplatingScope.RunTimeOnly,
                _ => this.GetExpressionScope(
                    new SyntaxNode?[] { node.ArgumentList, transformedInitializer },
                    new[] { argumentsScope, initializerScope },
                    node )
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

    public override SyntaxNode VisitWithExpression( WithExpressionSyntax node )
    {
        // The scope is determined by the expression and the initializer must comply.
        var transformedExpression = this.Visit( node.Expression );
        var expressionScope = this.GetNodeScope( transformedExpression ).GetExpressionValueScope();

        var scopeContext = expressionScope switch
        {
            TemplatingScope.RunTimeOnly =>
                this._currentScopeContext.RunTimePreferred( "on the right side of a 'with' initializer whose left side is run-time" ),
            TemplatingScope.CompileTimeOnly => this._currentScopeContext.CompileTimeOnly(
                "on the right side of a 'with' initializer whose left side is compile-time" ),
            _ => null
        };

        using ( this.WithScopeContext( scopeContext ) )
        {
            var transformedInitializer = this.Visit( node.Initializer );

            var scope = expressionScope == TemplatingScope.RunTimeOrCompileTime
                ? this.GetExpressionScope(
                    new SyntaxNode[] { node.Expression, node.Initializer },
                    new[] { expressionScope, this.GetNodeScope( transformedInitializer ) },
                    node )
                : expressionScope;

            return node.Update( transformedExpression, node.WithKeyword, transformedInitializer ).AddScopeAnnotation( scope );
        }
    }

    public override SyntaxNode VisitThrowExpression( ThrowExpressionSyntax node )
    {
        using ( this.WithScopeContext( this._currentScopeContext.RunTimePreferred( "an expression of a 'throw' expression" ) ) )
        {
            var transformedExpression = this.Visit( node.Expression );

            this.RequireScope( transformedExpression, TemplatingScope.RunTimeOnly, "a 'throw' expression" );

            return node.Update( node.ThrowKeyword, transformedExpression ).AddScopeAnnotation( TemplatingScope.RunTimeOnly );
        }
    }

    public override SyntaxNode VisitThrowStatement( ThrowStatementSyntax node )
    {
        using ( this.WithScopeContext( this._currentScopeContext.RunTimePreferred( "an expression of a 'throw' statement" ) ) )
        {
            var transformedExpression = this.Visit( node.Expression )!;

            this.RequireScope( transformedExpression, TemplatingScope.RunTimeOnly, "a 'throw' statement" );

            return node.Update( node.ThrowKeyword, transformedExpression, node.SemicolonToken ).AddScopeAnnotation( TemplatingScope.RunTimeOnly );
        }
    }

    public override SyntaxNode VisitTryStatement( TryStatementSyntax node )
    {
        var annotatedBlock = this.Visit( node.Block );

        var annotatedCatches = new CatchClauseSyntax[node.Catches.Count];

        for ( var i = 0; i < node.Catches.Count; i++ )
        {
            var @catch = node.Catches[i];

            using ( this.WithScopeContext( this._currentScopeContext.RunTimeConditional( "catch" ) ) )
            {
                var annotatedCatch = this.Visit( @catch );
                annotatedCatches[i] = annotatedCatch;
            }
        }

        FinallyClauseSyntax? annotatedFinally = null;

        if ( node.Finally != null )
        {
            using ( this.WithScopeContext( this._currentScopeContext.RunTimeConditional( "finally" ) ) )
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

    public override SyntaxNode VisitTypeOfExpression( TypeOfExpressionSyntax node )
    {
        // The processing of typeof(.) is very specific. It is always represented as a compile-time expression 
        // There is then compile-time-to-run-time conversion logic in the rewriter.
        // The value of typeof is scope-neutral except if the type is run-time only.

        TypeSyntax annotatedType;

        using ( this.WithScopeContext( this._currentScopeContext.RunTimeOrCompileTime( "typeof" ) ) )
        {
            annotatedType = this.Visit( node.Type );
        }

        var typeScope = this.GetNodeScope( annotatedType );

        var typeOfScope = typeScope.GetExpressionValueScope() switch
        {
            TemplatingScope.CompileTimeOnly => TemplatingScope.CompileTimeOnly,
            TemplatingScope.RunTimeOnly => ReferencesTemplateParameter() ? TemplatingScope.TypeOfTemplateTypeParameter : TemplatingScope.TypeOfRunTimeType,
            TemplatingScope.RunTimeOrCompileTime => TemplatingScope.CompileTimeOnlyReturningBoth,
            _ => throw new AssertionFailedException( $"Unexpected templating scope: {typeScope.GetExpressionExecutionScope()}." )
        };

        return node.WithType( annotatedType ).AddScopeAnnotation( typeOfScope );

        bool ReferencesTemplateParameter()
        {
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node.Type );

            return symbol != null && this._typeParameterDetectionVisitor.Visit( symbol );
        }
    }

    public override SyntaxNode VisitArrayRankSpecifier( ArrayRankSpecifierSyntax node )
    {
        // ReSharper disable once RedundantSuppressNullableWarningExpression
        var transformedSizes = node.Sizes.SelectAsImmutableArray( syntax => this.Visit( syntax )! );

        var sizeScope = this.GetExpressionScope( transformedSizes, node );

        var arrayRankScope = sizeScope.GetExpressionValueScope() switch
        {
            TemplatingScope.RunTimeOnly => TemplatingScope.RunTimeOnly,
            TemplatingScope.CompileTimeOnly => TemplatingScope.RunTimeOrCompileTime,
            TemplatingScope.RunTimeOrCompileTime => TemplatingScope.RunTimeOrCompileTime,
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

    public override SyntaxNode VisitTupleExpression( TupleExpressionSyntax node )
    {
        var transformedElements = node.Arguments.SelectAsImmutableArray( a => this.Visit( a.Expression ) );
        var tupleScope = this.GetExpressionScope( transformedElements, node ).GetExpressionValueScope( true );
        var transformedArguments = new ArgumentSyntax[transformedElements.Length];

        for ( var i = 0; i < transformedElements.Length; i++ )
        {
            transformedArguments[i] = node.Arguments[i].WithExpression( transformedElements[i] );
        }

        return node.Update( node.OpenParenToken, SeparatedList( transformedArguments, node.Arguments.GetSeparators() ), node.CloseParenToken )
            .AddScopeAnnotation( tupleScope );
    }

    public override SyntaxNode VisitInterpolatedStringExpression( InterpolatedStringExpressionSyntax node )
    {
        var transformedContents = new List<InterpolatedStringContentSyntax>( node.Contents.Count );

        foreach ( var content in node.Contents )
        {
            switch ( content )
            {
                case InterpolatedStringTextSyntax text:
                    transformedContents.Add( text );

                    break;

                case InterpolationSyntax interpolation:
                    var transformedExpression = this.Visit( interpolation.Expression );
                    var expressionScope = transformedExpression.GetScopeFromAnnotation().GetValueOrDefault( TemplatingScope.RunTimeOrCompileTime );
                    var interpolationScope = expressionScope;

                    if ( expressionScope == TemplatingScope.CompileTimeOnly )
                    {
                        // Implicit toString conversion.
                        interpolationScope = TemplatingScope.CompileTimeOnlyReturningBoth;
                    }
                    else if ( expressionScope.GetExpressionValueScope() == TemplatingScope.RunTimeOnly )
                    {
                        interpolationScope = TemplatingScope.RunTimeOnly;
                    }

                    transformedContents.Add( interpolation.WithExpression( transformedExpression ).AddScopeAnnotation( interpolationScope ) );

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected content kind {content.Kind()} at '{content.GetLocation()}.'" );
            }
        }

        var totalScope = this.GetExpressionScope( transformedContents, node );

        return node.WithContents( List( transformedContents ) ).AddScopeAnnotation( totalScope );
    }

    public override SyntaxNode VisitInitializerExpression( InitializerExpressionSyntax node )
        => base.VisitInitializerExpression( node )!.AddTargetScopeAnnotation( TemplatingScope.MustFollowParent );

    public override SyntaxNode? VisitDefaultExpression( DefaultExpressionSyntax node )
    {
        if ( this._templateMemberClassifier.IsNodeOfDynamicType( node.Type ) )
        {
            this.ReportDiagnostic(
                TemplatingDiagnosticDescriptors.ForbiddenDynamicUseInTemplate,
                node.Type.GetLocation(),
                default );
        }

        return base.VisitDefaultExpression( node );
    }
}