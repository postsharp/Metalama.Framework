// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    internal class EventFieldSubstitution : SyntaxNodeSubstitution
    {
        private readonly VariableDeclaratorSyntax _rootNode;
        private readonly IMethodSymbol _targetAccessor;

        public EventFieldSubstitution( VariableDeclaratorSyntax rootNode, IMethodSymbol targetAccessor )
        {
            this._rootNode = rootNode;
            this._targetAccessor = targetAccessor;
        }

        public override SyntaxNode TargetNode => this._rootNode;

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            switch ( currentNode )
            {
                case VariableDeclaratorSyntax:
                    if ( this._targetAccessor.MethodKind == MethodKind.EventAdd )
                    {
                        return
                            Block(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.AddAssignmentExpression,
                                        CreateFieldAccessExpression(),
                                        IdentifierName( "value" ) ) ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                    }
                    else if ( this._targetAccessor.MethodKind == MethodKind.EventRemove )
                    {
                        return
                            Block(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SubtractAssignmentExpression,
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
                var targetEvent = (IEventSymbol) this._targetAccessor.AssociatedSymbol.AssertNotNull();

                if ( targetEvent.IsStatic )
                {
                    return
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type( targetEvent.ContainingType ),
                            IdentifierName( LinkerRewritingDriver.GetBackingFieldName( targetEvent ) ) );
                }
                else
                {
                    return
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName( LinkerRewritingDriver.GetBackingFieldName( targetEvent ) ) );
                }
            }
        }
    }
}