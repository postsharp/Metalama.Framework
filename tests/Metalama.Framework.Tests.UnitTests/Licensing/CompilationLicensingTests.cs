﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public sealed class CompilationLicensingTests : LicensingTestsBase
    {
        public CompilationLicensingTests( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [InlineData( null )]
        [InlineData( nameof(LicenseKeys.PostSharpFramework) )]
        [InlineData( nameof(LicenseKeys.PostSharpUltimate) )]
        [InlineData( nameof(LicenseKeys.MetalamaFreePersonal) )]
        [InlineData( nameof(LicenseKeys.MetalamaStarterBusiness) )]
        [InlineData( nameof(LicenseKeys.MetalamaProfessionalBusiness) )]
        [InlineData( nameof(LicenseKeys.MetalamaUltimateBusiness) )]
        [InlineData( nameof(LicenseKeys.MetalamaUltimateOpenSourceRedistribution) )]
        [InlineData( nameof(LicenseKeys.MetalamaUltimatePersonalProjectBound), TestLicenseKeyProvider.MetalamaUltimateProjectBoundProjectName )]
        public async Task CompilationPassesWithNoMetalamaUsageAsync( string licenseKeyName, string projectName = "TestProject" )
        {
            var licenseKey = GetLicenseKey( licenseKeyName );
            
            const string code = @"
using System;

namespace HelloWorld;

class Test
{
    void SayHello()
    {
        Console.WriteLine(""Hello world!"");
    }
}";

            var diagnostics = await this.GetDiagnosticsAsync( code, licenseKey, projectName: projectName );

            Assert.Empty( diagnostics );
            Assert.False( this.ToastNotifications.WasDetectionTriggered );
        }
    }
}
