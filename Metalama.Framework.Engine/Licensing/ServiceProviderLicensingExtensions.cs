// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Licensing;

public static class ServiceProviderLicensingExtensions
{
    public static ProjectServiceProvider AddProjectLicenseConsumptionManager(
        this ServiceProvider<IProjectService> serviceProvider,
        string? projectLicenseKey = null,
        LicenseSourceKind ignoreLicenseKinds = LicenseSourceKind.None,
        Action<Diagnostic>? diagnosticAdder = null )
    {
        var service = serviceProvider.GetRequiredBackstageService<ILicenseConsumptionService>();

        return serviceProvider.WithService(
            ProjectLicenseConsumer.Create(
                service,
                projectLicenseKey,
                ignoreLicenseKinds,
                diagnosticAdder ) );
    }

    public static ProjectServiceProvider AddProjectLicenseConsumptionManagerForTest(
        this ServiceProvider<IProjectService> serviceProvider,
        string? projectLicenseKey )
        => serviceProvider.WithService(
            ProjectLicenseConsumer.Create(
                BackstageServiceFactory.CreateTestLicenseConsumptionService( serviceProvider, projectLicenseKey ),
                projectLicenseKey,
                LicenseSourceKind.All ) );
}