// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption;
using System;

namespace Caravela.Framework.Tests.UnitTests.Utilities
{
    internal static class TestBackstageServiceProviderFactory
    {
        public static IServiceProvider Create()
        {
            // We can't add these services to the ServiceProviderFactory using the WithServices method
            // because the backstage interfaces do not inherit from IService.
            var backstageServices = new BackstageServiceCollection()
                .AddSingleton<IDiagnosticsSink>( new ThrowingDiagnosticsSink() )
                .AddSingleton<ILicenseConsumptionManager>( new DummyLicenseConsumptionManager() );

            return backstageServices.ToServiceProvider();
        }
    }
}