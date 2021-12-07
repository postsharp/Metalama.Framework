// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.DesignTime.Pipeline;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Pipeline;
using Metalama.Framework.Impl.Templating;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    internal class DesignTimeTestRunner : BaseTestRunner
    {
        public DesignTimeTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            Dictionary<string, object?> state )
        {
            await base.RunAsync( testInput, testResult, state );

            using var domain = new UnloadableCompileTimeDomain();

            using var pipeline = new DesignTimeAspectPipeline( testResult.ProjectScopedServiceProvider, domain, this.MetadataReferences, true );

            if ( !pipeline.TryExecute( testResult.InputCompilation!, CancellationToken.None, out var compilationResult ) )
            {
                testResult.SetFailed( "DesignTimeAspectPipeline.TryExecute failed" );

                return;
            }

            testResult.InputCompilationDiagnostics.Report( compilationResult.SyntaxTreeResults.SelectMany( t => t.Diagnostics ) );

            testResult.HasOutputCode = true;

            var introducedSyntaxTree = compilationResult.IntroducedSyntaxTrees.SingleOrDefault();

            var introducedSyntaxRoot = introducedSyntaxTree == null
                ? SyntaxFactory.GlobalStatement( SyntaxFactoryEx.EmptyStatement )
                : await introducedSyntaxTree.GeneratedSyntaxTree.GetRootAsync();

            await testResult.SyntaxTrees.Single().SetRunTimeCodeAsync( introducedSyntaxRoot );
        }
    }
}