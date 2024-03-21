// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using static Metalama.Framework.Engine.SyntaxGeneration.SyntaxFactoryEx;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;
using TypedConstant = Metalama.Framework.Code.TypedConstant;
using TypeKind = Metalama.Framework.Code.TypeKind;
using VarianceKind = Metalama.Framework.Code.VarianceKind;

namespace Metalama.Framework.Engine.SyntaxGeneration;

#pragma warning disable CA1822

internal partial class ContextualSyntaxGenerator
{
    private static readonly SyntaxGenerator _roslynSyntaxGenerator;

    static ContextualSyntaxGenerator()
    {
        var type = WorkspaceHelper.CSharpWorkspacesAssembly.GetType( "Microsoft.CodeAnalysis.CSharp.CodeGeneration.CSharpSyntaxGenerator" )!;
        var field = type.GetField( "Instance", BindingFlags.Public | BindingFlags.Static )!;
        _roslynSyntaxGenerator = (SyntaxGenerator) field.GetValue( null ).AssertNotNull();
    }

    private readonly ConcurrentDictionary<ITypeSymbol, TypeSyntax> _typeSyntaxCache = new( SymbolEqualityComparer.IncludeNullability );
    private readonly SyntaxGenerationContext _context;

    public bool IsNullAware { get; }

    internal ContextualSyntaxGenerator( SyntaxGenerationContext context, bool nullAware )
    {
        this._context = context;
        this.IsNullAware = nullAware;
    }

    public TypeOfExpressionSyntax TypeOfExpression(
        ITypeSymbol type,
        IReadOnlyDictionary<string, TypeSyntax>? substitutions = null,
        bool keepNullableAnnotations = false )
    {
        var typeSyntax = this.Type( type );

        if ( type is INamedTypeSymbol { IsGenericType: true } namedType )
        {
            if ( namedType.IsGenericTypeDefinition() )
            {
                // In generic definitions, we must remove type arguments.
                typeSyntax = (TypeSyntax) new RemoveTypeArgumentsRewriter().Visit( typeSyntax ).AssertNotNull();
            }
        }

        if ( !keepNullableAnnotations )
        {
            // In regular typeof, we must remove ? annotations of nullable types.
            typeSyntax = (TypeSyntax) new RemoveReferenceNullableAnnotationsRewriter( type ).Visit( typeSyntax )!;
        }

        var dynamicToVarRewriter = new DynamicToVarRewriter();

        // In any typeof, we must change dynamic to object.
        typeSyntax = (TypeSyntax) dynamicToVarRewriter.Visit( typeSyntax ).AssertNotNull();

        SafeSyntaxRewriter rewriter = type switch
        {
            INamedTypeSymbol { IsGenericType: true } genericType when genericType.IsGenericTypeDefinition() => new RemoveTypeArgumentsRewriter(),
            INamedTypeSymbol { IsGenericType: true } => new RemoveReferenceNullableAnnotationsRewriter( type ),
            _ => dynamicToVarRewriter
        };

        var rewrittenTypeSyntax = rewriter.Visit( typeSyntax ).AssertNotNull();

        // Substitute type arguments.
        if ( substitutions is { Count: > 0 } )
        {
            var substitutionRewriter = new SubstitutionRewriter( substitutions );
            rewrittenTypeSyntax = substitutionRewriter.Visit( rewrittenTypeSyntax ).AssertNotNull();
        }

        return (TypeOfExpressionSyntax) _roslynSyntaxGenerator.TypeOfExpression( rewrittenTypeSyntax );
    }

    private sealed class NormalizeSpaceRewriter : SafeSyntaxRewriter
    {
        private readonly string _endOfLine;

        public NormalizeSpaceRewriter( string endOfLine )
        {
            this._endOfLine = endOfLine;
        }

#pragma warning disable LAMA0830 // NormalizeWhitespace is expensive.
        public override SyntaxNode VisitTupleType( TupleTypeSyntax node ) => base.VisitTupleType( node )!.NormalizeWhitespace( eol: this._endOfLine );
#pragma warning restore LAMA0830
    }

    public DefaultExpressionSyntax DefaultExpression( ITypeSymbol typeSymbol )
        => SyntaxFactory.DefaultExpression( this.Type( typeSymbol ) )
            .WithSimplifierAnnotationIfNecessary( this._context );

    public ArrayCreationExpressionSyntax ArrayCreationExpression( TypeSyntax elementType, IEnumerable<SyntaxNode> elements )
    {
        var array = (ArrayCreationExpressionSyntax) _roslynSyntaxGenerator.ArrayCreationExpression( elementType, elements );

        return array.WithType( array.Type.WithSimplifierAnnotationIfNecessary( this._context ) )
            .NormalizeWhitespaceIfNecessary( this._context );
    }

    public TypeSyntax Type( SpecialType specialType )
        => (TypeSyntax) _roslynSyntaxGenerator.TypeExpression( specialType )
            .WithSimplifierAnnotationIfNecessary( this._context );

    public CastExpressionSyntax CastExpression( ITypeSymbol targetTypeSymbol, ExpressionSyntax expression )
    {
        switch ( expression )
        {
            case BinaryExpressionSyntax:
            case ConditionalExpressionSyntax:
            case CastExpressionSyntax:
            case PrefixUnaryExpressionSyntax:
                expression = ParenthesizedExpression( expression );

                break;
        }

        return this.SafeCastExpression( this.Type( targetTypeSymbol ), expression );
    }

    public TypeSyntax TypeOrNamespace( INamespaceOrTypeSymbol symbol )
    {
        TypeSyntax expression;

        switch ( symbol )
        {
            case ITypeSymbol typeSymbol:
                return this.Type( typeSymbol );

            case INamespaceSymbol namespaceSymbol:
                expression = (NameSyntax) _roslynSyntaxGenerator.NameExpression( namespaceSymbol );

                break;

            default:
                throw new AssertionFailedException( $"Unexpected symbol kind: {symbol.Kind}." );
        }

        return expression.WithSimplifierAnnotationIfNecessary( this._context );
    }

    public ThisExpressionSyntax ThisExpression() => (ThisExpressionSyntax) _roslynSyntaxGenerator.ThisExpression();

    public LiteralExpressionSyntax LiteralExpression( object literal )
    {
        var result = (LiteralExpressionSyntax) _roslynSyntaxGenerator.LiteralExpression( literal );

        if ( result.Kind() is SyntaxKind.NullLiteralExpression or SyntaxKind.DefaultLiteralExpression )
        {
            throw new InvalidOperationException( $"The value {literal} could not be represented as a literal expression." );
        }

        return result;
    }

    public IdentifierNameSyntax IdentifierName( string identifier ) => (IdentifierNameSyntax) _roslynSyntaxGenerator.IdentifierName( identifier );

    public TypeSyntax ArrayTypeExpression( TypeSyntax type )
    {
        var arrayType = (ArrayTypeSyntax) _roslynSyntaxGenerator.ArrayTypeExpression( type ).WithSimplifierAnnotationIfNecessary( this._context );

        // Roslyn does not specify the rank properly so it needs to be fixed up.

        return arrayType.WithRankSpecifiers( SingletonList( ArrayRankSpecifier( SingletonSeparatedList<ExpressionSyntax>( OmittedArraySizeExpression() ) ) ) );
    }

    public TypeSyntax ReturnType( IMethod method ) => this.Type( method.ReturnType.GetSymbol() );

    public TypeSyntax PropertyType( IProperty property ) => this.Type( property.Type.GetSymbol() );

    public TypeSyntax IndexerType( IIndexer indexer ) => this.Type( indexer.Type.GetSymbol() );

    public TypeSyntax EventType( IEvent property ) => this.Type( property.Type.GetSymbol() );

    // ReSharper disable once MemberCanBeMadeStatic.Global

#pragma warning disable CA1822 // Can be made static

    public ArgumentListSyntax ArgumentList( IMethodBase method, Func<IParameter, ExpressionSyntax?> expressionFunc )
        =>

            // TODO: optional parameters.
            SyntaxFactory.ArgumentList(
                SeparatedList(
                    method.Parameters.SelectAsImmutableArray(
                        p =>
                            Argument( expressionFunc( p ).AssertNotNull() ) ) ) );
#pragma warning restore CA1822 // Can be made static

    public SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses( IMethod method )
    {
        List<TypeParameterConstraintClauseSyntax>? clauses = null;

        foreach ( var genericParameter in method.TypeParameters )
        {
            List<TypeParameterConstraintSyntax>? constraints = null;

            switch ( genericParameter.TypeKindConstraint )
            {
                case TypeKindConstraint.Class:
                    constraints ??= new List<TypeParameterConstraintSyntax>();
                    var constraint = ClassOrStructConstraint( SyntaxKind.ClassConstraint );

                    if ( genericParameter.HasDefaultConstructorConstraint )
                    {
                        constraint = constraint.WithQuestionToken( Token( SyntaxKind.QuestionToken ) );
                    }

                    constraints.Add( constraint );

                    break;

                case TypeKindConstraint.Struct:
                    constraints ??= new List<TypeParameterConstraintSyntax>();
                    constraints.Add( ClassOrStructConstraint( SyntaxKind.StructConstraint ) );

                    break;

                case TypeKindConstraint.Unmanaged:
                    constraints ??= new List<TypeParameterConstraintSyntax>();

                    constraints.Add(
                        TypeConstraint(
                            SyntaxFactory.IdentifierName( Identifier( default, SyntaxKind.UnmanagedKeyword, "unmanaged", "unmanaged", default ) ) ) );

                    break;

                case TypeKindConstraint.NotNull:
                    constraints ??= new List<TypeParameterConstraintSyntax>();
                    constraints.Add( TypeConstraint( SyntaxFactory.IdentifierName( "notnull" ) ) );

                    break;

                case TypeKindConstraint.Default:
                    constraints ??= new List<TypeParameterConstraintSyntax>();
                    constraints.Add( DefaultConstraint() );

                    break;
            }

            foreach ( var typeConstraint in genericParameter.TypeConstraints )
            {
                constraints ??= new List<TypeParameterConstraintSyntax>();

                constraints.Add( TypeConstraint( this.Type( typeConstraint.GetSymbol() ) ) );
            }

            if ( genericParameter.HasDefaultConstructorConstraint )
            {
                constraints ??= new List<TypeParameterConstraintSyntax>();
                constraints.Add( ConstructorConstraint() );
            }

            if ( constraints != null )
            {
                clauses ??= new List<TypeParameterConstraintClauseSyntax>();

                clauses.Add(
                    TypeParameterConstraintClause(
                        TokenWithTrailingSpace( SyntaxKind.WhereKeyword ),
                        SyntaxFactory.IdentifierName( genericParameter.Name ),
                        Token( SyntaxKind.ColonToken ),
                        SeparatedList( constraints ) ) );
            }
        }

        if ( clauses == null )
        {
            return default;
        }
        else
        {
            return List( clauses );
        }
    }

    public ExpressionSyntax EnumValueExpression( INamedTypeSymbol type, object value )
    {
        var member = type.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault( f => f is { IsConst: true, ConstantValue: { } } && f.ConstantValue.Equals( value ) );

        if ( member == null )
        {
            return this.CastExpression( type, this.LiteralExpression( value ) );
        }
        else
        {
            return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, this.Type( type ), this.IdentifierName( member.Name ) );
        }
    }

    public ExpressionSyntax TypedConstant( in TypedConstant typedConstant )
    {
        if ( typedConstant.IsNullOrDefault )
        {
            return this.DefaultExpression( typedConstant.Type.GetSymbol() );
        }
        else if ( typedConstant.Type is INamedType { TypeKind: TypeKind.Enum } enumType )
        {
            return this.EnumValueExpression( enumType.GetSymbol(), typedConstant.Value! );
        }
        else if ( typedConstant.Value is ImmutableArray<TypedConstant> immutableArray )
        {
            var elementType = typedConstant.Type.AssertCast<IArrayType>().ElementType;

            return this.ArrayCreationExpression(
                this.Type( elementType.GetSymbol() ),
                immutableArray.SelectAsReadOnlyList( item => this.TypedConstant( item ) ) );
        }
        else
        {
            return this.LiteralExpression( typedConstant.Value! );
        }
    }

    // ReSharper disable once MemberCanBeMadeStatic.Global

#pragma warning disable CA1822
    public ExpressionSyntax RenderInterpolatedString( InterpolatedStringExpressionSyntax interpolatedString )
#pragma warning restore CA1822
    {
        List<InterpolatedStringContentSyntax> contents = new( interpolatedString.Contents.Count );

        foreach ( var content in interpolatedString.Contents )
        {
            switch ( content )
            {
                case InterpolatedStringTextSyntax text:
                    var previousIndex = contents.Count - 1;

                    if ( contents.Count > 0 && contents[previousIndex] is InterpolatedStringTextSyntax previousText )
                    {
                        // If we have two adjacent text tokens, we need to merge them, otherwise reformatting will add a white space.

                        var appendedText = previousText.TextToken.ValueText + text.TextToken.ValueText;

                        var escapedTextWithQuotes =
                            Literal( appendedText ).Text.ReplaceOrdinal( "{", "{{" ).ReplaceOrdinal( "}", "}}" );

                        var escapedText = escapedTextWithQuotes.Substring( 1, escapedTextWithQuotes.Length - 2 );

                        contents[previousIndex] = previousText.WithTextToken(
                            Token( default, SyntaxKind.InterpolatedStringTextToken, escapedText, appendedText, default ) );
                    }
                    else
                    {
                        contents.Add( text );
                    }

                    break;

                case InterpolationSyntax interpolation:
                    contents.Add( interpolation );

                    break;
            }
        }

        return interpolatedString.WithContents( List( contents ) );
    }

    public TypeSyntax Type( ITypeSymbol symbol )
    {
        if ( this._context.HasCompilationContext && symbol.BelongsToCompilation( this._context.CompilationContext ) == true )
        {
            return this._typeSyntaxCache.GetOrAdd( symbol, static ( s, x ) => x.TypeCore( s ), this );
        }
        else
        {
            return this.TypeCore( symbol );
        }
    }

    private TypeSyntax TypeCore( ITypeSymbol symbol )
    {
        var typeSyntax = (TypeSyntax) _roslynSyntaxGenerator.TypeExpression( symbol ).WithSimplifierAnnotationIfNecessary( this._context );

        if ( !this.IsNullAware )
        {
            typeSyntax = (TypeSyntax) new RemoveReferenceNullableAnnotationsRewriter( symbol ).Visit( typeSyntax ).AssertNotNull();
        }

        if ( this.Options.NormalizeWhitespace )
        {
            // Just calling NormalizeWhitespaceIfNecessary here produces ugly whitespace, e.g. "typeof(global::System.Int32[, ])".
            typeSyntax = (TypeSyntax) new NormalizeSpaceRewriter( this._context.EndOfLine ).Visit( typeSyntax ).AssertNotNull();
        }

        return typeSyntax;
    }

    protected SyntaxGenerationOptions Options => this._context.Options;

    public AttributeSyntax Attribute( IAttributeData attribute )
    {
        var constructorArguments = attribute.ConstructorArguments.Select( a => AttributeArgument( this.AttributeValueExpression( a ) ) );

        var namedArguments = attribute.NamedArguments.SelectAsImmutableArray(
            a => AttributeArgument(
                NameEquals( a.Key ),
                null,
                this.AttributeValueExpression( a.Value ) ) );

        var attributeSyntax = SyntaxFactory.Attribute( (NameSyntax) this.Type( attribute.Type.GetSymbol() ) );

        var argumentList = AttributeArgumentList( SeparatedList( constructorArguments.Concat( namedArguments ) ) );

        if ( argumentList.Arguments.Count > 0 )
        {
            // Add the argument list only when it is non-empty, otherwise this generates redundant parenthesis.
            attributeSyntax = attributeSyntax.WithArgumentList( argumentList );
        }

        return attributeSyntax;
    }

    public SyntaxList<AttributeListSyntax> AttributesForDeclaration(
        in Ref<IDeclaration> declaration,
        CompilationModel compilation,
        SyntaxKind attributeTargetKind = SyntaxKind.None )
    {
        var attributes = compilation.GetAttributeCollection( declaration );

        if ( attributes.Count == 0 )
        {
            return default;
        }
        else
        {
            var list = new List<AttributeListSyntax>();

            foreach ( var attribute in attributes )
            {
                if ( attribute.GetTarget( compilation ).Constructor.DeclaringType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute" )
                {
                    continue;
                }

                var attributeList = AttributeList( SingletonSeparatedList( this.Attribute( attribute.GetTarget( compilation ) ) ) );

                if ( attributeTargetKind != SyntaxKind.None )
                {
                    attributeList = attributeList.WithTarget( AttributeTargetSpecifier( Token( attributeTargetKind ) ) );
                }

                list.Add( attributeList );
            }

            return List( list );
        }
    }

    public SyntaxNode AddAttribute( SyntaxNode oldNode, IAttributeData attribute )
    {
        var attributeList = AttributeList( SingletonSeparatedList( this.Attribute( attribute ) ) )
            .WithLeadingTriviaIfNecessary( oldNode.GetLeadingTrivia(), this._context.Options )
            .WithTrailingLineFeedIfNecessary( this._context );

        oldNode = oldNode.WithLeadingTriviaIfNecessary( default(SyntaxTriviaList), this._context.Options );

        if ( attributeList.GetLeadingTrivia().LastOrDefault() is { RawKind: (int) SyntaxKind.WhitespaceTrivia } indentationTrivia )
        {
            oldNode = oldNode.WithLeadingTriviaIfNecessary( indentationTrivia, this._context.Options );
        }

        return oldNode.Kind() switch
        {
            SyntaxKind.MethodDeclaration => ((MethodDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.DestructorDeclaration => ((DestructorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.ConstructorDeclaration => ((ConstructorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.InterfaceDeclaration => ((InterfaceDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.DelegateDeclaration => ((DelegateDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.EnumDeclaration => ((EnumDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.ClassDeclaration => ((ClassDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.StructDeclaration => ((StructDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.Parameter => ((ParameterSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.PropertyDeclaration => ((PropertyDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.EventDeclaration => ((EventDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.AddAccessorDeclaration => ((AccessorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.RemoveAccessorDeclaration => ((AccessorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.GetAccessorDeclaration => ((AccessorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.SetAccessorDeclaration => ((AccessorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.OperatorDeclaration => ((OperatorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.ConversionOperatorDeclaration => ((ConversionOperatorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.IndexerDeclaration => ((IndexerDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.FieldDeclaration => ((FieldDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            SyntaxKind.EventFieldDeclaration => ((EventFieldDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
            _ => throw new AssertionFailedException( $"Unexpected syntax kind {oldNode.Kind()} at '{oldNode.GetLocation()}'." )
        };
    }

    private ExpressionSyntax AttributeValueExpression( TypedConstant typedConstant )
    {
        if ( typedConstant.IsNullOrDefault )
        {
            return this.DefaultExpression( typedConstant.Type.GetSymbol() );
        }

        ExpressionSyntax GetValue( object? value, IType type )
        {
            if ( value is TypedConstant innerTypedConstant )
            {
                value = innerTypedConstant.Value;
            }

            if ( value == null )
            {
                return Null;
            }

            switch ( type )
            {
                case INamedType { TypeKind: TypeKind.Enum }:
                    return this.EnumValueExpression( (INamedTypeSymbol) type.GetSymbol(), value );

                case IArrayType arrayType:
                    return this.ArrayCreationExpression(
                        this.Type( arrayType.ElementType.GetSymbol() ),
                        ((ImmutableArray<TypedConstant>) value).Select( x => GetValue( x.Value, x.Type ) ) );

                default:
                    switch ( value )
                    {
                        case IType typeValue:
                            return this.TypeOfExpression( typeValue.GetSymbol() );

                        case Type systemTypeValue:
                            return this.TypeOfExpression( this._context.ReflectionMapper.GetTypeSymbol( systemTypeValue ) );

                        default:
                            {
                                var literal = LiteralExpressionOrNull( value );

                                if ( literal != null )
                                {
                                    return literal;
                                }
                                else
                                {
                                    throw new ArgumentOutOfRangeException(
                                        nameof(value),
                                        $"The value '{value}' cannot be converted to a custom attribute argument value." );
                                }
                            }
                    }
            }
        }

        return GetValue( typedConstant.Value, typedConstant.Type );
    }

    public TypeParameterListSyntax? TypeParameterList( IMethod method, CompilationModel compilation )
    {
        if ( method.TypeParameters.Count == 0 )
        {
            return null;
        }
        else
        {
            var list = SyntaxFactory.TypeParameterList(
                SeparatedList( method.TypeParameters.SelectAsImmutableArray( p => this.TypeParameter( p, compilation ) ) ) );

            return list;
        }
    }

    private TypeParameterSyntax TypeParameter( ITypeParameter typeParameter, CompilationModel compilation )
    {
        var syntax = SyntaxFactory.TypeParameter( typeParameter.Name );

        switch ( typeParameter.Variance )
        {
            case VarianceKind.In:
                syntax = syntax.WithVarianceKeyword( Token( SyntaxKind.InKeyword ) );

                break;

            case VarianceKind.Out:
                syntax = syntax.WithVarianceKeyword( Token( SyntaxKind.OutKeyword ) );

                break;
        }

        syntax = syntax.WithAttributeLists( this.AttributesForDeclaration( typeParameter.ToTypedRef<IDeclaration>(), compilation ) );

        return syntax;
    }

    public ParameterListSyntax ParameterList( IMethodBase method, CompilationModel compilation, bool removeDefaultValues = false )
        => SyntaxFactory.ParameterList( this.ParameterListParameters( method, compilation, removeDefaultValues ) );

    public ParameterListSyntax ParameterList( IReadOnlyList<IParameter> parameters, CompilationModel compilation, bool removeDefaultValues = false )
        => SyntaxFactory.ParameterList( this.ParameterListParameters( parameters, compilation, removeDefaultValues ) );

    public BracketedParameterListSyntax ParameterList( IIndexer indexer, CompilationModel compilation, bool removeDefaultValues = false )
        => BracketedParameterList( this.ParameterListParameters( indexer, compilation, removeDefaultValues ) );

    private SeparatedSyntaxList<ParameterSyntax> ParameterListParameters( IHasParameters method, CompilationModel compilation, bool removeDefaultValues )
        => this.ParameterListParameters( method.Parameters, compilation, removeDefaultValues );

    private SeparatedSyntaxList<ParameterSyntax> ParameterListParameters(
        IReadOnlyList<IParameter> parameters,
        CompilationModel compilation,
        bool removeDefaultValues )
        => SeparatedList(
            parameters.SelectAsReadOnlyList(
                p => Parameter(
                    this.AttributesForDeclaration( p.ToTypedRef<IDeclaration>(), compilation ),
                    p.GetSyntaxModifierList(),
                    this.Type( p.Type.GetSymbol() ).WithTrailingTriviaIfNecessary( ElasticSpace, this.Options ),
                    Identifier( p.Name ),
                    removeDefaultValues || p.DefaultValue == null
                        ? null
                        : EqualsValueClause( this.LiteralExpression( p.DefaultValue.Value.Value ) ) ) ) );

    public SyntaxList<TypeParameterConstraintClauseSyntax> TypeParameterConstraintClauses( ImmutableArray<ITypeParameterSymbol> typeParameters )
    {
        // Spec: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters

        if ( typeParameters.IsDefaultOrEmpty )
        {
            return default;
        }

        var list = List<TypeParameterConstraintClauseSyntax>();

        foreach ( var parameter in typeParameters )
        {
            var constraints = SeparatedList<TypeParameterConstraintSyntax>();

            if ( parameter.HasNotNullConstraint )
            {
                constraints = constraints.Add( TypeConstraint( SyntaxFactory.IdentifierName( "notnull" ) ) );
            }
            else if ( parameter.HasReferenceTypeConstraint )
            {
                if ( parameter.ReferenceTypeConstraintNullableAnnotation != NullableAnnotation.Annotated )
                {
                    constraints = constraints.Add( ClassOrStructConstraint( SyntaxKind.ClassConstraint ) );
                }
                else
                {
                    constraints = constraints.Add(
                        ClassOrStructConstraint( SyntaxKind.ClassConstraint ).WithQuestionToken( Token( SyntaxKind.QuestionToken ) ) );
                }
            }
            else if ( parameter.HasValueTypeConstraint )
            {
                if ( parameter.HasUnmanagedTypeConstraint )
                {
                    constraints = constraints.Add(
                        TypeConstraint(
                            SyntaxFactory.IdentifierName(
                                Identifier(
                                    default,
                                    SyntaxKind.UnmanagedKeyword,
                                    "unmanaged",
                                    "unmanaged",
                                    default ) ) ) );
                }
                else
                {
                    constraints = constraints.Add( ClassOrStructConstraint( SyntaxKind.StructConstraint ) );
                }
            }

            foreach ( var typeConstraint in parameter.ConstraintTypes )
            {
                constraints = constraints.Add( TypeConstraint( this.Type( typeConstraint ) ) );
            }

            if ( parameter.HasConstructorConstraint )
            {
                constraints = constraints.Add( ConstructorConstraint() );
            }

            if ( constraints.Count > 0 )
            {
                var clause = TypeParameterConstraintClause( parameter.Name ).WithConstraints( constraints ).NormalizeWhitespaceIfNecessary( this._context );
                list = list.Add( clause );
            }
        }

        return list;
    }

    // TODO: Move to ContextualSyntaxGenerator use conditional simplifier annotation.
    public static ExpressionSyntax LiteralExpression( string? s )
        => s == null
            ? ParenthesizedExpression(
                    SyntaxFactory.CastExpression(
                        NullableType( PredefinedType( Token( SyntaxKind.StringKeyword ) ) ),
                        SyntaxFactory.LiteralExpression( SyntaxKind.NullLiteralExpression ) ) )
                .WithAdditionalAnnotations( Simplifier.Annotation )
            : LiteralNonNullExpression( s );

    public CastExpressionSyntax SafeCastExpression( TypeSyntax type, ExpressionSyntax syntax )
    {
        if ( syntax is CastExpressionSyntax cast && cast.Type.IsEquivalentTo( type, topLevel: false ) )
        {
            // It's already a cast to the same type, no need to cast again.
            return cast;
        }

        var requiresParenthesis = syntax switch
        {
            CastExpressionSyntax => false,
            InvocationExpressionSyntax => false,
            MemberAccessExpressionSyntax => false,
            ElementAccessExpressionSyntax => false,
            IdentifierNameSyntax => false,
            LiteralExpressionSyntax => false,
            DefaultExpressionSyntax => false,
            TypeOfExpressionSyntax => false,
            ParenthesizedExpressionSyntax => false,
            ConditionalAccessExpressionSyntax => false,
            ObjectCreationExpressionSyntax => false,
            ArrayCreationExpressionSyntax => false,
            PostfixUnaryExpressionSyntax => false,

            // The syntax (T)-x is ambiguous and interpreted as binary minus, not cast of unary minus.
            PrefixUnaryExpressionSyntax { RawKind: not (int) SyntaxKind.UnaryMinusExpression } => false,
            TupleExpressionSyntax => false,
            ThisExpressionSyntax => false,
            _ => true
        };

        if ( requiresParenthesis )
        {
            return SyntaxFactory.CastExpression( type, ParenthesizedExpression( syntax ).WithAdditionalAnnotations( Simplifier.Annotation ) )
                .WithSimplifierAnnotationIfNecessary( this._context );
        }
        else
        {
            return SyntaxFactory.CastExpression( type, syntax ).WithAdditionalAnnotations( Simplifier.Annotation );
        }
    }

    public BlockSyntax FormattedBlock() => this.MemoizedFormattedBlock;

    [Memo]
    private BlockSyntax MemoizedFormattedBlock => this.FormattedBlock( Array.Empty<StatementSyntax>() );

    public BlockSyntax FormattedBlock( params StatementSyntax[] statements ) => this.FormattedBlock( (IEnumerable<StatementSyntax>) statements );

    private static bool NeedsLineFeed( StatementSyntax statement )
        => !statement.HasTrailingTrivia || !statement.GetTrailingTrivia()[^1].IsKind( SyntaxKind.EndOfLineTrivia );

    public BlockSyntax FormattedBlock( IEnumerable<StatementSyntax> statements )
        => Block(
            Token( default, SyntaxKind.OpenBraceToken, this._context.ElasticEndOfLineTriviaList ),
            List(
                statements.Select(
                    s => NeedsLineFeed( s )
                        ? s.WithTrailingLineFeedIfNecessary( this._context )
                        : s ) ),
            Token( this._context.ElasticEndOfLineTriviaList, SyntaxKind.CloseBraceToken, default ) );

    public PragmaWarningDirectiveTriviaSyntax PragmaWarningDirectiveTrivia(
        SyntaxKind disableOrRestoreKind,
        SeparatedSyntaxList<ExpressionSyntax> errorCodes )
        => SyntaxFactory.PragmaWarningDirectiveTrivia(
            Token( this._context.ElasticEndOfLineTriviaList, SyntaxKind.HashToken, default ),
            TokenWithTrailingSpace( SyntaxKind.PragmaKeyword ),
            TokenWithTrailingSpace( SyntaxKind.WarningKeyword ),
            TokenWithTrailingSpace( disableOrRestoreKind ),
            errorCodes,
            Token( default, SyntaxKind.EndOfDirectiveToken, this._context.ElasticEndOfLineTriviaList ),
            isActive: true );
}