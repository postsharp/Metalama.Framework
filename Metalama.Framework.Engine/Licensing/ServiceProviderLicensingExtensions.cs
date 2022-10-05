// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.Engine.Licensing;

public static class ServiceProviderLicensingExtensions
{
    public static ServiceProvider AddDesignTimeLicenseConsumptionManager( this ServiceProvider serviceProvider, string? projectLicense, bool isTest )
    {
        // If this is a test and there's no project license, we're not testing licensing,
        // so no license consumption manager should be provided.
        if ( !isTest || projectLicense != null )
        {
            // We don't consider unattended license in design time.
            serviceProvider = serviceProvider.WithUntypedService(
                typeof(ILicenseConsumptionManager),
                BackstageServiceFactory.CreateLicenseConsumptionManager(
                    false,
                    isTest,
                    projectLicense ) );
        }

        return serviceProvider;
    }

    /// <summary>
    /// Adds the license verifier to the service provider. This method is called from the testing framework.
    /// </summary>
    public static ServiceProvider AddTestLicenseVerifier( this ServiceProvider serviceProvider, string licenseKey, string? targetAssemblyName )
    {
        var serviceProviderBuilder = new ServiceProviderBuilder(
            ( type, impl ) => serviceProvider = serviceProvider.WithUntypedService( type, impl ),
            () => serviceProvider );

        serviceProviderBuilder.AddLicensing( ignoreUserProfileLicenses: true, projectLicense: licenseKey );

        return serviceProvider.WithService(
            new LicenseVerifier( serviceProvider.GetRequiredBackstageService<ILicenseConsumptionManager>(), targetAssemblyName ) );
    }
}