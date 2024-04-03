// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using System;
using System.Collections.Generic;

namespace Metalama.Testing.AspectTesting.Licensing
{
    internal sealed class TestFrameworkLicenseStatus
    {
        private bool IsLicensed { get; }

        private IReadOnlyList<LicensingMessage> Messages { get; }

        public TestFrameworkLicenseStatus( string testProjectName, string? projectLicense, bool ignoreUserProfileLicenses )
        {
            // We don't use the service BackstageServiceFactory.ServiceProvider here,
            // because the additional license is test-assembly-specific.

            var applicationInfo = new TestFrameworkApplicationInfo();

            var options = new BackstageInitializationOptions( applicationInfo )
            {
                AddLicensing = true,
                AddSupportServices = false,
                LicensingOptions = new LicensingInitializationOptions()
                {
                    ProjectLicense = projectLicense,
                    IgnoreUnattendedProcessLicense = ignoreUserProfileLicenses,
                    IgnoreUserProfileLicenses = ignoreUserProfileLicenses
                }
            };

            var builder = new SimpleServiceProviderBuilder();
            builder.AddBackstageServices( options );
            var serviceProvider = builder.ServiceProvider;

            var licenseConsumptionManager = serviceProvider.GetRequiredBackstageService<ILicenseConsumptionService>();

            this.IsLicensed = licenseConsumptionManager.CanConsume( LicenseRequirement.Professional, testProjectName );

            this.Messages = licenseConsumptionManager.Messages;
        }

        public void ThrowIfNotLicensed()
        {
            if ( this.IsLicensed )
            {
                return;
            }

            var message = "The Metalama Test Framework cannot be used because this feature is not covered by your license.";

            if ( this.Messages.Count > 0 )
            {
                message += Environment.NewLine + string.Join( Environment.NewLine, this.Messages );
            }

            throw new InvalidLicenseException( message );
        }
    }
}