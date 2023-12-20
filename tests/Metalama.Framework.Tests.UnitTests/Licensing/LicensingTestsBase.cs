// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.UnitTesting;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public abstract class LicensingTestsBase : UnitTestClass
    {
        protected LicensingTestsBase( ITestOutputHelper logger ) : base( logger ) { }

        protected async Task<DiagnosticBag> GetDiagnosticsAsync(
            string code,
            string? licenseKey,
            string? assemblyName = "AspectCountTests.ArbitraryNamespace",
            string projectName = "TestProject" )
        {
            var mocks = new AdditionalServiceCollection();
            mocks.ProjectServices.Add( sp => sp.AddProjectLicenseConsumptionManagerForTest( licenseKey ) );

            var testContextOptions = this.GetDefaultTestContextOptions() with { ProjectName = projectName };

            using var testContext = this.CreateTestContext( testContextOptions, mocks );
            var domain = testContext.Domain;

            var inputCompilation = TestCompilationFactory.CreateCSharpCompilation( code, name: assemblyName );

            using var compileTimePipeline = new CompileTimeAspectPipeline(
                testContext.ServiceProvider,
                domain,
                ExecutionScenario.CompileTime );

            var diagnostics = new DiagnosticBag();
            _ = await compileTimePipeline.ExecuteAsync( diagnostics, inputCompilation, default );

            if ( diagnostics.Count == 0 )
            {
                this.TestOutput.WriteLine( "No diagnostics reported." );
            }
            else
            {
                foreach ( var d in diagnostics )
                {
                    this.TestOutput.WriteLine( $"{d.WarningLevel} {d.Id} {d.GetMessage( CultureInfo.InvariantCulture )}" );
                }
            }

            return diagnostics;
        }

        protected static void AssertEmptyOrSdkOnly( DiagnosticBag diagnostics )
        {
            // We want to assert that the diagnostics are empty, but unit tests reference Metalama.Framework.Sdk,
            // so we need to ignore the Roslyn API license error, if present.
            // The SDK license error is tested in integration tests separately.
            if ( diagnostics.Count == 0 )
            {
                return;
            }

            // First, we need to make sure there is only one diagnostic in total. Running the next assert only would ignore other diagnostics.
            Assert.Single( diagnostics );
            Assert.Single( diagnostics, d => d.Id == LicensingDiagnosticDescriptors.RoslynApiNotAvailable.Id );
        }
    }
}