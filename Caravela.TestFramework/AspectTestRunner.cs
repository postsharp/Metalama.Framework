// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Executes aspect integration tests by running the full aspect pipeline on the input source file.
    /// </summary>
    public class AspectTestRunner : BaseTestRunner
    {
        public AspectTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        /// <summary>
        /// Runs the aspect test with the given name and source.
        /// </summary>
        /// <returns>The result of the test execution.</returns>
        public override TestResult RunTest( TestInput testInput )
        {
            var testResult = base.RunTest( testInput );

            using var testProjectOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();

            var pipeline = new CompileTimeAspectPipeline( testProjectOptions, domain, true, testProjectOptions );
            var spy = new Spy( testResult );
            pipeline.ServiceProvider.AddService<ICompileTimeCompilationBuilderSpy>( spy );
            pipeline.ServiceProvider.AddService<ITemplateCompilerSpy>( spy );

            if ( pipeline.TryExecute( testResult, testResult.InputCompilation!, CancellationToken.None, out var resultCompilation, out _ ) )
            {
                testResult.OutputCompilation = resultCompilation;

                if ( testInput.Options.IncludeFinalDiagnostics.GetValueOrDefault() )
                {
                    testResult.Report( resultCompilation.GetDiagnostics().Where( d => d.Severity >= DiagnosticSeverity.Warning ) );
                }

                testResult.SetOutputCompilation( resultCompilation );
            }
            else
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute failed." );
            }

            if ( testInput.Options.WriteHtml.GetValueOrDefault() )
            {
                this.WriteHtml( testInput, testResult );
            }

            return testResult;
        }

        // We don't want the base class to report errors in the input compilation because the pipeline does.
        protected override bool ReportInvalidInputCompilation => false;
    }
}