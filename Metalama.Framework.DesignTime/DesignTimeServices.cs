// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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