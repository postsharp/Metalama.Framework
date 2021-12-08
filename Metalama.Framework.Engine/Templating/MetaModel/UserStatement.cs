// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.SyntaxBuilders;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.MetaModel
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