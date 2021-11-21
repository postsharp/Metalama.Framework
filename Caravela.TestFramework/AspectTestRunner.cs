// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.CodeFixes;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
#if NET5_0
using Caravela.Framework.Code;
using System.Reflection;
using System.Runtime.Loader;
#endif

namespace Caravela.TestFramework
{
    /// <summary>
    /// Executes aspect integration tests by running the full aspect pipeline on the input source file.
    /// </summary>
    public class AspectTestRunner : BaseTestRunner
    {
        private int _runCount;

#if NET5_0
        private static readonly SemaphoreSlim _consoleLock = new( 1 );
#endif

        public AspectTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        // We don't want the base class to report errors in the input compilation because the pipeline does.

        private protected override bool ShouldStopOnInvalidInput( TestOptions testOptions ) => false;

        /// <summary>
        /// Runs the aspect test with the given name and source.
        /// </summary>
        /// <returns>The result of the test execution.</returns>
        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            Dictionary<string, object?> state )
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

            await base.RunAsync( testInput, testResult, state );

            if ( testResult.InputCompilation == null )
            {
                // The test was skipped.

                return;
            }

            var serviceProviderWithObserver = testResult.ProjectScopedServiceProvider.WithServices( new Observer( testResult ) );

            using var domain = new UnloadableCompileTimeDomain();

            var pipeline = new CompileTimeAspectPipeline( serviceProviderWithObserver, true, domain );

            var pipelineResult = await pipeline.ExecuteAsync(
                testResult.PipelineDiagnostics,
                testResult.InputCompilation!,
                default,
                CancellationToken.None );

            if ( pipelineResult != null )
            {
                if ( testInput.Options.ApplyCodeFix.GetValueOrDefault() )
                {
                    // When we test code fixed, we don't apply the pipeline output, but we apply the code fix instead.
                    if ( !await ApplyCodeFixAsync( testInput, testResult, domain ) )
                    {
                        return;
                    }
                }
                else
                {
                    if ( !await this.ProcessCompileTimePipelineOutputAsync( testInput, testResult, pipelineResult ) )
                    {
                        return;
                    }
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
        }

        private static async Task<bool> ApplyCodeFixAsync( TestInput testInput, TestResult testResult, CompileTimeDomain domain )
        {
            var codeFixes = testResult.PipelineDiagnostics.SelectMany( d => CodeFixTitles.GetCodeFixTitles( d ).Select( t => (Diagnostic: d, Title: t) ) );
            var codeFix = codeFixes.ElementAt( testInput.Options.AppliedCodeFixIndex.GetValueOrDefault() );
            var codeFixRunner = new CodeFixRunner( domain, new TestProjectOptions() );

            var inputDocument = testResult.SyntaxTrees[0].InputDocument;

            var transformedSolution = await codeFixRunner.ExecuteCodeFixAsync(
                inputDocument,
                codeFix.Diagnostic,
                codeFix.Title,
                CancellationToken.None );

            var transformedCompilation = await transformedSolution.GetProject( inputDocument.Project.Id)!.GetCompilationAsync(  );

            await testResult.SetOutputCompilationAsync( transformedCompilation! );
            testResult.HasOutputCode = true;

            return true;
        }

        private async Task<bool> ProcessCompileTimePipelineOutputAsync( TestInput testInput, TestResult testResult, CompileTimeAspectPipelineResult pipelineResult )
        {
            var resultCompilation = pipelineResult.ResultingCompilation.Compilation;
            testResult.OutputCompilation = resultCompilation;
            testResult.HasOutputCode = true;

            var minimalVerbosity = testInput.Options.ReportOutputWarnings.GetValueOrDefault() ? DiagnosticSeverity.Warning : DiagnosticSeverity.Error;

            await testResult.SetOutputCompilationAsync( resultCompilation );

            // Emit binary and report diagnostics.
            bool MustBeReported( Diagnostic d )
            {
                if (d.Id == "CS1701")
                {
                    // Ignore warning CS1701: Assuming assembly reference "Assembly Name #1" matches "Assembly Name #2", you may need to supply runtime policy.
                    // This warning is ignored by MSBuild anyway.
                    return false;
                }

                return d.Severity >= minimalVerbosity
                       && !testInput.Options.IgnoredDiagnostics.Contains( d.Id );
            }

            if (!testInput.Options.OutputCompilationDisabled.GetValueOrDefault())
            {
                // We don't build the PDB because the syntax trees were not written to disk anyway.
                var peStream = new MemoryStream();
                var emitResult = resultCompilation.Emit( peStream );

                testResult.OutputCompilationDiagnostics.Report( emitResult.Diagnostics.Where( MustBeReported ) );

                if (!emitResult.Success)
                {
                    testResult.SetFailed( "Final Compilation.Emit failed." );
                }
                else
                {
                    if (!this.VerifyBinaryStream( testInput, testResult, peStream ))
                    {
                        return false;
                    }

#if NET5_0
                    await ExecuteTestProgramAsync( testInput, testResult, peStream );
#endif
                }
            }
            else
            {
                testResult.OutputCompilationDiagnostics.Report( resultCompilation.GetDiagnostics().Where( MustBeReported ) );
            }

            return true;
        }

#if NET5_0
        private static async Task ExecuteTestProgramAsync( TestInput testInput, TestResult testResult, MemoryStream peStream, MemoryStream? pdbStream = null )
        {
            if ( !testInput.Options.ExecuteProgram.GetValueOrDefault( true ) )
            {
                return;
            }

            var mainMethod = FindProgramMain( testResult );

            if ( mainMethod == null )
            {
                return;
            }

            var loadContext = new AssemblyLoadContext( testInput.TestName, true );

            try
            {
                peStream.Seek( 0, SeekOrigin.Begin );
                var assembly = loadContext.LoadFromStream( peStream, pdbStream );
                var type = assembly.GetType( mainMethod.DeclaringType.FullName )!;
                var method = type.GetMethod( mainMethod.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )!;

                await _consoleLock.WaitAsync();

                try
                {
                    var oldConsoleOutput = Console.Out;
                    var oldConsoleError = Console.Error;
                    StringWriter outputWriter = new();
                    Console.SetOut( outputWriter );
                    Console.SetError( outputWriter );

                    try
                    {
                        if ( !mainMethod.IsAsync )
                        {
                            method.Invoke( null, null );
                        }
                        else
                        {
                            var task = (Task?) method.Invoke( null, null );

                            if ( task != null )
                            {
                                await task;
                            }
                            else
                            {
                                throw new InvalidOperationException( "Program.Main returned a null Task." );
                            }
                        }
                    }
                    finally
                    {
                        Console.SetOut( oldConsoleOutput );
                        Console.SetError( oldConsoleError );
                    }

                    testResult.ProgramOutput = outputWriter.ToString();
                }
                finally
                {
                    _consoleLock.Release();
                }
            }
            catch ( Exception e )
            {
                testResult.SetFailed( "Program execution failed", e );
            }
            finally
            {
                loadContext.Unload();
            }
        }

        private static IMethod? FindProgramMain( TestResult testResult )
        {
            if ( testResult.InitialCompilationModel == null )
            {
                return null;
            }

            var programTypes = testResult.InitialCompilationModel!.Types.Where( t => t.Name == "Program" ).ToList();

            switch ( programTypes.Count )
            {
                case 0:
                    return null;

                case 1:
                    break;

                default:
                    testResult.SetFailed( "The test cannot contain more classes named 'Program'." );

                    return null;
            }

            var programType = programTypes.Single();

            var mainMethods = programType.Methods.OfName( "Main" ).ToList();

            switch ( mainMethods.Count )
            {
                case 0:
                    return null;

                case 1:
                    break;

                default:
                    testResult.SetFailed( "The 'Program' class can contain a single method called 'Main'." );

                    return null;
            }

            var mainMethod = mainMethods.Single();

            if ( !mainMethod.IsStatic )
            {
                testResult.SetFailed( "The 'Program.Main' method must be static." );

                return null;
            }

            if ( mainMethod.IsAsync && mainMethod.ReturnType is not INamedType { Name: "Task" } )
            {
                testResult.SetFailed( "The 'Program.Main' method, if it is async, must be of return type 'Task'." );

                return null;
            }

            if ( mainMethod.Parameters.Count != 0 )
            {
                testResult.SetFailed( "The 'Program.Main' method must not have parameters." );

                return null;
            }

            return mainMethod;
        }
#endif

        private protected override void SaveResults( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
        {
            base.SaveResults( testInput, testResult, state );

            var expectedProgramOutputPath = Path.Combine(
                Path.GetDirectoryName( testInput.FullPath )!,
                Path.GetFileNameWithoutExtension( testInput.FullPath ) + FileExtensions.ProgramOutput );

            // Compare with expected program outputs.
            string? expectedOutput;

            var actualProgramOutput = NormalizeEndOfLines( testResult.ProgramOutput );

            // Update the file in obj/transformed if it is different.
            var actualProgramOutputPath = Path.Combine(
                this.ProjectDirectory!,
                "obj",
                "transformed",
                testInput.ProjectProperties.TargetFramework,
                Path.GetDirectoryName( testInput.RelativePath ) ?? "",
                Path.GetFileNameWithoutExtension( testInput.RelativePath ) + FileExtensions.ProgramOutput );

            if ( !string.IsNullOrWhiteSpace( actualProgramOutput ) )
            {
                // If the expectation file does not exist, create it with some placeholder content.
                if ( !File.Exists( expectedProgramOutputPath ) )
                {
                    // Coverage: Ignore

                    File.WriteAllText(
                        expectedProgramOutputPath,
                        "TODO: Replace this file with the correct program output. See the test output for the actual transformed code." );
                }

                if ( actualProgramOutput != expectedProgramOutputPath )
                {
                    File.WriteAllText( actualProgramOutputPath, actualProgramOutput );
                }

                this.Logger?.WriteLine( "=== ACTUAL PROGRAM OUTPUT ===" );
                this.Logger?.WriteLine( actualProgramOutput );
                this.Logger?.WriteLine( "=====================" );

                expectedOutput = NormalizeEndOfLines( File.ReadAllText( expectedProgramOutputPath ) );
            }
            else
            {
                expectedOutput = "";

                if ( File.Exists( expectedProgramOutputPath ) && string.IsNullOrWhiteSpace( File.ReadAllText( expectedProgramOutputPath ) ) )
                {
                    // Coverage: Ignore

                    File.Delete( expectedProgramOutputPath );
                }

                if ( File.Exists( actualProgramOutputPath ) )
                {
                    // Coverage: Ignore

                    File.Delete( actualProgramOutputPath );
                }
            }

            state["actualProgramOutput"] = expectedOutput;
            state["expectedProgramOutput"] = expectedOutput;
        }

        protected override void ExecuteAssertions( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
        {
            base.ExecuteAssertions( testInput, testResult, state );

            var expectedOutput = (string) state["expectedProgramOutput"]!;
            var actualOutput = (string) state["actualProgramOutput"]!;
            Assert.Equal( expectedOutput, actualOutput );
        }
    }
}