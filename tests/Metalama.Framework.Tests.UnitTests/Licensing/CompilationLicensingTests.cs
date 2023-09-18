// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public sealed class CompilationLicensingTests : LicensingTestsBase
    {
        public CompilationLicensingTests( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [InlineData( TestLicenseKeys.PostSharpEssentials )]
        [InlineData( TestLicenseKeys.PostSharpFramework )]
        [InlineData( TestLicenseKeys.PostSharpUltimate )]
        [InlineData( TestLicenseKeys.MetalamaFreePersonal )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness )]
        [InlineData( TestLicenseKeys.MetalamaUltimateBusiness )]
        [InlineData( TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution )]
        [InlineData( TestLicenseKeys.MetalamaUltimatePersonalProjectBound, TestLicenseKeys.MetalamaUltimateProjectBoundProjectName )]
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

            Assert.Empty( diagnostics );
        }
    }
}