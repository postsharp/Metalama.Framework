// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerRewritingDriver
    {
        internal class ReturnStatementWalker : CSharpSyntaxWalker
        {
            private readonly List<ReturnStatementSyntax> _returnStatements;

            public IReadOnlyList<ReturnStatementSyntax> ReturnStatements => this._returnStatements;

            public ReturnStatementWalker()
            {
                this._returnStatements = new List<ReturnStatementSyntax>();
            }

            public override void VisitReturnStatement( ReturnStatementSyntax node )
            {
                this._returnStatements.Add( node );
            }
        }
    }    
}
