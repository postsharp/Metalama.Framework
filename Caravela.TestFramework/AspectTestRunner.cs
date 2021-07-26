﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Executes aspect integration tests by running the full aspect pipeline on the input source file.
    /// </summary>
    public class AspectTestRunner : BaseTestRunner
    {
        private int _runCount;
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
        public override async Task<TestResult> RunTestAsync( TestInput testInput )
        {
            if ( this._runCount > 0 )
            {
                // We are reusing the TestProjectOptions from the service provider, so we cannot run a test twice with the same service provider.
                throw new InvalidOperationException( "The Run method can be called only once." );
            }
            else
            {
                this._runCount++;
            }
            
            var testResult = await base.RunTestAsync( testInput );

            
            using var domain = new UnloadableCompileTimeDomain();
            var testProjectOptions = (TestProjectOptions) this.ServiceProvider.GetService( typeof(TestProjectOptions) );
            var pipeline = new CompileTimeAspectPipeline( testProjectOptions, true, domain, testProjectOptions );
            var spy = new Spy( testResult );
            pipeline.ServiceProvider.AddService( spy );
            pipeline.ServiceProvider.AddService( spy );

            if ( pipeline.TryExecute( testResult, testResult.InputCompilation!, CancellationToken.None, out var resultCompilation, out _ ) )
            {
                testResult.OutputCompilation = resultCompilation;
                testResult.HasOutputCode = true;

                var minimalVerbosity = testInput.Options.ReportOutputWarnings.GetValueOrDefault() ? DiagnosticSeverity.Warning : DiagnosticSeverity.Error;

                await testResult.SetOutputCompilationAsync( resultCompilation );

                bool MustBeReported( Diagnostic d ) => d.Severity >= minimalVerbosity && !testInput.Options.IgnoredDiagnostics.Contains( d.Id );

                if ( !testInput.Options.OutputCompilationDisabled.GetValueOrDefault() )
                {
                    var emitResult = resultCompilation.Emit( Stream.Null );
                    testResult.Report( emitResult.Diagnostics.Where( MustBeReported ) );

                    if ( !emitResult.Success )
                    {
                        testResult.SetFailed( "Final Compilation.Emit failed." );
                    }
                }
                else
                {
                    testResult.Report( resultCompilation.GetDiagnostics().Where( MustBeReported ) );
                }
            }
            else
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute failed." );
            }

            if ( testInput.Options.WriteInputHtml.GetValueOrDefault() || testInput.Options.WriteOutputHtml.GetValueOrDefault() )
            {
                await this.WriteHtmlAsync( testInput, testResult );
            }

            return testResult;
        }

        // We don't want the base class to report errors in the input compilation because the pipeline does.
        protected override bool ReportInvalidInputCompilation => false;
    }
}