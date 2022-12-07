﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
    public class LicensingTestsBase : UnitTestClass
    {
        public LicensingTestsBase( ITestOutputHelper logger ) : base( logger ) { }

        protected async Task<DiagnosticBag> GetDiagnosticsAsync( string code, string licenseKey, string? assemblyName = null )
        {
            var mocks = new AdditionalServiceCollection();
            mocks.ProjectServices.Add( sp => sp.AddLicenseConsumptionManagerForLicenseKey( licenseKey ) );

            using var testContext = this.CreateTestContext( mocks );
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
                    this.TestOutput.WriteLine( $"{d.WarningLevel} {d.Id} {d.GetMessage( CultureInfo.CurrentCulture )}" );
                }
            }

            return diagnostics;
        }
    }
}