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
        [TestLicensesInlineData( "PostSharp Framework", nameof(TestLicenseKeys.PostSharpFramework) )]
        [TestLicensesInlineData( "PostSharp Ultimate", nameof(TestLicenseKeys.PostSharpUltimate) )]
        [TestLicensesInlineData( "Metalama Free Personal", nameof(TestLicenseKeys.MetalamaFreePersonal) )]
        [TestLicensesInlineData( "Metalama Started Business", nameof(TestLicenseKeys.MetalamaStarterBusiness) )]
        [TestLicensesInlineData( "Metalama Professional Business", nameof(TestLicenseKeys.MetalamaProfessionalBusiness) )]
        [TestLicensesInlineData( "Metalama Ultimate Business", nameof(TestLicenseKeys.MetalamaUltimateBusiness) )]
        [TestLicensesInlineData( "Metalama Ultimate Open-Source Redistribution", nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution) )]
        [TestLicensesInlineData(
            "Metalama Ultimate Personal Project-Bound",
            nameof(TestLicenseKeys.MetalamaUltimatePersonalProjectBound),
            TestLicenseKeys.MetalamaUltimateProjectBoundProjectName )]
        public async Task CompilationPassesWithValidLicenseAsync( string licenseName, string licenseKey, string projectName = "TestProject" )
        {
            _ = licenseName;

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
