﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Testing;
using Metalama.TestFramework;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public class LicensingTestsBase : LoggingTestBase
    {
        public LicensingTestsBase( ITestOutputHelper logger ) : base( logger ) { }

        protected async Task<DiagnosticBag> GetDiagnosticsAsync( string code, string licenseKey, string? assemblyName = null )
        {
            var mocks = new TestServiceCollection();
            mocks.ProjectServices.Add( sp => sp.AddLicenseConsumptionManagerForLicenseKey( licenseKey ) );

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext( mocks );
            var inputCompilation = CreateCSharpCompilation( code, name: assemblyName );

            using var compileTimePipeline = new CompileTimeAspectPipeline(
                testContext.ServiceProvider,
                domain,
                ExecutionScenario.CompileTime );

            var diagnostics = new DiagnosticBag();
            _ = await compileTimePipeline.ExecuteAsync( diagnostics, inputCompilation, default );

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