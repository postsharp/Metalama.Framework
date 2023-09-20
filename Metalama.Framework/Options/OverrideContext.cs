﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Options;

/// <summary>
/// Context of an <see cref="IOverridable.OverrideWith"/> operation.
/// </summary>
[CompileTime]
public readonly struct OverrideContext
{
    /// <summary>
    /// Gets the axis along which the override operation is performed.
    /// </summary>
    public OverrideAxis Axis { get; }

    /// <summary>
    /// Gets the declaration for which the override operation is performed.
    /// </summary>
    public IDeclaration Declaration { get; }

    internal OverrideContext( OverrideAxis axis, IDeclaration declaration )
    {
        this.Axis = axis;
        this.Declaration = declaration;
    }
}