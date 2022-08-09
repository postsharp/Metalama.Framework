namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// Encapsulates the license information that is specific to the project, not to the user profile.
/// </summary>
/// <param name="LicenseKey"></param>
public record ProjectLicenseInfo( string? LicenseKey )
{
    public static ProjectLicenseInfo Empty { get; } = new ProjectLicenseInfo( default(string) );
}