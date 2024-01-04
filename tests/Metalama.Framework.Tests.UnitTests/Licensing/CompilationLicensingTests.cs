// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        [InlineData( nameof(TestLicenseKeys.PostSharpFramework) )]
        [InlineData(  nameof(TestLicenseKeys.PostSharpUltimate) )]
        [InlineData(  nameof(TestLicenseKeys.MetalamaFreePersonal) )]
        [InlineData(  nameof(TestLicenseKeys.MetalamaStarterBusiness) )]
        [InlineData(  nameof(TestLicenseKeys.MetalamaProfessionalBusiness) )]
        [InlineData(  nameof(TestLicenseKeys.MetalamaUltimateBusiness) )]
        [InlineData(  nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution) )]
        [InlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonalProjectBound), TestLicenseKeys.MetalamaUltimateProjectBoundProjectName )]
        public async Task CompilationPassesWithValidLicenseAsync( string licenseKeyName, string projectName = "TestProject" )
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

            AssertEmptyOrSdkOnly( diagnostics );
        }
    }
}
