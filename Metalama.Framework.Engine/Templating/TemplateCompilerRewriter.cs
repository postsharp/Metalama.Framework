// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// ReSharper disable RedundantUsingDirective

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.Templating;

/// <summary>
/// Compiles the source code of a template, annotated with <see cref="TemplateAnnotator"/>,
/// to an executable template.
/// </summary>
internal sealed partial class TemplateCompilerRewriter : MetaSyntaxRewriter, IDiagnosticAdder
{
    private const string _rewrittenTypeOfAnnotation = "Metalama.RewrittenTypeOf";

    private readonly TemplateCompilerSemantics _syntaxKind;
    private readonly Compilation _runTimeCompilation;
    private readonly string _templateName;
    private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;
    private readonly IDiagnosticAdder _diagnosticAdder;
    private readonly CancellationToken _cancellationToken;
    private readonly SerializableTypes _serializableTypes;
    private readonly TemplateMetaSyntaxFactoryImpl _templateMetaSyntaxFactory;
    private readonly TemplateMemberClassifier _templateMemberClassifier;
    private readonly CompileTimeOnlyRewriter _compileTimeOnlyRewriter;
    private readonly TypeOfRewriter _typeOfRewriter;
    private readonly TypeSyntax _templateTypeArgumentType;
    private readonly HashSet<string> _templateCompileTimeTypeParameterNames = new();
    private readonly TypeSyntax _templateSyntaxFactoryType;
    private readonly TypeSyntax _dictionaryOfITypeType;
    private readonly TypeSyntax _dictionaryOfTypeSyntaxType;

    private MetaContext? _currentMetaContext;
    private int _nextStatementListId;
    private ISymbol? _rootTemplateSymbol;

    public TemplateCompilerRewriter(
        string templateName,
        TemplateCompilerSemantics syntaxKind,
        Compilation runTimeCompilation,
        Compilation compileTimeCompilation,
        SyntaxTreeAnnotationMap syntaxTreeAnnotationMap,
        IDiagnosticAdder diagnosticAdder,
        CompilationServices compileTimeCompilationServices,
        SerializableTypes serializableTypes,
        RoslynApiVersion targetApiVersion,
        CancellationToken cancellationToken ) : base( compileTimeCompilationServices.ServiceProvider, compileTimeCompilation, targetApiVersion )
    {
        this._templateName = templateName;
        this._syntaxKind = syntaxKind;
        this._runTimeCompilation = runTimeCompilation;
        this._syntaxTreeAnnotationMap = syntaxTreeAnnotationMap;
        this._diagnosticAdder = diagnosticAdder;
        this._cancellationToken = cancellationToken;
        this._serializableTypes = serializableTypes;
        this._templateMetaSyntaxFactory = new TemplateMetaSyntaxFactoryImpl();

        this._templateMemberClassifier = new TemplateMemberClassifier(
            runTimeCompilation,
            syntaxTreeAnnotationMap,
            compileTimeCompilationServices.ServiceProvider );

        this._compileTimeOnlyRewriter = new CompileTimeOnlyRewriter( this );

        var syntaxGenerationContext = compileTimeCompilationServices.GetSyntaxGenerationContext();
        this._typeOfRewriter = new TypeOfRewriter( syntaxGenerationContext );

        this._templateTypeArgumentType =
            syntaxGenerationContext.SyntaxGenerator.Type( this.MetaSyntaxFactory.ReflectionMapper.GetTypeSymbol( typeof(TemplateTypeArgument) ) );

        this._templateSyntaxFactoryType =
            syntaxGenerationContext.SyntaxGenerator.Type( this.MetaSyntaxFactory.ReflectionMapper.GetTypeSymbol( typeof(ITemplateSyntaxFactory) ) );

        this._dictionaryOfTypeSyntaxType =
            syntaxGenerationContext.SyntaxGenerator.Type( this.MetaSyntaxFactory.ReflectionMapper.GetTypeSymbol( typeof(Dictionary<string, TypeSyntax>) ) );

        this._dictionaryOfITypeType =
            syntaxGenerationContext.SyntaxGenerator.Type( this.MetaSyntaxFactory.ReflectionMapper.GetTypeSymbol( typeof(Dictionary<string, IType>) ) );
    }

    public bool Success { get; private set; } = true;

    public void Report( Diagnostic diagnostic )
    {
        this._diagnosticAdder.Report( diagnostic );

        if ( diagnostic.Severity == DiagnosticSeverity.Error )
        {
            this.Success = false;
        }
    }

    private static string NormalizeSpace( string statementComment )
    {
        statementComment = statementComment.Replace( '\n', ' ' ).Replace( '\r', ' ' );

        while ( true )
        {
            var old = statementComment;
            statementComment = statementComment.ReplaceOrdinal( "  ", " " );

            if ( old == statementComment )
            {
                return statementComment;
            }
        }
    }

    public override bool VisitIntoStructuredTrivia => false;

    /// <summary>
    /// Sets the current <see cref="MetaContext"/> for the current execution context. To be used in a <c>using</c> statement.
    /// </summary>
    /// <param name="newMetaContext"></param>
    /// <returns></returns>
    private MetaContextCookie WithMetaContext( MetaContext newMetaContext )
    {
        var cookie = new MetaContextCookie( this, this._currentMetaContext );

        this._currentMetaContext = newMetaContext;

        return cookie;
    }

    /// <summary>
    /// Generates the code to generate a run-time symbol name (i.e. a call to <see cref="ITemplateSyntaxFactory.GetUniqueIdentifier"/>),
    /// adds this code to the list of statements of the current <see cref="MetaContext"/>, and returns the identifier of
    /// the compiled template that contains the run-time symbol name.
    /// </summary>
    /// <param name="symbol">The symbol in the source template.</param>
    /// <returns>The identifier of the compiled template that contains the run-time symbol name.</returns>
    private IdentifierNameSyntax ReserveRunTimeSymbolName( ISymbol symbol )
    {
        var metaVariableIdentifier = this._currentMetaContext!.GetTemplateVariableName( symbol );

        var callGetUniqueIdentifier = this._templateMetaSyntaxFactory.GetUniqueIdentifier( symbol.Name );

        var localDeclaration =
            LocalDeclarationStatement(
                    VariableDeclaration( this.MetaSyntaxFactory.Type( typeof(SyntaxToken) ) )
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator( metaVariableIdentifier )
                                    .WithInitializer( EqualsValueClause( callGetUniqueIdentifier ) ) ) ) )
                .NormalizeWhitespace();

        this._currentMetaContext!.Statements.Add( localDeclaration );

        return IdentifierName( metaVariableIdentifier );
    }

    /// <summary>
    /// Determines how a <see cref="SyntaxNode"/> should be transformed:
    /// <see cref="MetaSyntaxRewriter.TransformationKind.None"/> for compile-time code
    /// or <see cref="MetaSyntaxRewriter.TransformationKind.Transform"/> for run-time code.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override TransformationKind GetTransformationKind( SyntaxNode node )
        => IsCompileTimeCode( node ) ? TransformationKind.None : TransformationKind.Transform;

    internal static bool IsCompileTimeCode( SyntaxNode node )
    {
        var targetScope = node.GetTargetScopeFromAnnotation();

        switch ( targetScope )
        {
            case TemplatingScope.RunTimeOnly:
                return false;

            case TemplatingScope.CompileTimeOnly:
                return true;

            case TemplatingScope.MustFollowParent:
                return GetFromParent();
        }

        var scope = node.GetScopeFromAnnotation().GetValueOrDefault( TemplatingScope.RunTimeOrCompileTime );

        // Take a decision from the node if we can.
        if ( scope.IsUndetermined() )
        {
            return GetFromParent();
        }
        else
        {
            // If we have a scope annotation, follow it.
            return !scope.MustBeTransformed();
        }

        bool GetFromParent()
        {
            // Look for annotation on the parent, but stop at 'if' and 'foreach' statements,
            // which have special interpretation.
            var parent = node.Parent;

            switch ( parent )
            {
                case null:
                    // This situation seems to happen only when Transform is called from a newly created syntax node,
                    // which has not been added to the syntax tree yet. Transform then calls Visit and, which then calls GetTransformationKind
                    // so we need to return Transform here. This is not nice and would need to be refactored.

                    return false;

                case IfStatementSyntax:
                case ForEachStatementSyntax:
                case ElseClauseSyntax:
                case WhileStatementSyntax:
                case SwitchSectionSyntax:
                    throw new AssertionFailedException( $"The node '{node}' must be annotated." );

                default:
                    return IsCompileTimeCode( parent );
            }
        }
    }

    private T TransformCompileTimeCode<T>( T node )
        where T : SyntaxNode
        => (T) this._compileTimeOnlyRewriter.Visit( node )!;

    protected override SyntaxNode? VisitCore( SyntaxNode? node )
    {
        if ( node == null )
        {
            return null;
        }

        this._cancellationToken.ThrowIfCancellationRequested();

        // Captures the root symbol.
        if ( this._rootTemplateSymbol == null )
        {
            if ( node == null )
            {
                throw new ArgumentNullException( nameof(node) );
            }

            this._rootTemplateSymbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node );

            if ( this._rootTemplateSymbol == null )
            {
                throw new AssertionFailedException( "Didn't find a symbol for a template method node." );
            }
        }

        if ( node.GetTargetScopeFromAnnotation() == TemplatingScope.RunTimeOnly &&
             node.GetScopeFromAnnotation().GetValueOrDefault().GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
        {
            // The node itself does not need to be transformed because it is compile time, but it needs to be converted
            // into a run-time value. However, calls to variants of Proceed must be transformed into calls to the standard Proceed.
            return this.CreateRunTimeExpression( (ExpressionSyntax) this.TransformCompileTimeCode( node ) );
        }
        else
        {
            return base.VisitCore( node );
        }
    }

    public override SyntaxNode VisitTupleExpression( TupleExpressionSyntax node )
    {
        var qualifiedTuple = this.AddTupleNames( node );

        return base.VisitTupleExpression( qualifiedTuple );
    }

    private TupleExpressionSyntax AddTupleNames( TupleExpressionSyntax node )
    {
        // Tuples can be initialized from variables and then items take names from variable name
        // but variable name is not safe and could be renamed because of target variables 
        // in this case we initialize tuple with explicit names.
        var tupleType = (INamedTypeSymbol?) this._syntaxTreeAnnotationMap.GetExpressionType( node );

        if ( tupleType == null )
        {
            // We may fail to get the tuple type if it has an element with the `default` keyword, i.e. `(default, "")`.
            throw new AssertionFailedException( $"Cannot get the type of tuple '{node}'." );
        }

        var transformedArguments = new ArgumentSyntax[node.Arguments.Count];

        for ( var i = 0; i < tupleType.TupleElements.Length; i++ )
        {
            var tupleElement = tupleType.TupleElements[i];
            ArgumentSyntax arg;

            if ( !tupleElement.Name.Equals( tupleElement.CorrespondingTupleField!.Name, StringComparison.Ordinal ) )
            {
                var name = tupleType.TupleElements[i].Name;
                arg = node.Arguments[i].WithNameColon( NameColon( name ) );
            }
            else
            {
                arg = node.Arguments[i];
            }

            transformedArguments[i] = arg;
        }

        var qualifiedTuple = node.WithArguments( default(SeparatedSyntaxList<ArgumentSyntax>).AddRange( transformedArguments ) );

        return qualifiedTuple;
    }

    protected override ExpressionSyntax Transform( SyntaxToken token )
    {
        // Following renaming of local variables cannot be apply for TupleElement  
        if ( token.IsKind( SyntaxKind.IdentifierToken ) && token.Parent != null && token.Parent is not TupleElementSyntax )
        {
            // Transforms identifier declarations (local variables and local functions). Local identifiers must have
            // a unique name in the target code, which is unknown when the template is compiled, therefore local identifiers
            // get their name dynamically at expansion time. The ReserveRunTimeSymbolName method generates code that
            // reserves the name at expansion time. The result is stored in a local variable of the expanded template.
            // Then, each template reference uses this local variable.

            var identifierSymbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( token.Parent! );

            if ( this.IsLocalSymbol( identifierSymbol! ) )
            {
                if ( !this._currentMetaContext!.TryGetRunTimeSymbolLocal( identifierSymbol!, out _ ) )
                {
                    var declaredSymbolNameLocal = this.ReserveRunTimeSymbolName( identifierSymbol! ).Identifier;
                    this._currentMetaContext.AddRunTimeSymbolLocal( identifierSymbol!, declaredSymbolNameLocal );

                    return IdentifierName( declaredSymbolNameLocal.Text );
                }
                else
                {
                    throw new AssertionFailedException( $"The local variable {identifierSymbol} has not been annotated." );
                }
            }
            else
            {
                // This is not a symbol declaration but a symbol reference.
            }
        }

        var transformedToken = base.Transform( token );

        var tokenKind = transformedToken.Kind().ToString();

        if ( tokenKind.EndsWith( "Keyword", StringComparison.Ordinal ) )
        {
            transformedToken = transformedToken.WithTrailingTrivia( ElasticSpace );
        }

        return transformedToken;
    }

    protected override ExpressionSyntax TransformVariableDeclaration( VariableDeclarationSyntax node )
    {
        switch ( node )
        {
            case { Type: NullableTypeSyntax { ElementType: IdentifierNameSyntax { Identifier: { Text: "dynamic" } } } }:
                // Variable of dynamic? type needs to become var type (without the ?).
                return base.TransformVariableDeclaration(
                    VariableDeclaration(
                        IdentifierName( Identifier( "var" ) ),
                        node.Variables ) );

            default:
                return base.TransformVariableDeclaration( node );
        }
    }

    protected override ExpressionSyntax TransformIdentifierName( IdentifierNameSyntax node )
    {
        switch ( node.Identifier.Kind() )
        {
            case SyntaxKind.GlobalKeyword:
            case SyntaxKind.VarKeyword:
                return base.TransformIdentifierName( node );

            case SyntaxKind.IdentifierToken:
                return this.TransformIdentifierToken( node );

            default:
                throw new AssertionFailedException( $"Unexpected identifier kind: {node.Identifier.Kind()}." );
        }
    }

    /// <summary>
    /// Determines is a symbol is local to the current template.
    /// </summary>
    private bool IsLocalSymbol( ISymbol? symbol )
        => symbol switch
        {
            IMethodSymbol { MethodKind: MethodKind.LocalFunction or MethodKind.AnonymousFunction or MethodKind.LambdaMethod } or ILocalSymbol => true,
            IParameterSymbol or ITypeParameterSymbol => this.IsLocalSymbol( symbol.ContainingSymbol ),
            _ => false
        };

    protected override ExpressionSyntax TransformNullableType( NullableTypeSyntax node )
    {
        if ( node.ElementType is IdentifierNameSyntax identifier && string.Equals( identifier.Identifier.Text, "dynamic", StringComparison.Ordinal ) )
        {
            // Avoid transforming "dynamic?" into "var?".
            return base.TransformIdentifierName( IdentifierName( Identifier( "var" ) ) );
        }
        else
        {
            return base.TransformNullableType( node );
        }
    }

    private ExpressionSyntax TransformIdentifierToken( IdentifierNameSyntax node )
    {
        if ( string.Equals( node.Identifier.Text, "dynamic", StringComparison.Ordinal ) )
        {
            // We change all dynamic into var in the template.
            return base.TransformIdentifierName( IdentifierName( Identifier( "var" ) ) );
        }

        // If the identifier is declared withing the template, the expanded name is given by the TemplateExpansionContext and
        // stored in a template variable named __foo, where foo is the variable name in the template. This variable is defined
        // and initialized in the VisitVariableDeclarator.
        // For identifiers declared outside of the template we just call the regular Roslyn SyntaxFactory.IdentifierName().
        var identifierSymbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

        if ( this.IsLocalSymbol( identifierSymbol! ) )
        {
            if ( this._currentMetaContext!.TryGetRunTimeSymbolLocal( identifierSymbol!, out var declaredSymbolNameLocal ) )
            {
                return this.MetaSyntaxFactory.IdentifierName( IdentifierName( declaredSymbolNameLocal.Text ) );
            }
            else if ( identifierSymbol is IParameterSymbol parameterSymbol
                      && SymbolEqualityComparer.Default.Equals( parameterSymbol.ContainingSymbol, this._rootTemplateSymbol ) )
            {
                // We have a reference to a template parameter. Currently, only introductions can have template parameters, and these don't need
                // to be renamed.

                return base.TransformIdentifierName( node );
            }
            else
            {
                // That should not happen in a correct compilation because IdentifierName is used only for an identifier reference, not an identifier definition.
                // Identifier definitions should be processed by Transform(SyntaxToken).

                // However, this can happen in an incorrect/incomplete compilation. In this case, returning anything is fine.
                this.Report(
                    TemplatingDiagnosticDescriptors.UndeclaredRunTimeIdentifier.CreateRoslynDiagnostic(
                        this._syntaxTreeAnnotationMap.GetLocation( node ),
                        node.Identifier.Text ) );

                this.Success = false;
            }
        }

        return this.MetaSyntaxFactory.IdentifierName( SyntaxFactoryEx.LiteralExpression( node.Identifier.Text ) );
    }

    protected override ExpressionSyntax TransformArgument( ArgumentSyntax node )
    {
        // The base implementation is very verbose, so we use this one:
        if ( node.RefKindKeyword.IsKind( SyntaxKind.None ) )
        {
            var transformedExpression = this.Transform( node.Expression );
            var transformedArgument = this.MetaSyntaxFactory.Argument( SyntaxFactoryEx.Null, SyntaxFactoryEx.Default, transformedExpression );

            if ( node.NameColon != null )
            {
                var transformedNameColon = this.TransformNameColon( node.NameColon );

                transformedArgument =
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                transformedArgument,
                                IdentifierName( "WithNameColon" ) ) )
                        .WithArgumentList( ArgumentList( SingletonSeparatedList( Argument( transformedNameColon ) ) ) );
            }

            return transformedArgument.WithTemplateAnnotationsFrom( node );
        }
        else
        {
            return base.TransformArgument( node );
        }
    }

    protected override ExpressionSyntax TransformStatement( StatementSyntax statement )
        =>

            // We can get here when the parent node is a run-time `if` or `foreach` and the current node a compile-time statement
            // that is not a block. The easiest approach is to wrap the statement into a block.
            (ExpressionSyntax) this.BuildRunTimeBlock( Block( statement ), true );

    protected override ExpressionSyntax TransformExpression( ExpressionSyntax expression, ExpressionSyntax originalExpression )
        => this.CreateRunTimeExpression( expression );

    /// <summary>
    /// Transforms an <see cref="ExpressionSyntax"/> that instantiates a <see cref="TypedExpressionSyntaxImpl"/>
    /// that represents the input.
    /// </summary>
    private ExpressionSyntax CreateRunTimeExpression( ExpressionSyntax expression )
    {
        switch ( expression.Kind() )
        {
            // TODO: We need to transform null and default values though. How to do this right then?
            case SyntaxKind.NullLiteralExpression:
            case SyntaxKind.DefaultLiteralExpression:
                return InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.RuntimeExpression) ) )
                    .AddArgumentListArguments( Argument( this.MetaSyntaxFactory.LiteralExpression( this.Transform( expression.Kind() ) ) ) );

            case SyntaxKind.DefaultExpression:
                return InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.RuntimeExpression) ) )
                    .AddArgumentListArguments(
                        Argument( this.MetaSyntaxFactory.DefaultExpression( (ExpressionSyntax) this.Visit( ((DefaultExpressionSyntax) expression).Type )! ) ) );

            case SyntaxKind.IdentifierName:
                {
                    var identifierName = (IdentifierNameSyntax) expression;

                    if ( identifierName.IsVar )
                    {
                        return this.TransformIdentifierName( (IdentifierNameSyntax) expression );
                    }

                    break;
                }

            case SyntaxKind.InvocationExpression:
                {
                    var typeOfAnnotation = expression.GetAnnotations( _rewrittenTypeOfAnnotation ).FirstOrDefault();

                    if ( typeOfAnnotation != null )
                    {
                        return InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.TypeOf) ) )
                            .AddArgumentListArguments(
                                Argument( SyntaxFactoryEx.LiteralExpression( typeOfAnnotation.Data! ) ),
                                Argument(
                                    this.CreateTypeParameterSubstitutionDictionary(
                                        nameof(TemplateTypeArgument.SyntaxWithoutNullabilityAnnotations),
                                        this._dictionaryOfTypeSyntaxType ) ) );
                    }

                    break;
                }

            case SyntaxKind.SimpleLambdaExpression:
                break;

            case SyntaxKind.ThisExpression:
                // Cannot use 'this' in a context that expects a run-time expression.
                var location = this._syntaxTreeAnnotationMap.GetLocation( expression );

                // Find a meaningful parent exception.
                var parentExpression = expression
                                           .Ancestors()
                                           .FirstOrDefault( n => n is InvocationExpressionSyntax or BinaryExpressionSyntax )
                                       ?? expression;

                this.Report( TemplatingDiagnosticDescriptors.CannotUseThisInRunTimeContext.CreateRoslynDiagnostic( location, parentExpression.ToString() ) );

                return expression;

            case SyntaxKind.TypeOfExpression:
                {
                    var type = (ITypeSymbol) this._syntaxTreeAnnotationMap.GetSymbol( ((TypeOfExpressionSyntax) expression).Type ).AssertNotNull();
                    var typeId = SerializableTypeIdProvider.GetId( type ).Id;

                    return InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.TypeOf) ) )
                        .AddArgumentListArguments(
                            Argument( SyntaxFactoryEx.LiteralExpression( typeId ) ),
                            Argument(
                                this.CreateTypeParameterSubstitutionDictionary( nameof(TemplateTypeArgument.Syntax), this._dictionaryOfTypeSyntaxType ) ) );
                }
        }

        var symbol = this._syntaxTreeAnnotationMap.GetSymbol( expression );

        var expressionType = this._syntaxTreeAnnotationMap.GetExpressionType( expression );

        if ( expressionType == null )
        {
            // This seems to happen with lambda expressions in a method that cannot be resolved.
            expressionType = this._runTimeCompilation.GetSpecialType( SpecialType.System_Object );
        }

        if ( symbol is IParameterSymbol parameter && this._templateMemberClassifier.IsRunTimeTemplateParameter( parameter ) )
        {
            // Run-time template parameters are always bound to a run-time meta-expression.
            return expression;
        }
        else if ( symbol is ITypeParameterSymbol typeParameter && this._templateMemberClassifier.IsCompileTimeTemplateTypeParameter( typeParameter ) )
        {
            return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName( nameof(TemplateTypeArgument.Syntax) ) );
        }

        // A local function that wraps the input `expression` into a LiteralExpression.
        ExpressionSyntax CreateRunTimeExpressionForLiteralCreateExpressionFactory( SyntaxKind syntaxKind )
        {
            InvocationExpressionSyntax literalExpression;

            if ( syntaxKind == SyntaxKind.StringLiteralExpression )
            {
                literalExpression = InvocationExpression(
                        this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.StringLiteralExpression) ) )
                    .AddArgumentListArguments( Argument( expression ) );
            }
            else
            {
                literalExpression = this.MetaSyntaxFactory.LiteralExpression(
                    this.Transform( syntaxKind ),
                    this.MetaSyntaxFactory.Literal( expression ) );
            }

            return InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.RuntimeExpression) ) )
                .AddArgumentListArguments(
                    Argument( literalExpression ),
                    Argument(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal( DocumentationCommentId.CreateDeclarationId( expressionType ) ) ) ) );
        }

        if ( expressionType is IErrorTypeSymbol )
        {
            // There is a compile-time error. Return default.
            return LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) );
        }

        // ReSharper disable once ConstantConditionalAccessQualifier
        switch ( expressionType.Name )
        {
            case "dynamic":
            case "Task" when expressionType is INamedTypeSymbol { IsGenericType: true } namedType && namedType.TypeArguments[0] is IDynamicTypeSymbol &&
                             expressionType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks":
            case "IEnumerable" or "IEnumerator" or "IAsyncEnumerable" or "IAsyncEnumerator"
                when expressionType is INamedTypeSymbol { IsGenericType: true } namedType2 && namedType2.TypeArguments[0] is IDynamicTypeSymbol &&
                     expressionType.ContainingNamespace.ToDisplayString() == "System.Collections.Generic":

                return InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.GetDynamicSyntax) ) )
                    .AddArgumentListArguments(
                        Argument( SyntaxFactoryEx.SafeCastExpression( NullableType( PredefinedType( Token( SyntaxKind.ObjectKeyword ) ) ), expression ) ) );

            case "String":
                return CreateRunTimeExpressionForLiteralCreateExpressionFactory( SyntaxKind.StringLiteralExpression );

            case "Int32":
            case "Int16":
            case "Int64":
            case "UInt32":
            case "UInt16":
            case "UInt64":
            case "Byte":
            case "SByte":
            case nameof(Single):
            case nameof(Double):
                return CreateRunTimeExpressionForLiteralCreateExpressionFactory( SyntaxKind.NumericLiteralExpression );

            case nameof(Char):
                return CreateRunTimeExpressionForLiteralCreateExpressionFactory( SyntaxKind.CharacterLiteralExpression );

            case nameof(Boolean):
                return InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.RuntimeExpression) ) )
                    .AddArgumentListArguments(
                        Argument(
                            InvocationExpression( this.MetaSyntaxFactory.SyntaxFactoryMethod( nameof(LiteralExpression) ) )
                                .AddArgumentListArguments(
                                    Argument(
                                        InvocationExpression(
                                                this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.Boolean) ) )
                                            .AddArgumentListArguments( Argument( expression ) ) ) ) ),
                        Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( "T:System.Boolean" ) ) ) );

            case null:
                throw new AssertionFailedException( $"Cannot convert {expression.Kind()} '{expression}' to a run-time value." );

            default:
                // Try to find a serializer for this type. If the object type is simply 'object', we will resolve it at expansion time.
                if ( expressionType.SpecialType == SpecialType.System_Object ||
                     this._serializableTypes.IsSerializable( expressionType, this._syntaxTreeAnnotationMap.GetLocation( expression ), this ) )
                {
                    return InvocationExpression(
                        this._templateMetaSyntaxFactory.GenericTemplateSyntaxFactoryMember(
                            nameof(ITemplateSyntaxFactory.Serialize),
                            this.MetaSyntaxFactory.Type( expressionType ) ),
                        ArgumentList( SingletonSeparatedList( Argument( expression ) ) ) );
                }
                else
                {
                    // We don't have a valid tree, but let the compilation continue. The call to IsSerializable wrote a diagnostic.
                    return LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) );
                }
        }
    }

    private ExpressionSyntax CreateTypeParameterSubstitutionDictionary( string propertyName, TypeSyntax dictionaryType )
    {
        if ( this._templateCompileTimeTypeParameterNames.Count == 0 )
        {
            return SyntaxFactoryEx.Null;
        }
        else
        {
            return ObjectCreationExpression( dictionaryType )
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SeparatedList<ExpressionSyntax>(
                            this._templateCompileTimeTypeParameterNames.SelectEnumerable(
                                name =>
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        ImplicitElementAccess()
                                            .WithArgumentList(
                                                BracketedArgumentList(
                                                    SingletonSeparatedList(
                                                        Argument(
                                                            LiteralExpression(
                                                                SyntaxKind.StringLiteralExpression,
                                                                Literal( name ) ) ) ) ) ),
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName( name ),
                                            IdentifierName( propertyName ) ) ) ) ) ) )
                .NormalizeWhitespace();
        }
    }

    public override SyntaxNode VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
    {
        if ( this.TryVisitNamespaceOrTypeName( node, out var transformedNode ) )
        {
            return transformedNode;
        }

        var transformationKind = this.GetTransformationKind( node.Expression );

        // Cast to dynamic expressions.
        if ( transformationKind != TransformationKind.Transform &&
             this._syntaxTreeAnnotationMap.GetExpressionType( node.Expression ) is IDynamicTypeSymbol &&
             !this._templateMemberClassifier.IsTemplateParameter( node.Expression ) )
        {
            return InvocationExpression(
                this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.DynamicMemberAccessExpression) ),
                ArgumentList(
                    SeparatedList(
                        new[]
                        {
                            Argument( this.CastToDynamicExpression( (ExpressionSyntax) this.Visit( node.Expression )! ) ),
                            Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( node.Name.Identifier.ValueText ) ) )
                        } ) ) );
        }

        return base.VisitMemberAccessExpression( node );
    }

    public override SyntaxNode? VisitExpressionStatement( ExpressionStatementSyntax node )
    {
        // The default implementation has to be overridden because VisitInvocationExpression can
        // return null in case of pragma. In this case, the ExpressionStatement must return null too.
        // In the default implementation, such case would result in an exception.

        if ( this.GetTransformationKind( node ) == TransformationKind.Transform
             || this._templateMemberClassifier.IsNodeOfDynamicType( node.Expression ) )
        {
            return this.TransformExpressionStatement( node );
        }
        else
        {
            var transformedExpression = this.Visit( node.Expression );

            if ( transformedExpression == null )
            {
                return null;
            }
            else
            {
                return node.Update(
                    this.VisitList( node.AttributeLists ),
                    (ExpressionSyntax) transformedExpression,
                    this.VisitToken( node.SemicolonToken ) );
            }
        }
    }

    protected override ExpressionSyntax TransformExpressionStatement( ExpressionStatementSyntax node )
    {
        if ( node.Expression is AssignmentExpressionSyntax { Left: IdentifierNameSyntax { Identifier: { Text: "_" } } } assignment )
        {
            if ( this.IsCompileTimeDynamic( assignment.Right ) )
            {
                // Process the statement "_ = meta.XXX()", where "meta.XXX()" is a call to a compile-time dynamic method. 

                var invocationExpression = InvocationExpression(
                        this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.DynamicDiscardAssignment) ) )
                    .AddArgumentListArguments(
                        Argument( this.CastToDynamicExpression( this.TransformCompileTimeCode( assignment.Right ) ) ),
                        Argument( LiteralExpression( SyntaxKind.FalseLiteralExpression ) ) );

                return this.WithCallToAddSimplifierAnnotation( invocationExpression );
            }
            else if ( assignment.Right is AwaitExpressionSyntax awaitExpression && this.IsCompileTimeDynamic( awaitExpression.Expression ) )
            {
                // Process the statement "_ = await meta.XXX()", where "meta.XXX()" is a call to a compile-time dynamic method. 

                var invocationExpression = InvocationExpression(
                        this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.DynamicDiscardAssignment) ) )
                    .AddArgumentListArguments(
                        Argument( this.CastToDynamicExpression( this.TransformCompileTimeCode( awaitExpression.Expression ) ) ),
                        Argument( LiteralExpression( SyntaxKind.TrueLiteralExpression ) ) );

                return this.WithCallToAddSimplifierAnnotation( invocationExpression );
            }
        }

        var expression = this.Transform( node.Expression );

        var toArrayStatementExpression = InvocationExpression(
            this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.ToStatement) ),
            ArgumentList( SingletonSeparatedList( Argument( expression ) ) ) );

        return toArrayStatementExpression;
    }

    public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
    {
        var transformationKind = this.GetTransformationKind( node );

        if ( node.IsNameOf() )
        {
            // nameof is always transformed into a literal except when it is a template parameter.

            var expression = node.ArgumentList.Arguments[0].Expression;
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( expression );

            if ( symbol is IParameterSymbol parameter && this._templateMemberClassifier.IsRunTimeTemplateParameter( parameter ) )
            {
                return this.MetaSyntaxFactory.InvocationExpression(
                    this.MetaSyntaxFactory.IdentifierName(
                        this.MetaSyntaxFactory.Identifier(
                            SyntaxFactoryEx.Default,
                            this.MetaSyntaxFactory.Kind( SyntaxKind.NameOfKeyword ),
                            SyntaxFactoryEx.LiteralExpression( "nameof" ),
                            SyntaxFactoryEx.LiteralExpression( "nameof" ),
                            SyntaxFactoryEx.Default ) ),
                    this.MetaSyntaxFactory.ArgumentList(
                        this.MetaSyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                            this.MetaSyntaxFactory.Argument( SyntaxFactoryEx.Default, SyntaxFactoryEx.Default, expression ) ) ) );
            }

            var symbolName = symbol?.Name ?? "<error>";

            if ( transformationKind == TransformationKind.Transform )
            {
                return this.MetaSyntaxFactory.LiteralExpression(
                    this.MetaSyntaxFactory.Kind( SyntaxKind.StringLiteralExpression ),
                    this.MetaSyntaxFactory.Literal( symbolName ) );
            }
            else
            {
                return SyntaxFactoryEx.LiteralExpression( symbolName );
            }
        }
        else if ( this._compileTimeOnlyRewriter.TryRewriteProceedInvocation( node, out var proceedNode ) )
        {
            return proceedNode;
        }

        if ( transformationKind != TransformationKind.Transform &&
             node.ArgumentList.Arguments.Any( a => this._templateMemberClassifier.IsDynamicParameter( a ) ) )
        {
            // We are transforming a call to a compile-time method that accepts dynamic arguments.

            SyntaxNode? LocalTransformArgument( ArgumentSyntax a )
            {
                if ( this._templateMemberClassifier.IsDynamicParameter( a ) )
                {
                    var expressionScope = a.Expression.GetScopeFromAnnotation().GetValueOrDefault();
                    var transformedExpression = (ExpressionSyntax) this.Visit( a.Expression )!;

                    switch ( expressionScope )
                    {
                        case TemplatingScope.Dynamic:
                        case TemplatingScope.RunTimeOnly:
                            return Argument( transformedExpression );

                        default:
                            return Argument( this.CreateRunTimeExpression( transformedExpression ) );
                    }
                }
                else
                {
                    return this.Visit( a );
                }
            }

            var transformedArguments = node.ArgumentList.Arguments.SelectArray( syntax => LocalTransformArgument( syntax )! );

            return node.Update(
                (ExpressionSyntax) this.Visit( node.Expression )!,
                ArgumentList( SeparatedList( transformedArguments )! ) );
        }
        else if ( this._templateMemberClassifier.IsNodeOfDynamicType( node.Expression ) )
        {
            // We are in an invocation like: `meta.This.Foo(...)`.
        }
        else if ( this._templateMemberClassifier.IsRunTimeMethod( node.Expression ) )
        {
            // Replace `meta.RunTime(x)` to `x`.
            var expression = node.ArgumentList.Arguments[0].Expression;

            if ( this.GetTransformationKind( expression ) == TransformationKind.None )
            {
                return this.CreateRunTimeExpression( expression );
            }
            else
            {
                return this.Visit( expression );
            }
        }
        else
        {
            // Process special methods.

            switch ( this._templateMemberClassifier.GetMetaMemberKind( node.Expression ) )
            {
                case MetaMemberKind.InsertComment:
                    {
                        var transformedArgumentList = (ArgumentListSyntax) this.Visit( node.ArgumentList )!;

                        var arguments = transformedArgumentList.Arguments.Insert(
                            0,
                            Argument( IdentifierName( this._currentMetaContext!.StatementListVariableName ) ) );

                        // TemplateSyntaxFactory.AddComments( __s, comments );

                        var addCommentsMetaStatement =
                            ExpressionStatement(
                                InvocationExpression(
                                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.AddComments) ),
                                    ArgumentList( arguments ) ) );

                        var addCommentsStatement = this.DeepIndent(
                            addCommentsMetaStatement.WithLeadingTrivia(
                                TriviaList( Comment( "// " + node.Parent!.WithoutTrivia().ToFullString() ), ElasticCarriageReturnLineFeed )
                                    .AddRange( addCommentsMetaStatement.GetLeadingTrivia() ) ) );

                        this._currentMetaContext.Statements.Add( addCommentsStatement );

                        return null;
                    }

                case MetaMemberKind.InsertStatement:
                    {
                        // TemplateSyntaxFactory.AddStatement( __s, comments );
                        var arguments = node.ArgumentList.Arguments.Insert(
                            0,
                            Argument( IdentifierName( this._currentMetaContext!.StatementListVariableName ) ) );

                        var addStatementMetaStatement =
                            ExpressionStatement(
                                InvocationExpression(
                                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.AddStatement) ),
                                    ArgumentList( arguments ) ) );

                        var addStatementStatement = this.DeepIndent(
                            addStatementMetaStatement.WithLeadingTrivia(
                                TriviaList( Comment( "// " + node.Parent!.WithoutTrivia().ToFullString() ), ElasticCarriageReturnLineFeed )
                                    .AddRange( addStatementMetaStatement.GetLeadingTrivia() ) ) );

                        this._currentMetaContext.Statements.Add( addStatementStatement );

                        return null;
                    }
            }
        }

        // Expand extension methods.
        if ( transformationKind == TransformationKind.Transform )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node.Expression );

            if ( symbol is IMethodSymbol { IsExtensionMethod: true } method )
            {
                var receiver = ((MemberAccessExpressionSyntax) node.Expression).Expression;

                List<ArgumentSyntax> arguments = new( node.ArgumentList.Arguments.Count + 1 ) { Argument( receiver ).WithTemplateAnnotationsFrom( receiver ) };

                arguments.AddRange( node.ArgumentList.Arguments );

                var replacementNode = InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            this.MetaSyntaxFactory.Type( method.ContainingType ),
                            IdentifierName( method.Name ) ),
                        ArgumentList( SeparatedList( arguments ) ) )
                    .WithSymbolAnnotationsFrom( node )
                    .WithTemplateAnnotationsFrom( node );

                var result = this.VisitInvocationExpression( replacementNode );

                return result;
            }
        }

        return base.VisitInvocationExpression( node );
    }

    private ParameterSyntax CreateTemplateSyntaxFactoryParameter()
        => Parameter( default, default, this._templateSyntaxFactoryType, Identifier( TemplateSyntaxFactoryParameterName ), null );

    public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
    {
        if ( node.Body == null && node.ExpressionBody == null )
        {
            // Not supported or incomplete syntax.
            return node;
        }

        this.Indent( 3 );

        // Build the template parameter list.
        var templateParameters =
            new List<ParameterSyntax>( 1 + node.ParameterList.Parameters.Count + (node.TypeParameterList?.Parameters.Count ?? 0) )
            {
                this.CreateTemplateSyntaxFactoryParameter()
            };

        foreach ( var parameter in node.ParameterList.Parameters )
        {
            var templateParameter = parameter;
            var parameterSymbol = (IParameterSymbol) this._syntaxTreeAnnotationMap.GetDeclaredSymbol( parameter ).AssertNotNull();
            var isCompileTime = this._templateMemberClassifier.IsCompileTimeParameter( parameterSymbol );

            if ( !isCompileTime )
            {
                templateParameter =
                    templateParameter
                        .WithType( SyntaxFactoryEx.ExpressionSyntaxType )
                        .WithModifiers( TokenList() )
                        .WithAttributeLists( default );
            }

            templateParameters.Add( templateParameter );
        }

        if ( node.TypeParameterList != null )
        {
            foreach ( var parameter in node.TypeParameterList.Parameters )
            {
                var parameterSymbol = (ITypeParameterSymbol) this._syntaxTreeAnnotationMap.GetDeclaredSymbol( parameter ).AssertNotNull();
                var isCompileTime = this._templateMemberClassifier.IsCompileTimeParameter( parameterSymbol );

                if ( isCompileTime )
                {
                    this._templateCompileTimeTypeParameterNames.Add( parameter.Identifier.Text );

                    templateParameters.Add( Parameter( default, default, this._templateTypeArgumentType, parameter.Identifier, null ) );
                }
            }
        }

        // Build the template body.
        BlockSyntax body;

        if ( node.Body != null )
        {
            body = (BlockSyntax) this.BuildRunTimeBlock( node.Body, false );
        }
        else
        {
            var isVoid = node.ReturnType is PredefinedTypeSyntax predefinedType && predefinedType.Keyword.IsKind( SyntaxKind.VoidKeyword );

            body = (BlockSyntax) this.BuildRunTimeBlock(
                node.ExpressionBody.AssertNotNull().Expression,
                false,
                isVoid );
        }

        var result = this.CreateTemplateMethod( node, body, ParameterList( SeparatedList( templateParameters ) ) );

        this.Unindent( 3 );

        this._templateCompileTimeTypeParameterNames.Clear();

        return result;
    }

    public override SyntaxNode VisitAccessorDeclaration( AccessorDeclarationSyntax node )
    {
        if ( node.Body == null && node.ExpressionBody == null )
        {
            // Not supported or incomplete syntax.
            return node;
        }

        this.Indent( 3 );

        // Create the body.
        BlockSyntax body;

        if ( node.Body != null )
        {
            body = (BlockSyntax) this.BuildRunTimeBlock( node.Body, false );
        }
        else
        {
            var isVoid = !node.Keyword.IsKind( SyntaxKind.GetKeyword );

            body = (BlockSyntax) this.BuildRunTimeBlock(
                node.ExpressionBody.AssertNotNull().Expression,
                false,
                isVoid );
        }

        // Create the parameter list.
        var parameters = node.Keyword.IsKind( SyntaxKind.GetKeyword )
            ? ParameterList( SingletonSeparatedList( this.CreateTemplateSyntaxFactoryParameter() ) )
            : ParameterList(
                SeparatedList(
                    new[]
                    {
                        this.CreateTemplateSyntaxFactoryParameter(),
                        Parameter( default, default, SyntaxFactoryEx.ExpressionSyntaxType, Identifier( "value" ), null )
                    } ) );

        // Create the method.
        var result = this.CreateTemplateMethod( node, body, parameters );

        this.Unindent( 3 );

        return result;
    }

    public override SyntaxNode VisitPropertyDeclaration( PropertyDeclarationSyntax node )
    {
        if ( node.ExpressionBody is { Expression: var bodyExpression } )
        {
            this.Indent( 3 );

            var body = (BlockSyntax) this.BuildRunTimeBlock( bodyExpression, false, false );

            var result = this.CreateTemplateMethod( node, body );

            this.Unindent( 3 );

            return result;
        }
        else if ( node.Initializer is { Value: var initializerExpression } )
        {
            this.Indent( 3 );

            var body = (BlockSyntax) this.BuildRunTimeBlock( initializerExpression, false, false );

            var result = this.CreateTemplateMethod( node, body );

            this.Unindent( 3 );

            return result;
        }
        else
        {
            throw new AssertionFailedException( $"The property has no expression body and no initializer at '{node.GetLocation()}'." );
        }
    }

    public override SyntaxNode VisitVariableDeclarator( VariableDeclaratorSyntax node )
    {
        if ( this._syntaxKind == TemplateCompilerSemantics.Initializer )
        {
            this.Indent( 3 );

            // This is template for field initializer.
            var body = (BlockSyntax) this.BuildRunTimeBlock( node.Initializer.AssertNotNull().Value, false, false );

            var result = this.CreateTemplateMethod( node, body );

            this.Unindent( 3 );

            return result;
        }
        else
        {
            return base.VisitVariableDeclarator( node );
        }
    }

    private MethodDeclarationSyntax CreateTemplateMethod( SyntaxNode node, BlockSyntax body, ParameterListSyntax? parameters = null )
        => MethodDeclaration(
                this.MetaSyntaxFactory.Type( typeof(SyntaxNode) ).WithTrailingTrivia( Space ),
                Identifier( this._templateName ) )
            .WithParameterList( parameters ?? ParameterList( SingletonSeparatedList( this.CreateTemplateSyntaxFactoryParameter() ) ) )
            .WithModifiers( TokenList( Token( SyntaxKind.PublicKeyword ).WithTrailingTrivia( Space ) ) )
            .NormalizeWhitespace()
            .WithBody( body )
            .WithLeadingTrivia( node.GetLeadingTrivia() )
            .WithTrailingTrivia( LineFeed, LineFeed );

    public override SyntaxNode VisitBlock( BlockSyntax node )
    {
        var transformationKind = this.GetTransformationKind( node );

        if ( transformationKind == TransformationKind.Transform )
        {
            return this.BuildRunTimeBlock( node, true );
        }
        else
        {
            using ( this.WithMetaContext( MetaContext.CreateForBuildTimeBlock( this._currentMetaContext! ) ) )
            {
                var metaStatements = this.ToMetaStatements( node.Statements );

                // Add the statements to the parent list.
                this._currentMetaContext!.Statements.AddRange( metaStatements );

                // Returns an empty block intentionally.
                return Block();
            }
        }
    }

    /// <summary>
    /// Generates a run-time block from expression.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="generateExpression"><c>true</c> if the returned <see cref="SyntaxNode"/> must be an
    /// expression (in this case, a delegate invocation is returned), or <c>false</c> if it can be a statement
    /// (in this case, a return statement is returned).</param>
    /// <param name="isVoid"></param>
    /// <returns></returns>
    private SyntaxNode BuildRunTimeBlock( ExpressionSyntax node, bool generateExpression, bool isVoid )
    {
        StatementSyntax statement;

        if ( node is ThrowExpressionSyntax throwExpression )
        {
            statement = ThrowStatement( throwExpression.ThrowKeyword, throwExpression.Expression, Token( SyntaxKind.SemicolonToken ) );
        }
        else
        {
            statement = isVoid ? ExpressionStatement( node ) : ReturnStatement( node );
        }

        return this.BuildRunTimeBlock( () => this.ToMetaStatements( statement ), generateExpression );
    }

    /// <summary>
    /// Generates a run-time block.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="generateExpression"><c>true</c> if the returned <see cref="SyntaxNode"/> must be an
    /// expression (in this case, a delegate invocation is returned), or <c>false</c> if it can be a statement
    /// (in this case, a return statement is returned).</param>
    /// <returns></returns>
    private SyntaxNode BuildRunTimeBlock( BlockSyntax node, bool generateExpression )
        => this.BuildRunTimeBlock( () => this.ToMetaStatements( node.Statements ).ToList(), generateExpression );

    /// <summary>
    /// Generates a run-time block.
    /// </summary>
    /// <param name="createMetaStatements">Function that returns meta statements.</param>
    /// <param name="generateExpression"><c>true</c> if the returned <see cref="SyntaxNode"/> must be an
    /// expression (in this case, a delegate invocation is returned), or <c>false</c> if it can be a statement
    /// (in this case, a return statement is returned).</param>
    /// <returns></returns>
    private SyntaxNode BuildRunTimeBlock( Func<List<StatementSyntax>> createMetaStatements, bool generateExpression )
    {
        using ( this.WithMetaContext( MetaContext.CreateForRunTimeBlock( this._currentMetaContext, $"__s{++this._nextStatementListId}" ) ) )
        {
            // List<StatementOrTrivia> statements = new List<StatementOrTrivia>();
            var listType = this.MetaSyntaxFactory.Type( typeof(List<StatementOrTrivia>) );

            this._currentMetaContext!.Statements.Add(
                LocalDeclarationStatement(
                        VariableDeclaration( listType )
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator( Identifier( this._currentMetaContext.StatementListVariableName ) )
                                        .WithInitializer( EqualsValueClause( ObjectCreationExpression( listType, ArgumentList(), default ) ) ) ) ) )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( this.GetIndentation() ) );

            this._currentMetaContext.Statements.AddRange( createMetaStatements() );

            // TemplateSyntaxFactory.ToStatementList( __s1 )
            var toArrayStatementExpression = InvocationExpression(
                this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.ToStatementList) ),
                ArgumentList( SingletonSeparatedList( Argument( IdentifierName( this._currentMetaContext.StatementListVariableName ) ) ) ) );

            if ( generateExpression )
            {
                // return TemplateSyntaxFactory.ToStatementArray( __s1 );

                var returnStatementSyntax = ReturnStatement( toArrayStatementExpression ).WithLeadingTrivia( this.GetIndentation() ).NormalizeWhitespace();
                this._currentMetaContext.Statements.Add( returnStatementSyntax );

                // Block( Func<SyntaxList<StatementSyntax>>( delegate { ... } )
                return this.DeepIndent(
                    this.MetaSyntaxFactory.Block(
                        SyntaxFactoryEx.Default,
                        InvocationExpression(
                            ObjectCreationExpression( this.MetaSyntaxFactory.Type( typeof(Func<SyntaxList<StatementSyntax>>) ) )
                                .NormalizeWhitespace()
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                AnonymousMethodExpression()
                                                    .WithBody(
                                                        Block( this._currentMetaContext.Statements )
                                                            .AddNoDeepIndentAnnotation() ) ) ) ) ) ) ) );
            }
            else
            {
                // return __s;
                this._currentMetaContext.Statements.Add(
                    ReturnStatement(
                        this.MetaSyntaxFactory.Block( SyntaxFactoryEx.Default, toArrayStatementExpression ).WithLeadingTrivia( this.GetIndentation() ) ) );

                return Block( this._currentMetaContext.Statements );
            }
        }
    }

    /// <summary>
    /// Transforms a list of <see cref="StatementSyntax"/> of the source template into a list of <see cref="StatementSyntax"/> for the compiled
    /// template.
    /// </summary>
    /// <param name="statements"></param>
    /// <returns></returns>
    private IEnumerable<StatementSyntax> ToMetaStatements( in SyntaxList<StatementSyntax> statements ) => statements.SelectMany( this.ToMetaStatements );

    /// <summary>
    /// Transforms a <see cref="StatementSyntax"/> of the source template into a single <see cref="StatementSyntax"/> for the compiled template.
    /// This method is guaranteed to return a single <see cref="StatementSyntax"/>. If the source statement results in several compiled statements,
    /// they will be wrapped into a block.
    /// </summary>
    /// <param name="statement"></param>
    /// <returns></returns>
    private StatementSyntax ToMetaStatement( StatementSyntax statement )
    {
        var statements = this.ToMetaStatements( statement );

        return statements.Count == 1 ? statements[0] : Block( statements );
    }

    /// <summary>
    /// Transforms a <see cref="StatementSyntax"/> of the source template into a list of <see cref="StatementSyntax"/> for the compiled template.
    /// </summary>
    /// <param name="statement">A statement of the source template.</param>
    /// <returns>A list of statements for the compiled template.</returns>
    private List<StatementSyntax> ToMetaStatements( StatementSyntax statement )
    {
        MetaContext newContext;

        if ( statement is BlockSyntax block )
        {
            // Push the build-time template block.
            newContext = MetaContext.CreateForBuildTimeBlock( this._currentMetaContext! );

            using ( this.WithMetaContext( newContext ) )
            {
                // Process all statements in this block.
                foreach ( var childStatement in block.Statements )
                {
                    ProcessStatement( childStatement );
                }
            }
        }
        else
        {
            // Push a new MetaContext so statements got added to a new list of statements, but
            // this MetaContext is neither a run-time nor a compile-time lexical scope. 
            newContext = MetaContext.CreateHelperContext( this._currentMetaContext! );

            using ( this.WithMetaContext( newContext ) )
            {
                ProcessStatement( statement );
            }
        }

        // Returns the statements collected during this call.
        return newContext.Statements;

        void ProcessStatement( StatementSyntax singleStatement )
        {
            var transformedNode = this.Visit( singleStatement );

            switch ( transformedNode )
            {
                case null:
                    break;

                case StatementSyntax statementSyntax:
                    // The statement is already build-time code so there is nothing to transform.

                    newContext.Statements.Add( statementSyntax.WithLeadingTrivia( this.GetIndentation() ) );

                    break;

                case ExpressionSyntax expressionSyntax:
                    {
                        // The statement is run-time code and has been transformed into an expression creating the StatementSyntax.
                        // We need to generate the code adding this code to the list of statements, i.e. `statements.Add( expression )`.

                        // Generate a comment with the template source code.
                        var statementComment = NormalizeSpace( singleStatement.ToString() );

                        if ( statementComment.Length > 120 )
                        {
                            // TODO: handle surrogate pairs correctly
                            statementComment = statementComment.Substring( 0, 117 ) + "...";
                        }

                        var leadingTrivia = TriviaList( ElasticCarriageReturnLineFeed )
                            .AddRange( this.GetIndentation() )
                            .Add( Comment( "// " + statementComment ) )
                            .Add( ElasticCarriageReturnLineFeed )
                            .AddRange( this.GetIndentation() );

                        var trailingTrivia = TriviaList( ElasticCarriageReturnLineFeed, ElasticCarriageReturnLineFeed );

                        // TemplateSyntaxFactory.Add( __s, expression )
                        var add =
                            this.DeepIndent(
                                ExpressionStatement(
                                    InvocationExpression(
                                        this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.AddStatement) ),
                                        ArgumentList(
                                            SeparatedList(
                                                new[]
                                                {
                                                    Argument( IdentifierName( this._currentMetaContext!.StatementListVariableName ) ),
                                                    Argument( expressionSyntax )
                                                } ) ) ) ) );

                        newContext.Statements.Add( add.WithLeadingTrivia( leadingTrivia ).WithTrailingTrivia( trailingTrivia ) );

                        break;
                    }

                default:
                    throw new AssertionFailedException( $"Unexpected node kind {transformedNode.Kind()} at '{singleStatement.GetLocation()};." );
            }
        }
    }

    protected override ExpressionSyntax TransformInterpolatedStringExpression( InterpolatedStringExpressionSyntax node )
    {
        List<ExpressionSyntax> transformedContents = new( node.Contents.Count );

        foreach ( var content in node.Contents )
        {
            switch ( content )
            {
                case InterpolatedStringTextSyntax text:
                    transformedContents.Add( this.TransformInterpolatedStringText( text ) );

                    break;

                case InterpolationSyntax interpolation:
                    if ( this.GetTransformationKind( interpolation ) == TransformationKind.None &&
                         !interpolation.Expression.IsKind( SyntaxKind.TypeOfExpression ) )
                    {
                        // We have a compile-time interpolation (e.g. formatting string argument).
                        // We can evaluate it at compile time and add it as a text content.

                        // typeof was skipped because it is always annotated as compile time but actually always transformed.

                        var compileTimeInterpolatedString =
                            InterpolatedStringExpression(
                                Token( SyntaxKind.InterpolatedStringStartToken ),
                                SingletonList<InterpolatedStringContentSyntax>( interpolation ),
                                Token( SyntaxKind.InterpolatedStringEndToken ) );

                        var token = this.MetaSyntaxFactory.Token(
                            LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) ),
                            this.Transform( SyntaxKind.InterpolatedStringTextToken ),
                            compileTimeInterpolatedString,
                            compileTimeInterpolatedString,
                            LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) ) );

                        transformedContents.Add( this.MetaSyntaxFactory.InterpolatedStringText( token ) );
                    }
                    else
                    {
                        var transformedInterpolation = this.TransformInterpolation( interpolation );
                        transformedContents.Add( transformedInterpolation );
                    }

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected content {content.Kind()} at '{content.GetLocation()}'." );
            }
        }

        this.Indent();

        var createInterpolatedString = InvocationExpression( this.MetaSyntaxFactory.SyntaxFactoryMethod( nameof(InterpolatedStringExpression) ) )
            .WithArgumentList(
                ArgumentList(
                    SeparatedList<ArgumentSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Argument( this.Transform( node.StringStartToken ) ).WithLeadingTrivia( this.GetIndentation() ),
                            Token( SyntaxKind.CommaToken ).WithTrailingTrivia( GetLineBreak() ),
                            Argument( this.MetaSyntaxFactory.List<InterpolatedStringContentSyntax>( transformedContents ) )
                                .WithLeadingTrivia( this.GetIndentation() ),
                            Token( SyntaxKind.CommaToken ).WithTrailingTrivia( GetLineBreak() ),
                            Argument( this.Transform( node.StringEndToken ) ).WithLeadingTrivia( this.GetIndentation() )
                        } ) ) );

        var callRender = InvocationExpression(
                this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.RenderInterpolatedString) ),
                ArgumentList( SingletonSeparatedList( Argument( createInterpolatedString ) ) ) )
            .NormalizeWhitespace();

        this.Unindent();

        return callRender;
    }

    protected override ExpressionSyntax TransformInterpolation( InterpolationSyntax node )
    {
        var transformedNode = base.TransformInterpolation( node ).AssertNotNull();

        var fixedNode = InvocationExpression(
                this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.FixInterpolationSyntax) ),
                ArgumentList( SingletonSeparatedList( Argument( transformedNode ) ) ) )
            .NormalizeWhitespace();

        return fixedNode;
    }

    public override SyntaxNode VisitInterpolation( InterpolationSyntax node )
    {
        var transformedNode = base.VisitInterpolation( node );

        if ( transformedNode is InterpolationSyntax transformedInterpolation )
        {
            return InterpolationSyntaxHelper.Fix( transformedInterpolation );
        }
        else
        {
            return transformedNode;
        }
    }

    public override SyntaxNode VisitSwitchStatement( SwitchStatementSyntax node )
    {
        if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
        {
            // Run-time. Just serialize to syntax.
            return this.TransformSwitchStatement( node );
        }
        else
        {
            var transformedSections = new SwitchSectionSyntax[node.Sections.Count];

            for ( var i = 0; i < node.Sections.Count; i++ )
            {
                var section = node.Sections[i];
                var transformedStatements = this.ToMetaStatements( section.Statements );
                transformedSections[i] = SwitchSection( section.Labels, List( transformedStatements ) );
            }

            return SwitchStatement(
                node.SwitchKeyword,
                node.OpenParenToken,
                node.Expression,
                node.CloseParenToken,
                node.OpenBraceToken,
                List( transformedSections ),
                node.CloseBraceToken );
        }
    }

    public override SyntaxNode VisitIfStatement( IfStatementSyntax node )
    {
        if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
        {
            // Run-time if. Just serialize to syntax.
            return this.TransformIfStatement( node );
        }
        else
        {
            var transformedStatement = this.ToMetaStatement( node.Statement );
            var transformedElseStatement = node.Else != null ? this.ToMetaStatement( node.Else.Statement ) : null;

            // The condition may contains constructs like typeof or nameof that need to be transformed.
            var condition = this.TransformCompileTimeCode( node.Condition );

            return IfStatement(
                node.AttributeLists,
                condition,
                transformedStatement,
                transformedElseStatement != null ? ElseClause( transformedElseStatement ) : null );
        }
    }

    public override SyntaxNode VisitWhileStatement( WhileStatementSyntax node )
    {
        if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
        {
            // Run-time while. Just serialize to syntax.
            return this.TransformWhileStatement( node );
        }
        else
        {
            var transformedStatement = this.ToMetaStatement( node.Statement );

            return WhileStatement(
                node.AttributeLists,
                node.Condition,
                transformedStatement );
        }
    }

    public override SyntaxNode VisitForEachStatement( ForEachStatementSyntax node )
    {
        if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
        {
            // Run-time foreach. Just serialize to syntax.
            return this.TransformForEachStatement( node );
        }

        this.Indent();

        var statement = this.ToMetaStatement( node.Statement );

        this.Unindent();

        // The expression may contain typeof, nameof, ...
        var expression = this.TransformCompileTimeCode( node.Expression );

        // It seems that trivia can be lost upstream, there can be a missing one between the 'in' keyword and the expression. Add them to be sure.
        return ForEachStatement(
            node.Type.WithTrailingTrivia( ElasticSpace ),
            node.Identifier.WithTrailingTrivia( ElasticSpace ),
            expression.WithLeadingTrivia( ElasticSpace ),
            statement );
    }

    /// <summary>
    /// Determines if the expression will be transformed into syntax that instantiates an <see cref="IUserExpression"/>.
    /// </summary>
    private bool IsCompileTimeDynamic( ExpressionSyntax? expression )
        => expression != null
           && expression.GetScopeFromAnnotation() == TemplatingScope.CompileTimeOnlyReturningRuntimeOnly
           && !this._templateMemberClassifier.IsTemplateParameter( expression )
           && this.GetTransformationKind( expression ) != TransformationKind.Transform
           && (
               this._syntaxTreeAnnotationMap.GetExpressionType( expression ) is IDynamicTypeSymbol
               || (
                   this._syntaxTreeAnnotationMap.GetExpressionType( expression ) is INamedTypeSymbol
                   {
                       Name: "Task" or "IEnumerable" or "IAsyncEnumerator", TypeArguments: { Length: 1 }
#pragma warning disable SA1513 // Formatting issue
                   } namedType
#pragma warning restore SA1513
                   && namedType.TypeArguments[0] is IDynamicTypeSymbol));

    public override SyntaxNode VisitReturnStatement( ReturnStatementSyntax node )
    {
        InvocationExpressionSyntax invocationExpression;

        if ( this.IsCompileTimeDynamic( node.Expression ) )
        {
            // We have a dynamic parameter. We need to call the second overload of ReturnStatement, the one that accepts the IDynamicExpression
            // itself and not the syntax.
            invocationExpression = CreateInvocationExpression( node.Expression.AssertNotNull(), false );
        }
        else if ( node.Expression is AwaitExpressionSyntax awaitExpression && this.IsCompileTimeDynamic( awaitExpression.Expression ) )
        {
            invocationExpression = CreateInvocationExpression( awaitExpression.Expression, true );
        }
        else
        {
            var expression = this.Transform( node.Expression );

            invocationExpression = InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.ReturnStatement) ) )
                .AddArgumentListArguments( Argument( expression ) );
        }

        InvocationExpressionSyntax CreateInvocationExpression( ExpressionSyntax expression, bool awaitResult )
        {
            return
                InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.DynamicReturnStatement) ) )
                    .AddArgumentListArguments(
                        Argument( this.CastToDynamicExpression( (ExpressionSyntax) this.Visit( expression ).AssertNotNull() ) ),
                        Argument( LiteralExpression( awaitResult ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression ) ) );
        }

        return this.WithCallToAddSimplifierAnnotation( invocationExpression );
    }

    private CastExpressionSyntax CastToDynamicExpression( ExpressionSyntax expression )
        => SyntaxFactoryEx.SafeCastExpression(
            this.MetaSyntaxFactory.Type( typeof(IUserExpression) ),
            expression );

    public override SyntaxNode VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
    {
        var declaration = node.Declaration;

        if ( declaration.Variables.Count == 1 )
        {
            var declarator = declaration.Variables[0];

            if ( declarator.Initializer != null )
            {
                if ( this.IsCompileTimeDynamic( declarator.Initializer.Value ) )
                {
                    // Assigning dynamic to a variable.
                    return this.WithCallToAddSimplifierAnnotation( CreateInvocationExpression( declarator.Initializer.Value, false ) );
                }

                if ( declarator.Initializer is { Value: AwaitExpressionSyntax awaitExpression } && this.IsCompileTimeDynamic( awaitExpression.Expression ) )
                {
                    // Assigning awaited dynamic to a variable.
                    return this.WithCallToAddSimplifierAnnotation( CreateInvocationExpression( awaitExpression.Expression, true ) );
                }

                InvocationExpressionSyntax CreateInvocationExpression(
                    ExpressionSyntax expression,
                    bool awaitResult )
                {
                    return InvocationExpression(
                            this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.DynamicLocalDeclaration) ) )
                        .AddArgumentListArguments(
                            Argument( (ExpressionSyntax) this.Visit( declaration.Type )! ),
                            Argument( this.Transform( declarator.Identifier ) ),
                            Argument( this.CastToDynamicExpression( (ExpressionSyntax) this.Visit( expression ).AssertNotNull() ) ),
                            Argument( LiteralExpression( awaitResult ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression ) ) );
                }
            }
        }

        return base.VisitLocalDeclarationStatement( node );
    }

    public override SyntaxNode VisitIdentifierName( IdentifierNameSyntax node )
    {
        if ( node.Identifier.IsKind( SyntaxKind.IdentifierToken ) && !node.IsVar )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

            if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
            {
                // Fully qualifies simple identifiers.

                if ( symbol is INamespaceOrTypeSymbol namespaceOrType )
                {
                    return this.Transform( OurSyntaxGenerator.CompileTime.TypeOrNamespace( namespaceOrType ) );
                }
                else if ( symbol is { IsStatic: true } && node.Parent is not MemberAccessExpressionSyntax && node.Parent is not AliasQualifiedNameSyntax )
                {
                    switch ( symbol.Kind )
                    {
                        case SymbolKind.Field:
                        case SymbolKind.Property:
                        case SymbolKind.Event:
                        case SymbolKind.Method:
                            // We have an access to a field or method with a "using static", or a non-qualified static member access.
                            return this.MetaSyntaxFactory.MemberAccessExpression(
                                this.MetaSyntaxFactory.Kind( SyntaxKind.SimpleMemberAccessExpression ),
                                this.Transform( OurSyntaxGenerator.CompileTime.TypeOrNamespace( symbol.ContainingType ) ),
                                this.MetaSyntaxFactory.IdentifierName( SyntaxFactoryEx.LiteralExpression( node.Identifier.Text ) ) );
                    }
                }

                // When TryVisitNamespaceOrTypeName calls Transform with the result ot the syntax generator, Transform eventually
                // calls the current method for each compile-time parameter. We need to change it to the value of the template
                // parameter.
                else if ( this._templateCompileTimeTypeParameterNames.Contains( node.Identifier.Text ) )
                {
                    return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName( node.Identifier ),
                        IdentifierName( nameof(TemplateTypeArgument.Syntax) ) );
                }
            }
        }

        return base.VisitIdentifierName( node );
    }

    private ExpressionSyntax WithCallToAddSimplifierAnnotation( ExpressionSyntax expression )
        => InvocationExpression(
            this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.AddSimplifierAnnotations) ),
            ArgumentList( SingletonSeparatedList( Argument( expression ) ) ) );

    /// <summary>
    /// Transforms a type or namespace so that it is fully qualified, but return <c>false</c> if the input <paramref name="node"/>
    /// is not a type or namespace.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="transformedNode"></param>
    /// <returns></returns>
    private bool TryVisitNamespaceOrTypeName( SyntaxNode node, [NotNullWhen( true )] out SyntaxNode? transformedNode )
    {
        var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

        switch ( symbol )
        {
            case INamespaceOrTypeSymbol namespaceOrType:
                // If we have a generic type, we do not write the generic arguments.
                var nameExpression = OurSyntaxGenerator.CompileTime.TypeOrNamespace( namespaceOrType );

                transformedNode = this.GetTransformationKind( node ) == TransformationKind.Transform
                    ? this.WithCallToAddSimplifierAnnotation( this.Transform( nameExpression ) )
                    : nameExpression;

                return true;

            default:
                transformedNode = null;

                return false;
        }
    }

    public override SyntaxNode VisitQualifiedName( QualifiedNameSyntax node )
    {
        if ( this.TryVisitNamespaceOrTypeName( node, out var transformedNode ) )
        {
            return transformedNode;
        }
        else
        {
            return base.VisitQualifiedName( node );
        }
    }

    protected override ExpressionSyntax TransformQualifiedName( QualifiedNameSyntax node )
    {
        var transformed = base.TransformQualifiedName( node );

        if ( node.HasAnnotation( Simplifier.Annotation ) )
        {
            transformed = this.WithCallToAddSimplifierAnnotation( transformed );
        }

        return transformed;
    }

    public override SyntaxNode VisitAliasQualifiedName( AliasQualifiedNameSyntax node )
    {
        if ( this.TryVisitNamespaceOrTypeName( node, out var transformedNode ) )
        {
            return transformedNode;
        }
        else
        {
            return base.VisitAliasQualifiedName( node );
        }
    }

    public override SyntaxNode VisitGenericName( GenericNameSyntax node )
    {
        if ( this.TryVisitNamespaceOrTypeName( node, out var transformedTypeName ) )
        {
            return transformedTypeName;
        }
        else
        {
            return base.VisitGenericName( node );
        }
    }

    protected override ExpressionSyntax TransformConditionalExpression( ConditionalExpressionSyntax node )
    {
        if ( node.WhenFalse is ThrowExpressionSyntax || node.WhenTrue is ThrowExpressionSyntax )
        {
            // If any of the expressions if a throw exception, we cannot reduce it at compile time because it would generate incorrect syntax.
            return base.TransformConditionalExpression( node );
        }

        var transformedCondition = this.Transform( node.Condition );
        var transformedWhenTrue = this.Transform( node.WhenTrue );
        var transformedWhenFalse = this.Transform( node.WhenFalse );

        return InvocationExpression(
            this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.ConditionalExpression) ),
            ArgumentList( SeparatedList( new[] { Argument( transformedCondition ), Argument( transformedWhenTrue ), Argument( transformedWhenFalse ) } ) ) );
    }

    protected override ExpressionSyntax TransformYieldStatement( YieldStatementSyntax node )
    {
        if ( node.Kind() == SyntaxKind.YieldReturnStatement && node.Expression is InvocationExpressionSyntax invocation &&
             this._templateMemberClassifier.GetMetaMemberKind( invocation.Expression ) == MetaMemberKind.Proceed )
        {
            // We have a 'yield return meta.Proceed()' statement.

            return InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.ConditionalExpression) ) );
        }
        else
        {
            return base.TransformYieldStatement( node );
        }
    }

    protected override ExpressionSyntax TransformPostfixUnaryExpression( PostfixUnaryExpressionSyntax node )
    {
        if ( node.Kind() == SyntaxKind.SuppressNullableWarningExpression )
        {
            return InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.SuppressNullableWarningExpression) ) )
                .WithArgumentList( ArgumentList( SingletonSeparatedList( Argument( (ExpressionSyntax) this.Visit( node.Operand )! ) ) ) );
        }
        else
        {
            return base.TransformPostfixUnaryExpression( node );
        }
    }

    public override SyntaxNode VisitTypeOfExpression( TypeOfExpressionSyntax node )
    {
        if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
        {
            return this.TransformTypeOfExpression( node );
        }
        else if ( this._syntaxTreeAnnotationMap.GetSymbol( node.Type ) is ITypeSymbol typeSymbol )
        {
            var typeId = SerializableTypeIdProvider.GetId( typeSymbol ).Id;

            return this._typeOfRewriter.RewriteTypeOf(
                    typeSymbol,
                    this.CreateTypeParameterSubstitutionDictionary( nameof(TemplateTypeArgument.Type), this._dictionaryOfITypeType ) )
                .WithAdditionalAnnotations( new SyntaxAnnotation( _rewrittenTypeOfAnnotation, typeId ) );
        }

        return base.VisitTypeOfExpression( node );
    }
}