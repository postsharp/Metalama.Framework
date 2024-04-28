// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Code.SyntaxBuilders;

[CompileTime]
public sealed class SwitchStatementBuilder
{
    private readonly IExpression _expression;
    private readonly List<SwitchSection> _cases = new();

    public SwitchStatementBuilder( IExpression expression )
    {
        this._expression = expression;
    }

    public void AddCase( IExpression value, IStatement statement, bool appendBreak = true )
    {
        this._cases.Add( new SwitchSection( value, statement, appendBreak ) );
    }

    public void AddDefault( IStatement statement, bool appendBreak = true )
    {
        this._cases.Add( new SwitchSection( null, statement, appendBreak ) );
    }

    public IStatement ToStatement() => SyntaxBuilder.CurrentImplementation.CreateSwitchStatement( this._expression, this._cases.ToImmutableArray() );
}