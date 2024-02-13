// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using TypedConstant = Metalama.Framework.Code.TypedConstant;
using TypeKind = Metalama.Framework.Code.TypeKind;
using VarianceKind = Metalama.Framework.Code.VarianceKind;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class SyntaxGeneratorWithContext : OurSyntaxGenerator
{
    private readonly ConcurrentDictionary<ITypeSymbol, TypeSyntax> _typeSyntaxCache;
    private readonly SyntaxGenerationContext _context;

    public SyntaxGeneratorWithContext( OurSyntaxGenerator prototype, SyntaxGenerationContext context ) : base( prototype )
    {
        this._context = context;
        this._typeSyntaxCache = new ConcurrentDictionary<ITypeSymbol, TypeSyntax>( context.CompilationContext.SymbolComparerIncludingNullability );
    }

    public override TypeSyntax Type( ITypeSymbol symbol )
    {
        if ( symbol.BelongsToCompilation( this._context.CompilationContext ) == true )
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
        return base.Type( symbol );
    }

    protected override bool NormalizeWhitespace => this._context.NormalizeWhitespace;

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
            .WithLeadingTriviaIfNecessary( oldNode.GetLeadingTrivia(), this._context.PreserveTrivia )
            .WithTrailingTriviaIfNecessary( ElasticCarriageReturnLineFeed, this._context.NormalizeWhitespace );

        oldNode = oldNode.WithLeadingTriviaIfNecessary( default(SyntaxTriviaList), this._context.PreserveTrivia );

        if ( attributeList.GetLeadingTrivia().LastOrDefault() is { RawKind: (int) SyntaxKind.WhitespaceTrivia } indentationTrivia )
        {
            oldNode = oldNode.WithLeadingTriviaIfNecessary( indentationTrivia, this._context.PreserveTrivia );
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
                return SyntaxFactoryEx.Null;
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
                                var literal = SyntaxFactoryEx.LiteralExpressionOrNull( value );

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
        => this.ParameterListParameters(method.Parameters, compilation, removeDefaultValues );

    private SeparatedSyntaxList<ParameterSyntax> ParameterListParameters( IReadOnlyList<IParameter> parameters, CompilationModel compilation, bool removeDefaultValues )
        => SeparatedList(
            parameters.SelectAsReadOnlyList(
                p => Parameter(
                    this.AttributesForDeclaration( p.ToTypedRef<IDeclaration>(), compilation ),
                    p.GetSyntaxModifierList(),
                    this.Type( p.Type.GetSymbol() ).WithTrailingTriviaIfNecessary( ElasticSpace, this.NormalizeWhitespace ),
                    Identifier( p.Name ),
                    removeDefaultValues || p.DefaultValue == null
                        ? null
                        : EqualsValueClause( SyntaxFactoryEx.LiteralExpression( p.DefaultValue.Value.Value ) ) ) ) );

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
                var clause = TypeParameterConstraintClause( parameter.Name ).WithConstraints( constraints ).NormalizeWhitespaceIfNecessary( this.NormalizeWhitespace );
                list = list.Add( clause );
            }
        }

        return list;
    }
}