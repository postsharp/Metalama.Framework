namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// Encapsulates the license information that is specific to the project, not to the user profile.
/// </summary>
/// <param name="RedistributionLicenseKey"></param>
public record ProjectLicenseInfo( string? RedistributionLicenseKey )
{
    public static ProjectLicenseInfo Empty { get; } = new ProjectLicenseInfo( default(string) );
}