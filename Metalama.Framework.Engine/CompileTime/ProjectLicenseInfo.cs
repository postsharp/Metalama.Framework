// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// Encapsulates the license information that is specific to the project, not to the user profile.
/// </summary>
/// <param name="RedistributionLicenseKey"></param>
public record ProjectLicenseInfo( string? RedistributionLicenseKey )
{
    public static ProjectLicenseInfo Empty { get; } = new ProjectLicenseInfo( default(string) );
}