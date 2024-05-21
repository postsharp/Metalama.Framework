// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Testing.AspectTesting
{
    internal sealed class DesignTimeTestRunner : BaseTestRunner
    {
        public DesignTimeTestRunner(
            GlobalServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger,
            ILicenseKeyProvider? licenseKeyProvider )
            : base( serviceProvider, projectDirectory, references, logger, licenseKeyProvider ) { }

        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            TestContext testContext )
        {
            await base.RunAsync( testInput, testResult, testContext );

            using var pipeline = new TestDesignTimeAspectPipeline( testContext.ServiceProvider, testContext.Domain );

            var pipelineResult = await pipeline.ExecuteAsync( testResult.InputCompilation! );

            testResult.PipelineDiagnostics.Report( pipelineResult.Diagnostics );

            if ( pipelineResult.Success )
            {
                testResult.HasOutputCode = true;

                var introducedSyntaxTree = pipelineResult.AdditionalSyntaxTrees.SingleOrDefault();

                var introducedSyntaxRoot = introducedSyntaxTree == null
                    ? SyntaxFactory.CompilationUnit()
                    : await introducedSyntaxTree.GeneratedSyntaxTree.GetRootAsync();

                await testResult.SyntaxTrees.Single( x => !x.IsAuxiliary ).SetRunTimeCodeAsync( introducedSyntaxRoot );
                
                testResult.DiagnosticSuppressions = pipelineResult.Suppressions;

                if ( introducedSyntaxTree != null )
                {
                    var outputCompilation = testResult.InputCompilation!.AddSyntaxTrees( introducedSyntaxTree.GeneratedSyntaxTree );

                    testResult.OutputCompilation = outputCompilation;
                    testResult.OutputCompilationDiagnostics.Report( outputCompilation.GetDiagnostics() );
                }
                else
                {
                    testResult.OutputCompilation = testResult.InputCompilation;
                }
            }
            else
            {
                testResult.SetFailed( "DesignTimeAspectPipeline.TryExecute failed" );
            }
        }
    }
}