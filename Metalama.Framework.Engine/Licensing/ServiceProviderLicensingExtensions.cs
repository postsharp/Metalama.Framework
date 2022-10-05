// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.Engine.Licensing;

public static class ServiceProviderLicensingExtensions
{
    public static ServiceProvider AddLicenseConsumptionManagerForLicenseKey( this ServiceProvider serviceProvider, string projectLicense )
    {
        var licenseConsumptionManager = CreateLicenseConsumptionManager( projectLicense );

        serviceProvider = serviceProvider.WithUntypedService(
            typeof(ILicenseConsumptionManager),
            licenseConsumptionManager );

        return serviceProvider;
    }

    private static ILicenseConsumptionManager CreateLicenseConsumptionManager( string projectLicense )
    {
        var options = new LicensingInitializationOptions
        {
            IgnoreUnattendedProcessLicense = true, IgnoreUserProfileLicenses = true, ProjectLicense = projectLicense
        };

        var licenseConsumptionManager = BackstageServiceFactory.CreateLicenseConsumptionManager( options );

        return licenseConsumptionManager;
    }

    /// <summary>
    /// Adds the license verifier to the service provider. This method is called from the testing framework.
    /// </summary>
    public static ServiceProvider AddLicenseVerifierForLicenseKey( this ServiceProvider serviceProvider, string licenseKey, string? targetAssemblyName )
    {
        serviceProvider = serviceProvider.AddLicenseConsumptionManagerForLicenseKey( licenseKey );

        return serviceProvider
            .WithService( new LicenseVerifier( serviceProvider.GetRequiredBackstageService<ILicenseConsumptionManager>(), targetAssemblyName ) );
    }
}