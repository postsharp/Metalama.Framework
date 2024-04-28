// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.SyntaxGeneration;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Templating.Statements;

internal class BlockStatement : IStatementImpl
{
    private readonly IReadOnlyList<IStatement> _statements;

    public BlockStatement( IReadOnlyList<IStatement> statements )
    {
        this._statements = statements;
    }

    public StatementSyntax GetSyntax( TemplateSyntaxFactoryImpl? templateSyntaxFactory )
        => SyntaxFactory.Block( this._statements.SelectAsReadOnlyCollection( x => ((IStatementImpl) x).GetSyntax( templateSyntaxFactory ) ) );
}