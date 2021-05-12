// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class CompilationChanges
    {
        public ImmutableArray<SyntaxTreeChange> SyntaxTreeChanges { get; }

        public bool HasCompileTimeCodeChange { get; }

        public CompilationChanges( ImmutableArray<SyntaxTreeChange> syntaxTreeChanges, bool hasCompileTimeCodeChange )
        {
            this.SyntaxTreeChanges = syntaxTreeChanges;
            this.HasCompileTimeCodeChange = hasCompileTimeCodeChange;
        }

        private CompilationChanges()
        {
            this.SyntaxTreeChanges = ImmutableArray<SyntaxTreeChange>.Empty;
        }

        public static CompilationChanges Empty { get; } = new();
    }
}