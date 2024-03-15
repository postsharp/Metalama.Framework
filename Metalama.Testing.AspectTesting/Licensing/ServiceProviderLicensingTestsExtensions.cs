// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Services;
using System;
using System.IO;

namespace Metalama.Testing.AspectTesting.Licensing
{
    internal static class ServiceProviderLicensingTestsExtensions
    {
        public static ProjectServiceProvider AddLicenseConsumptionManagerForTest( this ProjectServiceProvider serviceProvider, TestInput testInput )
        {
            string? licenseKey;

            if ( testInput.Options.LicenseFile != null )
            {
                licenseKey = File.ReadAllText( Path.Combine( testInput.SourceDirectory, testInput.Options.LicenseFile ) );
            }
            else if ( testInput.Options.LicenseExpression != null )
            {
                if ( testInput.Options.LicenseExpression.Equals( "none", StringComparison.OrdinalIgnoreCase ) )
                {
                    licenseKey = null;
                }
                else
                {
                    licenseKey = TestOptions.ReadLicenseExpression( testInput.Options.LicenseExpression );
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