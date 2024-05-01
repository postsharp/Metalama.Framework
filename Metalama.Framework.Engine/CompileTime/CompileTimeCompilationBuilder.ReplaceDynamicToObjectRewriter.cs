// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.CompileTime
{
    internal sealed partial class CompileTimeCompilationBuilder
    {
        private sealed class ReplaceDynamicToObjectRewriter : SafeSyntaxRewriter
        {
            private static readonly RecyclableObjectPool<ReplaceDynamicToObjectRewriter> _pool = new( () => new ReplaceDynamicToObjectRewriter() );

            private ReplaceDynamicToObjectRewriter() { }

            public static T Rewrite<T>( T node )
                where T : SyntaxNode
            {
                using var rewriter = _pool.Allocate();

                return (T) rewriter.Value.Visit( node ).AssertNotNull();
            }

            public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
            {
                if ( node.Identifier.Text == "dynamic" )
                {
                    return SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.ObjectKeyword ) ).WithTriviaFrom( node );
                }
                else
                {
                    return base.VisitIdentifierName( node );
                }
            }
        }
    }
}