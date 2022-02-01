// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using System.Collections.Immutable;

namespace Metalama.Framework.Introspection
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