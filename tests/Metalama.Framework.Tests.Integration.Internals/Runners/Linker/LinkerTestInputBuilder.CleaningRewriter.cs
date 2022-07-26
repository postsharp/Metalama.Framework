// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Tests.Integration.Runners.Linker
{
    internal partial class LinkerTestInputBuilder
    {
        private class CleaningRewriter : SafeSyntaxRewriter
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