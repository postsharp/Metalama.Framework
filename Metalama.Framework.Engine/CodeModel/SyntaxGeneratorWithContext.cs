// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

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
        var constructorArguments = attribute.ConstructorArguments.Select( a => SyntaxFactory.AttributeArgument( this.AttributeValueExpression( a.Value ) ) );

        var namedArguments = attribute.NamedArguments.Select(
            a => SyntaxFactory.AttributeArgument(
                SyntaxFactory.NameEquals( a.Key ),
                null,
                this.AttributeValueExpression( a.Value ) ) );

        var attributeSyntax = SyntaxFactory.Attribute( (NameSyntax) this.Type( attribute.Type.GetSymbol() ) );

        var argumentList = SyntaxFactory.AttributeArgumentList( SyntaxFactory.SeparatedList( constructorArguments.Concat( namedArguments ) ) );

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
                var attributeList = SyntaxFactory.AttributeList( SyntaxFactory.SingletonSeparatedList( this.Attribute( attribute.GetTarget( compilation ) ) ) );

                if ( attributeTargetKind != SyntaxKind.None )
                {
                    attributeList = attributeList.WithTarget( SyntaxFactory.AttributeTargetSpecifier( SyntaxFactory.Token( attributeTargetKind ) ) );
                }

                list.Add( attributeList );
            }

            return SyntaxFactory.List( list );
        }
    }

    public SyntaxNode AddAttribute( SyntaxNode oldNode, IAttributeData attribute )
    {
        var attributeList = SyntaxFactory.AttributeList( SyntaxFactory.SingletonSeparatedList( this.Attribute( attribute ) ) )
            .WithLeadingTrivia( oldNode.GetLeadingTrivia() )
            .WithTrailingTrivia( SyntaxFactory.ElasticLineFeed );

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

    private ExpressionSyntax AttributeValueExpression( object? value )
    {
        if ( value == null )
        {
            return SyntaxFactoryEx.Null;
        }

        if ( value is TypedConstant typedConstant )
        {
            return this.AttributeValueExpression( typedConstant.Value );
        }

        var literalExpression = SyntaxFactoryEx.LiteralExpressionOrNull( value );

        if ( literalExpression != null )
        {
            return literalExpression;
        }

        if ( value is Type type )
        {
            return this.TypeOfExpression( this._context.ReflectionMapper.GetTypeSymbol( type ) );
        }

        var valueType = value.GetType();

        if ( valueType.IsEnum )
        {
            return this.EnumValueExpression( (INamedTypeSymbol) this._context.ReflectionMapper.GetTypeSymbol( valueType ), value );
        }

        throw new ArgumentOutOfRangeException( nameof(value), $"The value '{value}' cannot be converted to a custom attribute argument value." );
    }
}