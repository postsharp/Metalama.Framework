// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Utilities;
using System.Collections.Immutable;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// Exposes the information needed to reconstruct a <see cref="Workspace"/>.
    /// </summary>
    [VisualBehavior( IsHidden = true )]
    public interface IWorkspaceLoadInfo
    {
        ImmutableArray<string> LoadedPaths { get; }

        ImmutableDictionary<string, string> Properties { get; }
    }
}