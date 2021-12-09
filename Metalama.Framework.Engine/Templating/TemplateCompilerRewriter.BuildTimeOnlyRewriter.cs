// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        private class BuildTimeOnlyRewriter : CSharpSyntaxRewriter
        {
            private readonly TemplateCompilerRewriter _rewriter;

            public BuildTimeOnlyRewriter( TemplateCompilerRewriter rewriter )
            {
                this._rewriter = rewriter;
            }

            public bool TryRewriteProceedInvocation( InvocationExpressionSyntax node, out InvocationExpressionSyntax transformedNode )
            {
                var kind = this._rewriter._templateMemberClassifier.GetMetaMemberKind( node.Expression );

                if ( kind.IsAnyProceed() )
                {
                    var methodName = node.Expression switch
                    {
                        MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                        IdentifierNameSyntax identifier => identifier.Identifier.Text,
                        _ => throw new AssertionFailedException( $"Don't know how to get the member name in {node.Expression.GetType().Name}" )
                    };

                    transformedNode =
                        node.CopyAnnotationsTo(
                            InvocationExpression(
                                    this._rewriter._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.Proceed) ) )
                                .WithArgumentList( ArgumentList( SeparatedList( new[] { Argument( SyntaxFactoryEx.LiteralExpression( methodName ) ) } ) ) ) )!;

                    return true;
                }
                else
                {
                    transformedNode = node;

                    return false;
                }
            }

            public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
            {
                this.TryRewriteProceedInvocation( node, out var transformedNode );

                return transformedNode;
            }
        }
    }
}