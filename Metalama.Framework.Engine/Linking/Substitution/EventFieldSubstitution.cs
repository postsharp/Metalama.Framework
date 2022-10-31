// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Templating;
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
            switch ( currentNode, this._targetAccessor.MethodKind )
            {
                case (VariableDeclaratorSyntax, MethodKind.EventAdd):
                    return
                        SyntaxFactoryEx.FormattedBlock(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.AddAssignmentExpression,
                                        CreateFieldAccessExpression(),
                                        IdentifierName( "value" ) ) ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                case (VariableDeclaratorSyntax, MethodKind.EventRemove ):
                    return
                        SyntaxFactoryEx.FormattedBlock(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SubtractAssignmentExpression,
                                        CreateFieldAccessExpression(),
                                        IdentifierName( "value" ) ) ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                default:
                    throw new AssertionFailedException($"({currentNode.Kind()}, {this._targetAccessor.MethodKind}) are not supported.");
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