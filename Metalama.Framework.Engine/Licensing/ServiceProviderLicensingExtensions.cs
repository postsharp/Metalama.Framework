// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.Licensing;

public static class ServiceProviderLicensingExtensions
{
    public static ProjectServiceProvider AddLicenseConsumptionManager(
        this ServiceProvider<IProjectService> serviceProvider, string? projectLicenseKey )
    {
        var service = serviceProvider.GetRequiredBackstageService<ILicenseConsumptionService>().WithAdditionalLicense( projectLicenseKey );
        
        return serviceProvider.WithService( new ProjectLicenseConsumptionService( service ) );
    }
}