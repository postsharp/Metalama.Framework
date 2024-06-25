// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;

namespace Metalama.Framework.Workspaces;

public sealed class WorkspaceLoadException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public WorkspaceLoadException( string message, IReadOnlyList<string> errors ) : base( message )
    {
        this.Errors = errors;
    }
}