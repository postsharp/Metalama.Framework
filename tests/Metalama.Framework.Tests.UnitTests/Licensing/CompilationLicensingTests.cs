// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
using Metalama.Framework.Engine.Licensing;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public sealed class CompilationLicensingTests : LicensingTestsBase
    {
        public CompilationLicensingTests( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [TestLicensesInlineData( nameof(TestLicenses.PostSharpEssentials) )]
        [TestLicensesInlineData( nameof(TestLicenses.PostSharpFramework) )]
        [TestLicensesInlineData( nameof(TestLicenses.PostSharpUltimate) )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreePersonal) )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterBusiness) )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalBusiness) )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateBusiness) )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateOpenSourceRedistribution) )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimatePersonalProjectBound), TestLicenses.MetalamaUltimateProjectBoundProjectName )]
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

            // We want to assert that the diagnostics are empty, but unit tests reference Metalama.Framework.Sdk,
            // so we need to ignore the Roslyn API license error.
            if ( diagnostics.Count > 0 )
            {
                Assert.Single( diagnostics, d => d.Id == LicensingDiagnosticDescriptors.RoslynApiNotAvailable.Id );
            }
        }
    }
}