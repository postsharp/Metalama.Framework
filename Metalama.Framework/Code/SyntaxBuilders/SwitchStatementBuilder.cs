// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Code.SyntaxBuilders;

/// <summary>
/// Builds a <c>switch</c> statement.
/// </summary>
[CompileTime]
public sealed class SwitchStatementBuilder : IStatementBuilder
{
    private readonly IExpression _expression;
    private readonly List<SwitchStatementSection> _cases = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchStatementBuilder"/> class.
    /// </summary>
    /// <param name="expression">The expression in the <c>switch ( expression )</c> statement.</param>
    public SwitchStatementBuilder( IExpression expression )
    {
        this._expression = expression;
    }

    /// <summary>
    /// Add a <c>case</c> switch section.
    /// </summary>
    /// <param name="label">The label, i.e. the value to match.</param>
    /// <param name="statements">The statements to execute.</param>
    /// <param name="appendBreak">Value indicating whether a <c>break;</c> statement should be appended to <paramref name="statements"/>. The value is <c>true</c>.</param>
    public void AddCase( SwitchStatementLabel label, IStatementList statements, bool appendBreak = true )
    {
        this._cases.Add( new SwitchStatementSection( label, null, statements, appendBreak ) );
    }

    public void AddCase( SwitchStatementLabel label, IExpression? when, IStatementList statements, bool appendBreak = true )
    {
        this._cases.Add( new SwitchStatementSection( label, when, statements, appendBreak ) );
    }

    /// <summary>
    /// Add a <c>default</c> switch section.
    /// </summary>
    /// <param name="statements">The statements to execute.</param>
    /// <param name="appendBreak">Value indicating whether a <c>break;</c> statement should be appended to <paramref name="statements"/>. The value is <c>true</c>.</param>
    public void AddDefault( IStatementList statements, bool appendBreak = true )
    {
        this._cases.Add( new SwitchStatementSection( null, null, statements, appendBreak ) );
    }

    /// <summary>
    /// Gets the number of sections in the <c>switch</c>.
    /// </summary>
    public int SectionCount => this._cases.Count;

    /// <summary>
    /// Builds an <see cref="IStatement"/> from the current object.
    /// </summary>
    public IStatement ToStatement()
    {
        if ( this._cases.Count == 0 )
        {
            throw new InvalidOperationException( "The switch does not have any sections." );
        }

        return SyntaxBuilder.CurrentImplementation.CreateSwitchStatement( this._expression, this._cases.ToImmutableArray() );
    }
}