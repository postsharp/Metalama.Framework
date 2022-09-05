// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Diagnostics;

namespace Metalama.Framework.Engine.Testing;

public static class TestingServices
{
    public static void Initialize()
    {
        BackstageServiceFactoryInitializer.Initialize<TestFrameworkApplicationInfo>();
    }
}