// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Options;

[CompileTime]
public enum HierarchicalOptionsOverrideAxis
{
    /// <summary>
    /// Means that options on a contained declaration override options defined in the base declaration.
    /// For instance, type-level options on the declaring type of an <c>override</c> method override method-level options on the <c>base</c> method.
    /// </summary>
    DeclaringType,

    /// <summary>
    /// Means that options directly applied to the declaration override other options also directly applied to the declaration.
    /// </summary>
    Self,

    /// <summary>
    /// Means that options directly applied to the declaration override options inherited along the containment or base axis.
    /// </summary>
    Declaration,
    Aspect,
    Namespace
}