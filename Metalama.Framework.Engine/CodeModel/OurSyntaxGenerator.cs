// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;
using TypedConstant = Metalama.Framework.Code.TypedConstant;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel;

internal partial class OurSyntaxGenerator
{
    public static OurSyntaxGenerator NullOblivious { get; }

    public static OurSyntaxGenerator Default { get; }

    public static OurSyntaxGenerator CompileTime => Default;

    public static OurSyntaxGenerator GetInstance( bool nullableContext ) => nullableContext ? Default : NullOblivious;

    static OurSyntaxGenerator()
    {
        var type = WorkspaceHelper.CSharpWorkspacesAssembly.GetType( $"Microsoft.CodeAnalysis.CSharp.CodeGeneration.CSharpSyntaxGenerator" )!;
        var field = type.GetField( "Instance", BindingFlags.Public | BindingFlags.Static )!;
        var syntaxGenerator = (SyntaxGenerator) field.GetValue( null ).AssertNotNull();
        Default = new OurSyntaxGenerator( syntaxGenerator, true );
        NullOblivious = new OurSyntaxGenerator( syntaxGenerator, false );
    }

    private readonly SyntaxGenerator _syntaxGenerator;

    public bool IsNullAware { get; }

    private OurSyntaxGenerator( SyntaxGenerator syntaxGenerator, bool nullAware )
    {
        this._syntaxGenerator = syntaxGenerator;
        this.IsNullAware = nullAware;
    }

    protected OurSyntaxGenerator( OurSyntaxGenerator prototype ) : this( prototype._syntaxGenerator, prototype.IsNullAware ) { }

    public TypeOfExpressionSyntax TypeOfExpression( ITypeSymbol type, IReadOnlyDictionary<string, TypeSyntax>? substitutions = null, bool keepNullableAnnotations = false )
    {
        var typeSyntax = this.Type( type );

        if ( type is INamedTypeSymbol { IsGenericType: true } namedType )
        {
            if ( namedType.IsGenericTypeDefinition() )
            {
                // In generic definitions, we must remove type arguments.
                typeSyntax = (TypeSyntax) new RemoveTypeArgumentsRewriter().Visit( typeSyntax );
            }
        }

        if ( !keepNullableAnnotations )
        {
            // In regular typeof, we must remove ? annotations of nullable types.
            typeSyntax = (TypeSyntax) new RemoveReferenceNullableAnnotationsRewriter( type ).Visit( typeSyntax )!;
        }

        var dynamicToVarRewriter = new DynamicToVarRewriter();
        // In any typeof, we must change dynamic to object.
        typeSyntax = (TypeSyntax) dynamicToVarRewriter.Visit( typeSyntax );

        SafeSyntaxRewriter rewriter = type switch
        {
            INamedTypeSymbol { IsGenericType: true } genericType when genericType.IsGenericTypeDefinition() => new RemoveTypeArgumentsRewriter(),
            INamedTypeSymbol { IsGenericType: true } => new RemoveReferenceNullableAnnotationsRewriter( type ),
            _ => dynamicToVarRewriter
        };

        var rewrittenTypeSyntax = rewriter.Visit( typeSyntax );

        // Substitute type arguments.
        if ( substitutions is { Count: > 0 } )
        {
            var substitutionRewriter = new SubstitutionRewriter( substitutions );
            rewrittenTypeSyntax = substitutionRewriter.Visit( rewrittenTypeSyntax ).AssertNotNull();
        }

        return (TypeOfExpressionSyntax) this._syntaxGenerator.TypeOfExpression( rewrittenTypeSyntax );
    }

    public TypeSyntax Type( ITypeSymbol symbol )
    {
        var typeSyntax = (TypeSyntax) this._syntaxGenerator.TypeExpression( symbol ).WithAdditionalAnnotations( Simplifier.Annotation );

        if ( !this.IsNullAware )
        {
            typeSyntax = (TypeSyntax) new RemoveReferenceNullableAnnotationsRewriter( symbol ).Visit( typeSyntax );
        }

        return (TypeSyntax) new NormalizeSpaceRewriter().Visit( typeSyntax );
    }

    public DefaultExpressionSyntax DefaultExpression( ITypeSymbol typeSymbol )
        => SyntaxFactory.DefaultExpression( this.Type( typeSymbol ) )
            .WithAdditionalAnnotations( Simplifier.Annotation );

    public ArrayCreationExpressionSyntax ArrayCreationExpression( TypeSyntax type, IEnumerable<SyntaxNode> elements )
    {
        var array = (ArrayCreationExpressionSyntax) this._syntaxGenerator.ArrayCreationExpression( type, elements );

        return array.WithType( array.Type.WithAdditionalAnnotations( Simplifier.Annotation ) ).NormalizeWhitespace();
    }

    public TypeSyntax Type( SpecialType specialType )
        => (TypeSyntax) this._syntaxGenerator.TypeExpression( specialType )
            .WithAdditionalAnnotations( Simplifier.Annotation );

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

        return SyntaxFactoryEx.SafeCastExpression( this.Type( targetTypeSymbol ), expression );
    }

    public ExpressionSyntax TypeOrNamespace( INamespaceOrTypeSymbol symbol )
    {
        ExpressionSyntax expression;

        switch ( symbol )
        {
            case ITypeSymbol typeSymbol:
                return this.Type( typeSymbol );

            case INamespaceSymbol namespaceSymbol:
                expression = (ExpressionSyntax) this._syntaxGenerator.NameExpression( namespaceSymbol );

                break;

            default:
                throw new AssertionFailedException( $"Unexpected symbol kind: {symbol.Kind}." );
        }

        return expression.WithAdditionalAnnotations( Simplifier.Annotation );
    }

    public ThisExpressionSyntax ThisExpression() => (ThisExpressionSyntax) this._syntaxGenerator.ThisExpression();

    public LiteralExpressionSyntax LiteralExpression( object literal ) => (LiteralExpressionSyntax) this._syntaxGenerator.LiteralExpression( literal );

    public IdentifierNameSyntax IdentifierName( string identifier ) => (IdentifierNameSyntax) this._syntaxGenerator.IdentifierName( identifier );

    public TypeSyntax ArrayTypeExpression( TypeSyntax type )
    {
        var arrayType = (ArrayTypeSyntax) this._syntaxGenerator.ArrayTypeExpression( type ).WithAdditionalAnnotations( Simplifier.Annotation );

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
                        Token( SyntaxKind.WhereKeyword ).WithTrailingTrivia( Space ),
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
}