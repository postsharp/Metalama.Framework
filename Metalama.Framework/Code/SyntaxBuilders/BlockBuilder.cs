// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Code.SyntaxBuilders;

[CompileTime]
public sealed class BlockBuilder
{
    private readonly List<IStatement> _statements = new();

    public void Add( IStatement statement )
    {
        this._statements.Add( statement );
    }

    public IStatement ToStatement() => SyntaxBuilder.CurrentImplementation.CreateBlock( this._statements.ToImmutableArray() );
}