// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

internal sealed partial class ProjectVersionProvider
{
    private sealed partial class Implementation
    {
        private sealed class IncrementalChangeNode
        {
            /// <summary>
            /// Gets the incremental changes between the compilation at the head of the linked list
            /// and the value of <see cref="CompilationChanges.NewProjectVersion"/>.
            /// </summary>
            public CompilationChanges IncrementalChanges { get; }

            public IncrementalChangeNode( CompilationChanges changes, IncrementalChangeNode? next )
            {
                this.IncrementalChanges = changes;
                this.Next = next;
            }

            public IncrementalChangeNode? Next { get; }
        }
    }
}