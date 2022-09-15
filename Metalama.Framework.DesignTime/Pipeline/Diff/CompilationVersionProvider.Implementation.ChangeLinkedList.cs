// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

internal partial class CompilationVersionProvider
{
    private partial class Implementation
    {
        private class ChangeLinkedList
        {
            private CompilationChanges? _nonIncrementalChanges;

            public CompilationVersion CompilationVersion { get; }

            public CompilationChanges NonIncrementalChanges => this._nonIncrementalChanges ??= CompilationChanges.NonIncremental( this.CompilationVersion );

            public IncrementalChangeNode? FirstIncrementalChange { get; private set; }

            public ChangeLinkedList( CompilationVersion compilationVersion )
            {
                this.CompilationVersion = compilationVersion;
            }

            public void Insert( CompilationChanges changes )
            {
                this.FirstIncrementalChange = new IncrementalChangeNode( changes, this.FirstIncrementalChange );
            }
        }
    }
}