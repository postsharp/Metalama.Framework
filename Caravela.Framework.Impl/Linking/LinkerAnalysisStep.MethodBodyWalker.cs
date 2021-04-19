// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerAnalysisStep
    {
        // TODO: Change this to counting return statements that change the control flow.

        /// <summary>
        /// Walks method bodies, counting return statements.
        /// </summary>
        private class MethodBodyWalker : CSharpSyntaxWalker
        {
            public int ReturnStatementCount { get; private set; }

            public override void VisitReturnStatement( ReturnStatementSyntax node )
            {
                this.ReturnStatementCount++;
                base.VisitReturnStatement( node );
            }
        }
    }
}