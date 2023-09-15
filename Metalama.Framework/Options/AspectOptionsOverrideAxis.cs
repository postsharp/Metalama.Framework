// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Options;

[CompileTime]
public enum AspectOptionsOverrideAxis
{
    /// <summary>
    /// Options defined on a contained declaration override options defined on the containing declaration.
    /// For instance, type-level options override namespace-level options.
    /// </summary>
    Containment,

    /// <summary>
    /// Options on a contained declaration override options defined in the base declaration.
    /// For instance, type-level options on the declaring type of an <c>override</c> method override method-level options on the <c>base</c> method.
    /// </summary>
    ContainmentOverBase,

    /// <summary>
    /// Options directly applied to the declaration override other options directly applied to the declaration.
    /// </summary>
    Self,

    /// <summary>
    /// Options directly applied to the declaration override options inherited along the containment or base axis.
    /// </summary>
    DirectOverInheritance
}