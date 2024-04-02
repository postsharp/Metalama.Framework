// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Diagnostics;

[CompileTime]
[InternalImplement]
public interface ISuppression
{
    SuppressionDefinition Definition { get; }

    Func<SuppressionDiagnostic, bool>? Filter { get; }
}