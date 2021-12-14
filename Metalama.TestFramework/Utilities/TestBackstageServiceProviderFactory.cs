// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework.Licensing;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption;
using System;

namespace Metalama.TestFramework.Utilities
{
    /// <summary>
    /// Factory creating a service provider with backstage service implementations for testing.
    /// </summary>
    internal static class TestBackstageServiceProviderFactory
    {
        /// <summary>
        /// Creates a service provider with backstage service implementations for testing.
        /// </summary>
        /// <returns>A service provider with backstage service implementations for testing.</returns>
        public static IServiceProvider Create()
        {
            var diagnosticsSink = new TestBackstageDiagnosticsSink();

            var services = new ServiceCollection();

            var serviceProviderBuilder = new ServiceProviderBuilder(
                ( type, service ) => services.AddService( type, service ),
                () => services.GetServiceProvider() );

            // We can't add these services to the ServiceProviderFactory using the WithServices method
            // because the backstage interfaces do not inherit from IService.
            serviceProviderBuilder
                .AddSingleton<IBackstageDiagnosticSink>( diagnosticsSink )

                // This allows the retrieval of the service using its type name
                .AddSingleton( diagnosticsSink )
                .AddSingleton<ILicenseConsumptionManager>( new DummyLicenseConsumptionManager() );

            return serviceProviderBuilder.ServiceProvider;
        }
    }
}