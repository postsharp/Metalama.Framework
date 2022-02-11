// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerLinkingStep
    {
        // TODO: This is temporary for unneeded label removal until the linker uses control flow analysis resuls for inlining.
        private class CountLabelUsesWalker : CSharpSyntaxWalker
        {            
            public Dictionary<string, int> ObservedLabelCounters { get; } = new Dictionary<string, int>();

            public override void VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
            {
                // Don't visit local functions (may have same labels).
            }

            public override void VisitGotoStatement( GotoStatementSyntax node )
            {
                if ( node.Expression is IdentifierNameSyntax identifierName )
                {
                    this.ObservedLabelCounters.TryGetValue( identifierName.Identifier.ValueText, out var counter );
                    this.ObservedLabelCounters[identifierName.Identifier.ValueText] = counter + 1;
                }
            }
        }
    }
}