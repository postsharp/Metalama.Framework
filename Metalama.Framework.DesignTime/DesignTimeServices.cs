﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Compiler;
using Metalama.Framework.Engine.Utilities.Diagnostics;

namespace Metalama.Framework.DesignTime;

internal static class DesignTimeServices
{
    public static void Initialize()
    {
        if ( !MetalamaCompilerInfo.IsActive )
        {
            // We don't initialize licensing because it depends on the project license key, which is not known at that time.

            BackstageServiceFactoryInitializer.Initialize(
                new BackstageInitializationOptions( new MetalamaDesignTimeApplicationInfo() ) { AddSupportServices = true, OpenWelcomePage = true } );
        }
    }
}