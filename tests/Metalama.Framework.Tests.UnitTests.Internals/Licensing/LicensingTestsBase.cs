// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.TestFramework;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public class LicensingTestsBase : TestBase
    {
        protected async Task<DiagnosticList> GetDiagnosticsAsync( string code, string licenseKey )
        {
            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();
            var inputCompilation = CreateCSharpCompilation( code );
            var serviceProvider =
                testContext.ServiceProvider.AddTestLicenseVerifier( licenseKey );
            using var compileTimePipeline = new CompileTimeAspectPipeline(
                serviceProvider,
                true,
                domain,
                ExecutionScenario.CompileTime );
            var diagnostics = new DiagnosticList();
            var compileTimeResult = await compileTimePipeline.ExecuteAsync( diagnostics, inputCompilation, default, CancellationToken.None );
            Assert.Null( compileTimeResult );
            return diagnostics;
        }
    }
}