// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using System.Collections.Immutable;

namespace Caravela.Framework.Workspaces
{
    /// <summary>
    /// Exposes the information needed to reconstruct a <see cref="Workspace"/>.
    /// </summary>
    [DumpBehavior( IsHidden = true )]
    public interface IWorkspaceLoadInfo
    {
        ImmutableArray<string> LoadedPaths { get; }

        ImmutableDictionary<string, string> Properties { get; }
    }
}