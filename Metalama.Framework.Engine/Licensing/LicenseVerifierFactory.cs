using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.Engine.Licensing;

public static class LicenseVerifierFactory
{
    /// <summary>
    /// Adds the license verifier to the service provider. This method is called from the testing framework.
    /// </summary>
    public static ServiceProvider AddTestLicenseVerifier( ServiceProvider serviceProvider, string licenseKey )
    {
        // TODO: add the license consumer.
        return serviceProvider.WithService( new LicenseVerifier(serviceProvider) );
    }
}