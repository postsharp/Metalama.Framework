// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.TestFramework;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public class LicensingTestsBase : TestBase
    {
        public LicensingTestsBase( ITestOutputHelper logger ) : base( logger ) { }

        protected async Task<DiagnosticList> GetDiagnosticsAsync( string code, string licenseKey, string? assemblyName = null )
        {
            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();
            var inputCompilation = CreateCSharpCompilation( code, name: assemblyName );

            var serviceProvider =
                testContext.ServiceProvider.AddTestLicenseVerifier( licenseKey );

            using var compileTimePipeline = new CompileTimeAspectPipeline(
                serviceProvider,
                true,
                domain,
                ExecutionScenario.CompileTime );

            var diagnostics = new DiagnosticList();
            _ = await compileTimePipeline.ExecuteAsync( diagnostics, inputCompilation, default, CancellationToken.None );

            if ( diagnostics.Count == 0 )
            {
                this.Logger.WriteLine( "No diagnostics reported." );
            }
            else
            {
                foreach ( var d in diagnostics )
                {
                    this.Logger.WriteLine( $"{d.WarningLevel} {d.Id} {d.GetMessage()}" );
                }
            }

            return diagnostics;
        }
    }
}