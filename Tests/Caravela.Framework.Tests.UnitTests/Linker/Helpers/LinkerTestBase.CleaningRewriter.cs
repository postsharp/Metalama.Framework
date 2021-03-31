// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Tests.UnitTests.Linker.Helpers
{
    public partial class LinkerTestBase
    {
        private class CleaningRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode? Visit( SyntaxNode? node )
            {
                if ( node != null && IsTemporary( node ) )
                {
                    return null;
                }

                return base.Visit( node );
            }
        }
    }
}
