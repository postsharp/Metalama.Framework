// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.UnitTesting;
using System;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Testing.AspectTesting;

internal class PreviewTestRunner : BaseTestRunner
{
    internal PreviewTestRunner(
        GlobalServiceProvider serviceProvider,
        string? projectDirectory,
        TestProjectReferences references,
        ITestOutputHelper? logger,
        ILicenseKeyProvider? licenseKeyProvider ) : base( serviceProvider, projectDirectory, references, logger, licenseKeyProvider ) { }

    protected override async Task RunAsync( TestInput testInput, TestResult testResult, TestContext testContext )
    {
        await base.RunAsync( testInput, testResult, testContext );

        var previewPipeline = new PreviewAspectPipeline(
            testContext.ServiceProvider,
            ExecutionScenario.Preview,
            testContext.Domain );

        throw new NotImplementedException();
    }
}
