﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using System;
using System.Collections.Generic;

namespace Metalama.TestFramework.Licensing
{
    internal class TestFrameworkLicenseStatus
    {
        public bool IsLicensed { get; }

        public IReadOnlyList<LicensingMessage> Messages { get; }

        public TestFrameworkLicenseStatus( string testAssemblyName, string? additionalLicense )
        {
            var applicationInfo =
                BackstageServiceFactory.ServiceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;

            // We don't use the service BackstageServiceFactory.ServiceProvider here,
            // because the additional license is test-assembly-specific.
            var serviceProvider = new ServiceProviderBuilder()
                .AddBackstageServices( applicationInfo: applicationInfo, additionalLicense: additionalLicense )
                .ServiceProvider;

            var licenseConsumptionManager = serviceProvider.GetRequiredBackstageService<ILicenseConsumptionManager>();

            this.IsLicensed = licenseConsumptionManager.CanConsume( LicenseRequirement.Professional, testAssemblyName );

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