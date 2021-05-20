// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime.Diff
{
    /// <summary>
    /// Represents changes between two instances of the <see cref="Compilation"/> class.
    /// </summary>
    internal sealed class CompilationChange
    {
        /// <summary>
        /// Gets the set of syntax tree changes.
        /// </summary>
        public ImmutableArray<SyntaxTreeChange> SyntaxTreeChanges { get; }

        /// <summary>
        /// Gets a value indicating whether the changes affects the compile-time subproject.
        /// </summary>
        public bool HasCompileTimeCodeChange { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationChange"/> class.
        /// </summary>
        public CompilationChange( ImmutableArray<SyntaxTreeChange> syntaxTreeChanges, bool hasCompileTimeCodeChange )
        {
            this.SyntaxTreeChanges = syntaxTreeChanges;
            this.HasCompileTimeCodeChange = hasCompileTimeCodeChange;
        }

        private CompilationChange()
        {
            this.SyntaxTreeChanges = ImmutableArray<SyntaxTreeChange>.Empty;
        }

        /// <summary>
        /// Gets a <see cref="CompilationChange"/> that represents the absence of any change.
        /// </summary>
        public static CompilationChange Empty { get; } = new();
    }
}