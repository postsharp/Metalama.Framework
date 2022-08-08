// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Utilities.Diagnostics;

namespace Metalama.Framework.DesignTime;

internal static class DesignTimeServices
{
    public static void Initialize()
    {
        if ( !MetalamaCompilerInfo.IsActive )
        {
            BackstageServiceFactoryInitializer.Initialize<MetalamaDesignTimeApplicationInfo>();
        }
    }
}