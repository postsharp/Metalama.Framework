// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.Engine.Licensing;

public static class ServiceProviderLicensingExtensions
{
    public static ServiceProvider AddLicenseConsumptionManager( this ServiceProvider serviceProvider, LicensingInitializationOptions options )
    {
        var licenseConsumptionManager = CreateLicenseConsumptionManager( options );

        serviceProvider = serviceProvider.WithUntypedService(
            typeof(ILicenseConsumptionManager),
            licenseConsumptionManager );

        return serviceProvider;
    }

    private static ILicenseConsumptionManager CreateLicenseConsumptionManager( LicensingInitializationOptions options )
    {
        var licenseConsumptionManager = BackstageServiceFactory.CreateLicenseConsumptionManager( options );

        return licenseConsumptionManager;
    }

    /// <summary>
    /// Adds the license verifier to the service provider. This method is called from the testing framework.
    /// </summary>
    public static ServiceProvider AddLicenseConsumptionManagerForLicenseKey( this ServiceProvider serviceProvider, string licenseKey )
    {
        // We always ignore user profile and unattended licenses in tests.
        return serviceProvider.AddLicenseConsumptionManager(
            new LicensingInitializationOptions { ProjectLicense = licenseKey, IgnoreUserProfileLicenses = true, IgnoreUnattendedProcessLicense = true } );
    }
}