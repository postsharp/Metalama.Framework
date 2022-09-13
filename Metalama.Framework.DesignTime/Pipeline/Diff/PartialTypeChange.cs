// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

internal readonly struct PartialTypeChange
{
    public TypeDependencyKey Type { get; }

    public PartialTypeChangeKind Kind { get; }

    public PartialTypeChange( TypeDependencyKey type, PartialTypeChangeKind kind )
    {
        this.Type = type;
        this.Kind = kind;
    }

    public PartialTypeChange Merge( PartialTypeChange change )
        => (this.Kind, change.Kind) switch
        {
            (_, PartialTypeChangeKind.None) => this,
            (PartialTypeChangeKind.None, _) => change,
            _ => new PartialTypeChange( this.Type, PartialTypeChangeKind.None )
        };
}