// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Licensing;

namespace Metalama.Framework.DesignTime.Licensing;

internal class DesignTimeLicenseConsumptionManagerProvider : ILicenseConsumptionManagerProvider
{
    public ILicenseConsumptionManager LicenseConsumptionManager { get; }

    public DesignTimeLicenseConsumptionManagerProvider( string? additionalLicense )
    {
        // We don't consider unattended license in design time.
        this.LicenseConsumptionManager = BackstageServiceFactory.CreateLicenseConsumptionManager( additionalLicense: additionalLicense );
    }
}