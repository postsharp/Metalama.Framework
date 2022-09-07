// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    internal class AutoPropertyAccessorSubstitution : SyntaxNodeSubstitution
    {
        private readonly AccessorDeclarationSyntax _rootNode;
        private readonly IPropertySymbol _targetProperty;
        private readonly string? _returnVariableIdentifier;

        public AutoPropertyAccessorSubstitution( AccessorDeclarationSyntax rootNode, IPropertySymbol targetProperty, string? returnVariableIdentifier )
        {
            this._rootNode = rootNode;
            this._targetProperty = targetProperty;
            this._returnVariableIdentifier = returnVariableIdentifier;
        }

        public override SyntaxNode TargetNode => this._rootNode;

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            switch ( currentNode )
            {
                case AccessorDeclarationSyntax accessorDeclarationSyntax:
                    if (accessorDeclarationSyntax.IsKind( SyntaxKind.GetAccessorDeclaration))
                    {
                        if ( this._returnVariableIdentifier != null )
                        {
                            return
                                Block(
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName( this._returnVariableIdentifier ),
                                            CreateFieldAccessExpression() ) ) )
                                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                        else
                        {
                            return
                                Block(
                                    ReturnStatement(
                                        Token( TriviaList(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                                        CreateFieldAccessExpression(),
                                        Token( TriviaList(), SyntaxKind.SemicolonToken, TriviaList( ElasticLineFeed ) ) ) )
                                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                    }
                    else if (accessorDeclarationSyntax.IsKind(SyntaxKind.SetAccessorDeclaration) || accessorDeclarationSyntax.IsKind(SyntaxKind.InitAccessorDeclaration))
                    {
                        return
                            Block(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        CreateFieldAccessExpression(),
                                        IdentifierName( "value" ) ) ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                    }
                    else
                    {
                        throw new AssertionFailedException();
                    }

                default:
                    throw new AssertionFailedException();
            }

            ExpressionSyntax CreateFieldAccessExpression()
            {
                if (this._targetProperty.IsStatic)
                {
                    return
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type(this._targetProperty.ContainingType),
                            IdentifierName( LinkerRewritingDriver.GetBackingFieldName( this._targetProperty ) ) );
                }
                else
                {
                    return
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName( LinkerRewritingDriver.GetBackingFieldName( this._targetProperty ) ) );
                }                
            }
        }
    }
}