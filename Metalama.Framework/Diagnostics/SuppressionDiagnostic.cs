// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Collections.Immutable;

namespace Metalama.Framework.Diagnostics;

[CompileTime]
public struct SuppressionDiagnostic
{
    public SuppressionDiagnostic( string id, string invariantMessage, ImmutableArray<object?> arguments, in SourceSpan? span )
    {
        this.Id = id;
        this.InvariantMessage = invariantMessage;
        this.Arguments = arguments;
        this.Span = span;
    }

    public string Id { get; }

    public string InvariantMessage { get; }

    public ImmutableArray<object?> Arguments { get; }

    public SourceSpan? Span { get; }
}