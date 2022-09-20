// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

internal partial class ProjectVersionProvider
{
    private partial class Implementation
    {
        private class ChangeLinkedList
        {
            private CompilationChanges? _nonIncrementalChanges;

            public ProjectVersion ProjectVersion { get; }

            public CompilationChanges NonIncrementalChanges => this._nonIncrementalChanges ??= CompilationChanges.NonIncremental( this.ProjectVersion );

            public IncrementalChangeNode? FirstIncrementalChange { get; private set; }

            public ChangeLinkedList( ProjectVersion projectVersion )
            {
                this.ProjectVersion = projectVersion;
            }

            public void Insert( CompilationChanges changes )
            {
                this.FirstIncrementalChange = new IncrementalChangeNode( changes, this.FirstIncrementalChange );
            }
        }
    }
}