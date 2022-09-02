// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerRewritingDriver
    {
        internal class ReturnStatementWalker : SafeSyntaxWalker
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

            public override void VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
            {
                // Never visit local functions.
            }

            public override void VisitSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
            {
                // Never visit lambdas.
            }

            public override void VisitParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node )
            {
                // Never visit lambdas.
            }

            public override void VisitAnonymousMethodExpression( AnonymousMethodExpressionSyntax node )
            {
                // Never visit anonymous methods.
            }
        }
    }
}