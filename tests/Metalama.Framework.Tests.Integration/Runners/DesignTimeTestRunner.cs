// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.Api.Options;
using Metalama.Testing.Framework;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    internal class DesignTimeTestRunner : BaseTestRunner
    {
        public DesignTimeTestRunner(
            GlobalServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, references, logger ) { }

        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            TestContextOptions projectOptions,
            Dictionary<string, object?> state )
        {
            await base.RunAsync( testInput, testResult, projectOptions, state );

            using var domain = new UnloadableCompileTimeDomain();

            using var pipeline = new TestDesignTimeAspectPipeline( testResult.ProjectScopedServiceProvider, domain );

            var pipelineResult = await pipeline.ExecuteAsync( testResult.InputCompilation! );

            testResult.PipelineDiagnostics.Report( pipelineResult.Diagnostics );

            if ( pipelineResult.Success )
            {
                testResult.HasOutputCode = true;

                var introducedSyntaxTree = pipelineResult.AdditionalSyntaxTrees.SingleOrDefault();

                var introducedSyntaxRoot = introducedSyntaxTree == null
                    ? SyntaxFactory.CompilationUnit()
                    : await introducedSyntaxTree.GeneratedSyntaxTree.GetRootAsync();

                await testResult.SyntaxTrees.Single().SetRunTimeCodeAsync( introducedSyntaxRoot );
            }
            else
            {
                testResult.SetFailed( "DesignTimeAspectPipeline.TryExecute failed" );
            }
        }
    }
}