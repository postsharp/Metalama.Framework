// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Services;
using System.IO;

namespace Metalama.Testing.AspectTesting.Licensing
{
    internal static class ServiceProviderLicensingTestsExtensions
    {
        public static ProjectServiceProvider AddLicenseConsumptionManagerForTest( this ProjectServiceProvider serviceProvider, TestInput testInput )
        {
            string? licenseKey = null;
            
            if ( testInput.Options.LicenseFile != null )
            {
                licenseKey = File.ReadAllText( Path.Combine( testInput.ProjectDirectory, testInput.Options.LicenseFile ) );
            }
            else if ( testInput.Options.LicenseExpression != null )
            {
                licenseKey = TestOptions.ReadLicenseExpression( testInput.Options.LicenseExpression );
            }

            return licenseKey == null ? serviceProvider : serviceProvider.Underlying.AddLicenseConsumptionManagerForLicenseKey( licenseKey );
        }
    }
}