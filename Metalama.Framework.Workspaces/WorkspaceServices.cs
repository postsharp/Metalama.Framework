// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Workspaces;

internal static class WorkspaceServices
{
    public static void Initialize()
    {
        // We don't initialize if another process has initialized.

        if ( !BackstageServiceFactoryInitializer.IsInitialized )
        {
            // Don't enforce licensing in workspaces.

            BackstageServiceFactoryInitializer.Initialize( new BackstageInitializationOptions( new WorkspaceApplicationInfo() ) );
        }
    }
}

public sealed class WorkspaceLoadException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public WorkspaceLoadException( string message, IReadOnlyList<string> errors ) : base( message )
    {
        this.Errors = errors;
    }
}