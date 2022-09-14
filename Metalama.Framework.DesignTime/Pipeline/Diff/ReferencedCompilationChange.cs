// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

internal readonly struct ReferencedCompilationChange
{
    public ReferencedCompilationChangeKind ChangeKind { get; }

    public Compilation? NewCompilation { get; }

    public Compilation? OldCompilation { get; }

    public bool HasCompileTimeCodeChange => this.ChangeKind != ReferencedCompilationChangeKind.Modified || this.Changes.AssertNotNull().HasCompileTimeCodeChange;

    /// <summary>
    /// Gets the changes in the referenced compilation, but only if <see cref="ChangeKind"/> is <see cref="ReferencedCompilationChangeKind.Modified"/>.
    /// Specifically, the property is not set when <see cref="ChangeKind"/> is <see cref="ReferencedCompilationChangeKind.Added"/>.
    /// </summary>
    public CompilationChanges? Changes { get; }

    public ReferencedCompilationChange(
        Compilation? oldCompilation,
        Compilation? newCompilation,
        ReferencedCompilationChangeKind changeKind,
        CompilationChanges? changes = null )
    {
        this.OldCompilation = oldCompilation;
        this.NewCompilation = newCompilation;
        this.ChangeKind = changeKind;
        this.Changes = changes;
    }
}