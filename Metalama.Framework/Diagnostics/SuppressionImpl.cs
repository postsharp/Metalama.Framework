// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Diagnostics;

internal sealed class SuppressionImpl( SuppressionDefinition definition, Func<SuppressionDiagnostic, bool> filter ) : ISuppression
{
    public SuppressionDefinition Definition { get; } = definition;

    public Func<SuppressionDiagnostic, bool> Filter { get; } = filter;

    public override string ToString() => $"{this.Definition} with filter";
}