// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline;
using System.IO;

namespace Metalama.TestFramework.Licensing
{
    internal static class ServiceProviderLicensingTestsExtensions
    {
        public static ServiceProvider AddLicenseConsumptionManagerForTest( this ServiceProvider serviceProvider, TestInput testInput )
        {
            if ( testInput.Options.LicenseFile == null )
            {
                return serviceProvider;
            }

            // ReSharper disable once MethodHasAsyncOverload
            var licenseKey = File.ReadAllText( Path.Combine( testInput.ProjectDirectory, testInput.Options.LicenseFile ) );

            return serviceProvider.AddLicenseConsumptionManagerForLicenseKey( licenseKey );
        }
    }
}