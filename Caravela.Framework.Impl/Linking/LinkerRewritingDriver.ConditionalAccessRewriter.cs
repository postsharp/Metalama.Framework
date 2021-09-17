// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// TODO: A lot methods here are called multiple times. Optimize.
// TODO: Split into a subclass for each declaration type?

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerRewritingDriver
    {
        private class ConditionalAccessRewriter : CSharpSyntaxRewriter
        {
            private readonly string _replacingIdentifier;
            private ConditionalAccessExpressionSyntax? _context;

            public ConditionalAccessRewriter(string replacingIdentifier)
            {
                this._replacingIdentifier = replacingIdentifier;
            }

            public override SyntaxNode? VisitConditionalAccessExpression( ConditionalAccessExpressionSyntax node )
            {
                if ( this._context == null )
                {
                    this._context = node;

                    var result = node.WithWhenNotNull((ExpressionSyntax)this.Visit( node.WhenNotNull ));

                    this._context = null;

                    return result;
                }
                else
                {
                    // Template compiler currently does not generate expressions that would chain x?.y?.z without parentheses.
                    throw new AssertionFailedException( Justifications.CoverageMissing );
                    
                    // return node;
                }
            }

            public override SyntaxNode? VisitMemberBindingExpression( MemberBindingExpressionSyntax node )
            {
                if ( this._context != null )
                {
                    return node.WithName( IdentifierName( this._replacingIdentifier ) );
                }
                else
                {
                    // Template compiler currently does not generate expressions that would chain x?.y?.z without parentheses.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                    // return node;
                }
            }
        }
    }
}