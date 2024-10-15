// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Tests.LinkerTests.Runner
{
    internal partial class LinkerTestInputBuilder
    {
        private sealed class CleaningRewriter : SafeSyntaxRewriter
        {
            protected override SyntaxNode? VisitCore( SyntaxNode? node )
            {
                if ( node != null && IsTemporary( node ) )
                {
                    return null;
                }

                return base.VisitCore( node );
            }
        }
    }
}