// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.Engine.Licensing;

public static class LicenseVerifierFactory
{
    /// <summary>
    /// Adds the license verifier to the service provider. This method is called from the testing framework.
    /// </summary>
    public static ServiceProvider AddTestLicenseVerifier( this ServiceProvider serviceProvider, string licenseKey )
    {
        var serviceProviderBuilder = new ServiceProviderBuilder(
            ( type, impl ) => serviceProvider = serviceProvider.WithUntypedService( type, impl ),
            () => serviceProvider );

        serviceProviderBuilder.AddLicensing( additionalLicense: licenseKey, ignoreUserProfileLicenses: true );

        return serviceProvider.WithService( new LicenseVerifier( serviceProvider.GetRequiredBackstageService<ILicenseConsumptionManager>() ) );
    }
}