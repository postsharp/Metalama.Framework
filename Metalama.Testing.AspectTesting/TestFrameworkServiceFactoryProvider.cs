// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;

namespace Metalama.Testing.AspectTesting;

internal static class TestFrameworkServiceFactoryProvider
{
    public static GlobalServiceProvider GetServiceProvider() => ServiceProviderFactory.GetServiceProvider().WithService( new TestAssemblyMetadataReader() );
}