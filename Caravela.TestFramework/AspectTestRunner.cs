// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Executes aspect integration tests by running the full aspect pipeline on the input source file.
    /// </summary>
    public class AspectTestRunner : BaseTestRunner
    {
        private int _runCount;
        private static readonly SemaphoreSlim _consoleLock = new( 1 );

        public AspectTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        // We don't want the base class to report errors in the input compilation because the pipeline does.
        protected override bool ReportInvalidInputCompilation => false;

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
            var testProjectOptions = (TestProjectOptions?) this.ServiceProvider.GetService( typeof(TestProjectOptions) );

            if ( testProjectOptions == null )
            {
                throw new InvalidOperationException( "The service provider does not contain a TestProjectOptions." );
            }

            var pipeline = new CompileTimeAspectPipeline( testProjectOptions, true, domain, testProjectOptions );
            var observer = new Observer( testResult );
            pipeline.ServiceProvider.AddService( observer );

            if ( pipeline.TryExecute( testResult.PipelineDiagnostics, testResult.InputCompilation!, CancellationToken.None, out var resultCompilation, out _ ) )
            {
                testResult.OutputCompilation = resultCompilation;
                testResult.HasOutputCode = true;

                var minimalVerbosity = testInput.Options.ReportOutputWarnings.GetValueOrDefault() ? DiagnosticSeverity.Warning : DiagnosticSeverity.Error;

                await testResult.SetOutputCompilationAsync( resultCompilation );

                // Emit binary and report diagnostics.
                bool MustBeReported( Diagnostic d ) => d.Severity >= minimalVerbosity && !testInput.Options.IgnoredDiagnostics.Contains( d.Id );

                if ( !testInput.Options.OutputCompilationDisabled.GetValueOrDefault() )
                {
                    // We don't build the PDB because the syntax trees were not written to disk anyway.
                    var peStream = new MemoryStream();
                    var emitResult = resultCompilation.Emit( peStream );

                    testResult.OutputCompilationDiagnostics.Report( emitResult.Diagnostics.Where( MustBeReported ) );

                    if ( !emitResult.Success )
                    {
                        testResult.SetFailed( "Final Compilation.Emit failed." );
                    }
                    else
                    {
                        await ExecuteTestProgramAsync( testInput, testResult, peStream );
                    }
                }
                else
                {
                    testResult.OutputCompilationDiagnostics.Report( resultCompilation.GetDiagnostics().Where( MustBeReported ) );
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

            var programTypes = testResult.InitialCompilationModel!.DeclaredTypes.Where( t => t.Name == "Program" ).ToList();

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

        public override void ExecuteAssertions( TestInput testInput, TestResult testResult )
        {
            base.ExecuteAssertions( testInput, testResult );

            // Compare with expected program outputs.
            if ( testResult.ProgramOutput != null )
            {
                var expectedProgramOutputPath = Path.Combine(
                    Path.GetDirectoryName( testInput.FullPath )!,
                    Path.GetFileNameWithoutExtension( testInput.FullPath ) + FileExtensions.ProgramOutput );

                // If the expectation file does not exist, create it with some placeholder content.
                if ( !File.Exists( expectedProgramOutputPath ) )
                {
                    File.WriteAllText(
                        expectedProgramOutputPath,
                        "TODO: Replace this file with the correct program output. See the test output for the actual transformed code." );
                }

                this.Logger?.WriteLine( "=== ACTUAL TRANSFORMED CODE ===" );
                this.Logger?.WriteLine( testResult.ProgramOutput );
                this.Logger?.WriteLine( "=====================" );

                var expectedOutput = File.ReadAllText( expectedProgramOutputPath );

                Assert.Equal( expectedOutput, testResult.ProgramOutput );
            }
        }
    }
}