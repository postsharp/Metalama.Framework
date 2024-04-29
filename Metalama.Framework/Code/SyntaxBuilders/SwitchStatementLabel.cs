// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Code.SyntaxBuilders;

/// <summary>
/// Represents the label of a <c>switch case</c> (i.e. the literal or tuple literal to which the expression is compared).
/// Only single literals or tuple of literals can be represented. 
/// </summary>
[CompileTime]
public sealed class SwitchStatementLabel
{
    /// <summary>
    /// Gets the list of literals in the tuple.
    /// </summary>
    public IReadOnlyList<TypedConstant> Values { get; }

    public SwitchStatementLabel( params object[] values )
    {
        if ( values.Length == 0 )
        {
            throw new ArgumentOutOfRangeException( nameof(values), "At least one value is required." );
        }

        this.Values = values.Select( TypedConstant.Create ).ToList();
    }

    public SwitchStatementLabel( params TypedConstant[] values )
    {
        if ( values.Length == 0 )
        {
            throw new ArgumentOutOfRangeException( nameof(values), "At least one value is required." );
        }

        this.Values = values;
    }

    public SwitchStatementLabel( IReadOnlyList<TypedConstant> values )
    {
        if ( values.Count == 0 )
        {
            throw new ArgumentOutOfRangeException( nameof(values), "At least one value is required." );
        }

        this.Values = values;
    }
}