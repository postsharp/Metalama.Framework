// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
using Metalama.Backstage.UserInterface;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.UnitTesting;
using System.Globalization;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public abstract class LicensingTestsBase : UnitTestClass
    {
        private protected TestToastNotificationDetectionService ToastNotifications { get; } = new();

        protected LicensingTestsBase( ITestOutputHelper logger ) : base( logger ) { }

        protected static TestLicenseKeyProvider LicenseKeys { get; } = new TestLicenseKeyProvider( typeof(LicensingTestsBase).Assembly );

        protected async Task<DiagnosticBag> GetDiagnosticsAsync(
            string code,
            string? licenseKey,
            string? assemblyName = "AspectCountTests.ArbitraryNamespace",
            string projectName = "TestProject" )
        {
            var mocks = new AdditionalServiceCollection();
            mocks.ProjectServices.Add( sp => sp.AddProjectLicenseConsumptionManagerForTest( licenseKey ) );

            mocks.BackstageServices.Add<IToastNotificationDetectionService>( _ => this.ToastNotifications );
            
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

        protected static string? GetLicenseKey( string? name )
        {
            if ( name == null )
            {
                return null;
            }

            return LicenseKeys.GetLicenseKey( name );
        }
    }
}