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
    internal class EventRemoveRedirectionSubstitution : SyntaxNodeSubstitution
    {
        private readonly SyntaxNode _rootNode;
        private readonly IEventSymbol _targetEvent;

        public EventRemoveRedirectionSubstitution( SyntaxNode rootNode, IEventSymbol targetEvent )
        {
            this._rootNode = rootNode;
            this._targetEvent = targetEvent;
        }

        public override SyntaxNode TargetNode => this._rootNode;

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            return
                Block(
                    ExpressionStatement( AssignmentExpression( SyntaxKind.SubtractAssignmentExpression, CreateAccessExpression(), IdentifierName( "value" ) ) ) )
                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );

            InvocationExpressionSyntax CreateAccessExpression()
            {
                return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        this._targetEvent.IsStatic
                        ? substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type( this._targetEvent.ContainingType )
                        : ThisExpression(),
                        IdentifierName( this._targetEvent.Name ) ) );
            }
        }
    }
}