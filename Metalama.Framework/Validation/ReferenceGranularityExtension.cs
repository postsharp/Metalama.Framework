// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Validation;

/// <summary>
/// Extensions of the <see cref="ReferenceGranularity"/> enum.
/// </summary>
[CompileTime]
[PublicAPI]
public static class ReferenceGranularityExtension
{
    public static ReferenceGranularity CombineWith( this ReferenceGranularity a, ReferenceGranularity b ) => a > b ? a : b;
}