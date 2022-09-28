// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Licensing;

namespace Metalama.Framework.DesignTime.Licensing;

internal class DesignTimeLicenseConsumptionManagerProvider : ILicenseConsumptionManagerProvider
{
    public ILicenseConsumptionManager? LicenseConsumptionManager { get; }

    public DesignTimeLicenseConsumptionManagerProvider( string? projectLicense, bool isTest )
    {
        // If this is a test and there's no project license, we're not testing licensing,
        // so no license consumption manager should be provided.
        if ( !isTest || projectLicense != null )
        {
            // We don't consider unattended license in design time.
            this.LicenseConsumptionManager = BackstageServiceFactory.CreateLicenseConsumptionManager( false, isTest, projectLicense );
        }
    }
}