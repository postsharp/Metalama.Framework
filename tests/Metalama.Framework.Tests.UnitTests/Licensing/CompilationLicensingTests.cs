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
        [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpEssentials) )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpFramework) )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpUltimate) )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaFreePersonal) )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness) )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness) )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness) )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution) )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonalProjectBound), TestLicenseKeys.MetalamaUltimateProjectBoundProjectName )]
        public async Task CompilationPassesWithValidLicenseAsync( string licenseKey, string projectName = "TestProject" )
        {
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