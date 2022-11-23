// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Project;
using System.IO;

namespace Metalama.TestFramework.Licensing
{
    internal static class ServiceProviderLicensingTestsExtensions
    {
        public static ProjectServiceProvider AddLicenseConsumptionManagerForTest( this ProjectServiceProvider serviceProvider, TestInput testInput )
        {
            if ( testInput.Options.LicenseFile == null )
            {
                return serviceProvider;
            }

            // ReSharper disable once MethodHasAsyncOverload
            var licenseKey = File.ReadAllText( Path.Combine( testInput.ProjectDirectory, testInput.Options.LicenseFile ) );

            return serviceProvider.Underlying.AddLicenseConsumptionManagerForLicenseKey( licenseKey );
        }
    }
}