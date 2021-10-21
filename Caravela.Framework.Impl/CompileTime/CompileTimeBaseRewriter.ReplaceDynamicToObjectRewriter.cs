// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CompileTime
{
    internal abstract partial class CompileTimeBaseRewriter
    {
        protected class ReplaceDynamicToObjectRewriter : CSharpSyntaxRewriter
        {
            private static readonly ReplaceDynamicToObjectRewriter _instance = new();

            private ReplaceDynamicToObjectRewriter() { }

            public static T Rewrite<T>( T node )
                where T : SyntaxNode
                => (T) _instance.Visit( node );

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