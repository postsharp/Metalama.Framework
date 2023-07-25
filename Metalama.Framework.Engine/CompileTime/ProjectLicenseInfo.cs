// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// Encapsulates the license information that is specific to the project, not to the user profile.
/// </summary>
/// <param name="RedistributionLicenseKey"></param>
public sealed record ProjectLicenseInfo( string? RedistributionLicenseKey )
{
    internal static ProjectLicenseInfo Empty { get; } = new( default(string) );

    public static ProjectLicenseInfo Get( ILicenseConsumptionService? licenseConsumptionService ) 
        => licenseConsumptionService?.IsRedistributionLicense == true ? new ProjectLicenseInfo( licenseConsumptionService.LicenseString ) : Empty;
}