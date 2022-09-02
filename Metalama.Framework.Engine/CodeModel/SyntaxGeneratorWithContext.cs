// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using TypedConstant = Metalama.Framework.Code.TypedConstant;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel;

internal class SyntaxGeneratorWithContext : OurSyntaxGenerator
{
    private readonly SyntaxGenerationContext _context;

    public SyntaxGeneratorWithContext( OurSyntaxGenerator prototype, SyntaxGenerationContext context ) : base( prototype )
    {
        this._context = context;
    }

    public AttributeSyntax Attribute( IAttributeData attribute )
    {
        var constructorArguments = attribute.ConstructorArguments.Select( a => AttributeArgument( this.AttributeValueExpression( a ) ) );

        var namedArguments = attribute.NamedArguments.Select(
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
            .WithLeadingTrivia( oldNode.GetLeadingTrivia() )
            .WithTrailingTrivia( ElasticLineFeed );

        SyntaxNode newNode = oldNode.Kind() switch
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
            _ => throw new AssertionFailedException()
        };

        return newNode;
    }

    private ExpressionSyntax AttributeValueExpression( TypedConstant typedConstant )
    {
        if ( typedConstant.IsNullOrDefault )
        {
            return this.DefaultExpression( typedConstant.Type.GetSymbol() );
        }

        ExpressionSyntax GetValue( object? value, IType type )
        {
            if ( value == null )
            {
                return SyntaxFactoryEx.Null;
            }

            if ( type is INamedType { TypeKind: TypeKind.Enum } )
            {
                return this.EnumValueExpression( (INamedTypeSymbol) type.GetSymbol(), value );
            }
            else
            {
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

        if ( typedConstant.Type is IArrayType arrayType )
        {
            return this.ArrayCreationExpression(
                this.Type( arrayType.ElementType.GetSymbol() ),
                typedConstant.Values.Select( x => GetValue( x.Value, arrayType.ElementType ) ) );
        }
        else
        {
            return GetValue( typedConstant.Value, typedConstant.Type );
        }
    }
}