// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Diagnostics;

public interface IDiagnosticBag : IDiagnosticAdder, IReadOnlyCollection<Diagnostic>
{
    void Clear();

    bool HasError { get; }
}