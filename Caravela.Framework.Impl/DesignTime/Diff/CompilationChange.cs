// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime.Diff
{
    /// <summary>
    /// Represents changes between two instances of the <see cref="Microsoft.CodeAnalysis.Compilation"/> class.
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
        public CompilationChange(
            ImmutableArray<SyntaxTreeChange> syntaxTreeChanges,
            bool hasCompileTimeCodeChange,
            Compilation compilationToAnalyze,
            bool isIncremental )
        {
            this.SyntaxTreeChanges = syntaxTreeChanges;
            this.HasCompileTimeCodeChange = hasCompileTimeCodeChange;
            this.CompilationToAnalyze = compilationToAnalyze;
            this.IsIncremental = isIncremental;
        }

        public static CompilationChange Empty( Compilation compilation ) => new( ImmutableArray<SyntaxTreeChange>.Empty, false, compilation, true );

        public bool HasChange => this.SyntaxTreeChanges.Length > 0 || this.HasCompileTimeCodeChange;

        public bool IsIncremental { get; }

        /// <summary>
        /// Gets the <see cref="Microsoft.CodeAnalysis.Compilation"/> that must be analyzed. If <see cref="HasChange"/> is false,
        /// this is the last compilation of <see cref="CompilationChangeTracker"/>. Otherwise, this is the new compilation. 
        /// </summary>
        public Compilation CompilationToAnalyze { get; }

        public override string ToString() => $"HasCompileTimeCodeChange={this.HasCompileTimeCodeChange}, SyntaxTreeChanges={this.SyntaxTreeChanges.Length}";
    }
}