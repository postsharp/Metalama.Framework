// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.Engine.Licensing;

public static class LicenseVerifierFactory
{
    /// <summary>
    /// Adds the license verifier to the service provider. This method is called from the testing framework.
    /// </summary>
    public static ServiceProvider AddTestLicenseVerifier( this ServiceProvider serviceProvider, string licenseKey, string? targetAssemblyName )
    {
        var serviceProviderBuilder = new ServiceProviderBuilder(
            ( type, impl ) => serviceProvider = serviceProvider.WithUntypedService( type, impl ),
            () => serviceProvider );

        serviceProviderBuilder.AddLicensing( projectLicense: licenseKey, ignoreUserProfileLicenses: true );

        return serviceProvider.WithService(
            new LicenseVerifier( serviceProvider.GetRequiredBackstageService<ILicenseConsumptionManager>(), targetAssemblyName ) );
    }
}