// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.Licensing;

// TODO: Remove

public class DesignTimeLicenseVerifier : IService
{
    private readonly ILicenseConsumptionManager _licenseConsumptionManager;

    public string? RedistributionLicenseKey => this._licenseConsumptionManager.RedistributionLicenseKey;
    
    public DesignTimeLicenseVerifier( IServiceProvider serviceProvider )
    {
        var projectOptions = serviceProvider.GetRequiredService<IProjectOptions>();

        // this._licenseConsumptionManager = BackstageServiceFactory.CreateLicenseConsumptionManager( true, additionalLicense: projectOptions.AdditionalLicense );
    }

    public bool CanExecuteCodeAction( CodeActionModel codeAction, string? targetAssemblyName )
    {
        if ( !string.IsNullOrEmpty( codeAction.SourceRedistributionLicenseKey ) )
        {
            if ( this._licenseConsumptionManager.ValidateRedistributionLicenseKey(
                    codeAction.SourceRedistributionLicenseKey!,
                    codeAction.SourceAssemblyName! ) )
            {
                return true;
            }
        }

        return this._licenseConsumptionManager.CanConsume( LicenseRequirement.Professional, targetAssemblyName );
    }
}