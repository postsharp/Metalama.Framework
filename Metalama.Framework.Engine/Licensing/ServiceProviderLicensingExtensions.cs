// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.Licensing;

public static class ServiceProviderLicensingExtensions
{
    public static ProjectServiceProvider AddLicenseConsumptionManager(
        this ServiceProvider<IProjectService> serviceProvider,
        LicensingInitializationOptions options )
    {
        return serviceProvider.WithService( new ProjectLicenseConsumptionService( options ) );
    }

    /// <summary>
    /// Adds the license verifier to the service provider. This method is called from the testing framework.
    /// </summary>
    public static ProjectServiceProvider AddLicenseConsumptionManagerForLicenseKey( this ServiceProvider<IProjectService> serviceProvider, string licenseKey )
    {
        // We always ignore user profile and unattended licenses in tests.
        return serviceProvider.AddLicenseConsumptionManager(
            new LicensingInitializationOptions { ProjectLicense = licenseKey, IgnoreUserProfileLicenses = true, IgnoreUnattendedProcessLicense = true } );
    }
}