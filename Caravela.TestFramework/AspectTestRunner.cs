// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Executes aspect integration tests by running the full aspect pipeline on the input source file.
    /// </summary>
    public class AspectTestRunner : TestRunnerBase
    {
        public AspectTestRunner( string? projectDirectory = null ) : base( projectDirectory ) { }

        /// <summary>
        /// Runs the aspect test with the given name and source.
        /// </summary>
        /// <returns>The result of the test execution.</returns>
        public override async Task<TestResult> RunTestAsync( TestInput testInput )
        {
            var testResult = await base.RunTestAsync( testInput );

            var pipeline = new CompileTimeAspectPipeline( new TestBuildOptions() );

            if ( pipeline.TryExecute( testResult, testResult.InitialCompilation, out var resultCompilation, out _ ) )
            {
                testResult.ResultCompilation = resultCompilation;
                var syntaxRoot = resultCompilation.SyntaxTrees.Single().GetRoot();

                if ( testInput.Options.IncludeFinalDiagnostics )
                {
                    testResult.ReportDiagnostics( resultCompilation.GetDiagnostics().Where( d => d.Severity >= DiagnosticSeverity.Warning ) );
                }

                testResult.SetTransformedTarget( syntaxRoot );
            }
            else
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute failed." );
            }

            return testResult;
        }

        // We don't want the base class to report errors in the input compilation because the pipeline does.
        protected override bool ReportInvalidInputCompilation => false;
    }
}