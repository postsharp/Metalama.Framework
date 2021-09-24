// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class SyntaxGenerator
    {
        private sealed class RemoveTypeArgumentsRewriter : CSharpSyntaxRewriter
        {
            public static readonly CSharpSyntaxRewriter Instance = new RemoveTypeArgumentsRewriter();

            private RemoveTypeArgumentsRewriter() { }

            public override SyntaxNode? VisitGenericName( GenericNameSyntax node )
            {
                // We intentionally don't visit type arguments, because we don't want remove the nested type arguments.

                // Remove the list of type arguments.
                if ( node.TypeArgumentList.Arguments.Count == 1 )
                {
                    return SyntaxFactory.GenericName( node.Identifier );
                }
                else
                {
                    return SyntaxFactory.GenericName( node.Identifier )
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                    node.TypeArgumentList.Arguments.Select( _ => SyntaxFactory.OmittedTypeArgument() ) ) ) );
                }
            }
        }
    }
}