// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class SyntaxGenerator
    {
        private class DynamicToVarRewriter : CSharpSyntaxRewriter
        {
            public static readonly CSharpSyntaxRewriter Instance = new DynamicToVarRewriter();

            private DynamicToVarRewriter() { }

            public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
            {
                if ( node.Identifier.Text == "dynamic" )
                {
                    return SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.ObjectKeyword ) );
                }
                else
                {
                    return node;
                }
            }
        }
    }
}