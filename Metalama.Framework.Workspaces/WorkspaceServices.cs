// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Diagnostics;

namespace Metalama.Framework.Workspaces;

internal static class WorkspaceServices
{
    public static void Initialize()
    {
        // We don't initialize if another process has initialized.

        if ( !BackstageServiceFactoryInitializer.IsInitialized )
        {
            BackstageServiceFactoryInitializer.Initialize<WorkspaceApplicationInfo>();
        }
    }
}