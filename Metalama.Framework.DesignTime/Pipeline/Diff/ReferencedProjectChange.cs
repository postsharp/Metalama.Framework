// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

/// <summary>
/// Represents a change in a referenced project.
/// </summary>
internal readonly struct ReferencedProjectChange
{
    public ReferenceChangeKind ChangeKind { get; }

    public Compilation? NewCompilation { get; }

    public Compilation? OldCompilation { get; }

    public bool HasCompileTimeCodeChange => this.ChangeKind != ReferenceChangeKind.Modified || this.Changes.AssertNotNull().HasCompileTimeCodeChange;

    /// <summary>
    /// Gets the changes in the referenced compilation, but only if <see cref="ChangeKind"/> is <see cref="ReferenceChangeKind.Modified"/>.
    /// Specifically, the property is not set when <see cref="ChangeKind"/> is <see cref="ReferenceChangeKind.Added"/>.
    /// </summary>
    public CompilationChanges? Changes { get; }

    public ReferencedProjectChange(
        Compilation? oldCompilation,
        Compilation? newCompilation,
        ReferenceChangeKind changeKind,
        CompilationChanges? changes = null )
    {
        this.OldCompilation = oldCompilation;
        this.NewCompilation = newCompilation;
        this.ChangeKind = changeKind;
        this.Changes = changes;
    }
}

internal readonly struct ReferencedPortableExecutableChange
{
    public ReferenceChangeKind ChangeKind { get; }

    public string Path { get; }

    public bool HasCompileTimeCodeChange => this.ChangeKind != ReferenceChangeKind.None;

    public ReferencedPortableExecutableChange( ReferenceChangeKind changeKind, string path )
    {
        this.ChangeKind = changeKind;
        this.Path = path;
    }
}