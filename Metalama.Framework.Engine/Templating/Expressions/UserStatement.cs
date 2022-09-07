// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.SyntaxBuilders;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    internal class UserStatement : IStatement
    {
        public StatementSyntax Syntax { get; }

        public UserStatement( StatementSyntax syntax )
        {
            this.Syntax = syntax;
        }
    }
}