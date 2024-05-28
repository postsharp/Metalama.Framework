// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using System;
using System.Collections.Immutable;

namespace Metalama.Testing.AspectTesting.Licensing
{
    internal sealed class TestFrameworkLicenseStatus
    {
        private readonly ImmutableArray<LicensingMessage> _messages;

        private bool IsLicensed { get; }

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
                    IgnoreUnattendedProcessLicense = ignoreUserProfileLicenses, IgnoreUserProfileLicenses = ignoreUserProfileLicenses
                }
            };

            var builder = new SimpleServiceProviderBuilder();
            builder.AddBackstageServices( options );
            var serviceProvider = builder.ServiceProvider;

            var consumer = serviceProvider.GetRequiredBackstageService<ILicenseConsumptionService>()
                .CreateConsumer(
                    projectLicense,
                    ignoreUserProfileLicenses ? LicenseSourceKind.All : LicenseSourceKind.None,
                    out this._messages );

            this.IsLicensed = consumer.CanConsume( LicenseRequirement.Professional, testProjectName );
        }

        public void ThrowIfNotLicensed()
        {
            if ( this.IsLicensed )
            {
                return;
            }

            var message = "The Metalama Test Framework cannot be used because this feature is not covered by your license.";

            if ( this._messages.Length > 0 )
            {
                message += Environment.NewLine + string.Join( Environment.NewLine, this._messages );
            }

            throw new InvalidLicenseException( message );
        }
    }
}