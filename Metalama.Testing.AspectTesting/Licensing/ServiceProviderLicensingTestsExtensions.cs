// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Services;
using System;

namespace Metalama.Testing.AspectTesting.Licensing
{
    internal static class ServiceProviderLicensingTestsExtensions
    {
        public static ProjectServiceProvider AddLicenseConsumptionManagerForTest(
            this ProjectServiceProvider serviceProvider,
            TestInput testInput,
            ILicenseKeyProvider licenseKeyProvider )
        {
            string? licenseKey;

            if ( testInput.Options.LicenseKey != null )
            {
                if ( !licenseKeyProvider.TryGetLicenseKey( testInput.Options.LicenseKey, out licenseKey ) )
                {
                    throw new InvalidOperationException( $"The license key name {testInput.Options.LicenseKey} is invalid." );
                }
            }
            else
            {
                return serviceProvider;
            }

            return serviceProvider.Underlying.AddProjectLicenseConsumptionManagerForTest( licenseKey );
        }
    }
}