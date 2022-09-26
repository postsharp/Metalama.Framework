// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption;

namespace Metalama.Framework.Engine.Licensing;

internal class CompileTimeLicenseConsumptionManagerProvider : ILicenseConsumptionManagerProvider
{
    public ILicenseConsumptionManager LicenseConsumptionManager { get; }
    
    public CompileTimeLicenseConsumptionManagerProvider( ILicenseConsumptionManager licenseConsumptionManager )
    {
        this.LicenseConsumptionManager = licenseConsumptionManager;
    }
}