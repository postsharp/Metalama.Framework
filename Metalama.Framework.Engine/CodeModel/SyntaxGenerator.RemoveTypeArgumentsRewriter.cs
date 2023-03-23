// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    internal partial class OurSyntaxGenerator
    {
        private sealed class RemoveTypeArgumentsRewriter : SafeSyntaxRewriter
        {
            public override SyntaxNode VisitGenericName( GenericNameSyntax node )
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
                                    node.TypeArgumentList.Arguments.SelectAsImmutableArray( _ => SyntaxFactory.OmittedTypeArgument() ) ) ) );
                }
            }
        }
    }
}