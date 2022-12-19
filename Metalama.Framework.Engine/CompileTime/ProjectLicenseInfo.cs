// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// Encapsulates the license information that is specific to the project, not to the user profile.
/// </summary>
/// <param name="RedistributionLicenseKey"></param>
public sealed record ProjectLicenseInfo( string? RedistributionLicenseKey )
{
    public static ProjectLicenseInfo Empty { get; } = new( default(string) );
}