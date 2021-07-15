// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal class DesignTimeTestRunner : BaseTestRunner
    {
        public DesignTimeTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        public override async Task<TestResult> RunTestAsync( TestInput testInput )
        {
            var testResult = await base.RunTestAsync( testInput );

            using var buildOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();

            var pipeline = new DesignTimeAspectPipeline( buildOptions, domain, true );
            var pipelineResult = pipeline.Execute( PartialCompilation.CreateComplete( testResult.InputCompilation! ), CancellationToken.None );

            testResult.Report( pipelineResult.Diagnostics.ReportedDiagnostics );

            if ( pipelineResult.Success )
            {
                var introducedSyntaxTree = pipelineResult.IntroducedSyntaxTrees.SingleOrDefault();

                var introducedSyntaxRoot = introducedSyntaxTree == null
                    ? SyntaxFactoryEx.EmptyStatement
                    : await introducedSyntaxTree.GeneratedSyntaxTree.GetRootAsync();

                await testResult.SyntaxTrees.Single().SetRunTimeCodeAsync( introducedSyntaxRoot );
            }
            else
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute failed." );
            }

            return testResult;
        }
    }
}