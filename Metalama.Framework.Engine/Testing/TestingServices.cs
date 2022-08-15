// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Diagnostics;

namespace Metalama.Framework.Engine.Testing;

public static class TestingServices
{
    public static void Initialize()
    {
        BackstageServiceFactoryInitializer.Initialize<TestFrameworkApplicationInfo>();
    }
}