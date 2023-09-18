// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Options;

[RunTimeOrCompileTime]
[AttributeUsage( AttributeTargets.Class )]
public sealed class HierarchicalOptionsAttribute : Attribute
{
    internal static HierarchicalOptionsAttribute Default { get; } = new();

    public bool InheritedByDerivedTypes { get; init; } = true;

    public bool InheritedByOverridingMembers { get; init; } = true;

    public bool InheritedByNestedTypes { get; init; } = true;

    public bool InheritedByMembers { get; init; } = true;
}