// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine.Utilities.Diagnostics;

namespace Metalama.Testing.UnitTesting;

internal static class TestingServices
{
    public static void Initialize()
    {
        // We don't initialize licensing because it depends on the project license key, which is not known at that time.

        BackstageServiceFactoryInitializer.Initialize( new BackstageInitializationOptions( new TestApiApplicationInfo() ) { AddSupportServices = true } );
    }
}