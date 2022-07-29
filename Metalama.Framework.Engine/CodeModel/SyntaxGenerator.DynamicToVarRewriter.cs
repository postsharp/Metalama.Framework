// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.CodeModel
{
    internal partial class OurSyntaxGenerator
    {
        private class DynamicToVarRewriter : SafeSyntaxRewriter
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