// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.AspectTesting.Licensing;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
#if NET5_0_OR_GREATER
using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using System.Reflection;
using System.Runtime.Loader;
using SpecialType = Metalama.Framework.Code.SpecialType;
#endif

namespace Metalama.Testing.AspectTesting;

/// <summary>
/// Executes aspect integration tests by running the full aspect pipeline on the input source file.
/// </summary>
internal class AspectTestRunner : BaseTestRunner
{
    private int _runCount;

#if NET5_0_OR_GREATER
    private static readonly SemaphoreSlim _consoleLock = new( 1 );
#endif

    public AspectTestRunner(
        GlobalServiceProvider serviceProvider,
        string? projectDirectory,
        TestProjectReferences references,
        ITestOutputHelper? logger )
        : base( serviceProvider, projectDirectory, references, logger ) { }

    // We don't want the base class to report errors in the input compilation because the pipeline does.

    private protected override bool ShouldStopOnInvalidInput( TestOptions testOptions ) => false;

    /// <summary>
    /// Runs the aspect test with the given name and source.
    /// </summary>
    /// <returns>The result of the test execution.</returns>
    protected override async Task RunAsync(
        TestInput testInput,
        TestResult testResult,
        TestContext testContext,
        TestTextResult textResult )
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

        await base.RunAsync( testInput, testResult, testContext, textResult );

        if ( testResult.InputCompilation == null )
        {
            // The test was skipped.

            return;
        }

        var serviceProviderForThisTestWithoutLicensing = testContext.ServiceProvider
            .WithService( new Observer( testContext.ServiceProvider, testResult ) );

        var serviceProviderForThisTestWithLicensing = serviceProviderForThisTestWithoutLicensing
            .AddLicenseConsumptionManagerForTest( testInput );

        var testScenario = testInput.Options.TestScenario ?? TestScenario.Default;

        var isLicensingRequiredForCompilation = testScenario switch
        {
            TestScenario.ApplyCodeFix => false,
            TestScenario.PreviewCodeFix => false,
            TestScenario.Default => true,
            _ => throw new InvalidOperationException( $"Unknown test scenario: {testScenario}" )
        };

        var pipeline = new CompileTimeAspectPipeline(
            isLicensingRequiredForCompilation ? serviceProviderForThisTestWithLicensing : serviceProviderForThisTestWithoutLicensing,
            testContext.Domain );

        var pipelineResult = await pipeline.ExecuteAsync(
            testResult.PipelineDiagnostics,
            testResult.InputCompilation!,
            default );

        if ( pipelineResult.IsSuccessful && !testResult.PipelineDiagnostics.HasError )
        {
            switch ( testScenario )
            {
                case TestScenario.ApplyCodeFix:
                case TestScenario.PreviewCodeFix:
                    {
                        // When we test code fixes, we don't apply the pipeline output, but we apply the code fix instead.
                        if ( !await ApplyCodeFixAsync(
                                testInput,
                                testResult,
                                testContext.Domain,
                                serviceProviderForThisTestWithLicensing,
                                testInput.Options.TestScenario == TestScenario.PreviewCodeFix ) )
                        {
                            return;
                        }

                        break;
                    }

                case TestScenario.Default:
                    {
                        if ( !await this.ProcessCompileTimePipelineOutputAsync( testInput, testResult, pipelineResult.Value ) )
                        {
                            return;
                        }

                        break;
                    }

                default:
                    throw new InvalidOperationException( $"Unknown test scenario: {testScenario}" );
            }
        }
        else
        {
            testResult.SetFailed( "CompileTimeAspectPipeline.ExecuteAsync failed." );
        }

        if ( testInput.Options.WriteInputHtml.GetValueOrDefault() || testInput.Options.WriteOutputHtml.GetValueOrDefault() )
        {
            await this.WriteHtmlAsync( testInput, testResult );
        }
    }

    private static async Task<bool> ApplyCodeFixAsync(
        TestInput testInput,
        TestResult testResult,
        CompileTimeDomain domain,
        ProjectServiceProvider serviceProvider,
        bool isComputingPreview )
    {
        var codeFixes = testResult.PipelineDiagnostics.SelectMany(
            d => CodeFixTitles.GetCodeFixTitles( d ).SelectAsReadOnlyList( t => (Diagnostic: d, Title: t) ) );

        var codeFix = codeFixes.ElementAt( testInput.Options.AppliedCodeFixIndex.GetValueOrDefault() );
        var codeFixRunner = new StandaloneCodeFixRunner( domain, serviceProvider );

        var codeActionResult = await codeFixRunner.ExecuteCodeFixAsync(
            testResult.InputCompilation.AssertNotNull(),
            codeFix.Diagnostic.Location.SourceTree.AssertNotNull(),
            codeFix.Diagnostic.Id,
            codeFix.Diagnostic.Location.SourceSpan,
            codeFix.Title,
            isComputingPreview,
            default );

        Assert.NotNull( codeActionResult );

        if ( !codeActionResult.IsSuccessful )
        {
            Assert.NotNull( codeActionResult.ErrorMessages );

            testResult.SetFailed( $"Code fix runner execution failed: {string.Join( "; ", codeActionResult.ErrorMessages! )}" );

            return false;
        }

        Assert.Null( codeActionResult.ErrorMessages );
        Assert.NotNull( testResult.InputProject );

        var transformedSolution = await codeActionResult.ApplyAsync( testResult.InputProject!, NullLogger.Instance, true, CancellationToken.None );
        var transformedCompilation = await transformedSolution.GetProject( testResult.InputProject!.Id )!.GetCompilationAsync();

        await testResult.SetOutputCompilationAsync( transformedCompilation! );
        testResult.HasOutputCode = true;

        return true;
    }

    private async Task<bool> ProcessCompileTimePipelineOutputAsync(
        TestInput testInput,
        TestResult testResult,
        CompileTimeAspectPipelineResult pipelineResult )
    {
        var resultCompilation = pipelineResult.ResultingCompilation.Compilation;
        testResult.OutputCompilation = resultCompilation;
        testResult.HasOutputCode = true;

        var minimalVerbosity = testInput.Options.ReportOutputWarnings.GetValueOrDefault() ? DiagnosticSeverity.Warning : DiagnosticSeverity.Error;

        await testResult.SetOutputCompilationAsync( resultCompilation );

        // Emit binary and report diagnostics.
        bool MustBeReported( Diagnostic d )
        {
            if ( d.Id == "CS1701" )
            {
                // Ignore warning CS1701: Assuming assembly reference "Assembly Name #1" matches "Assembly Name #2", you may need to supply runtime policy.
                // This warning is ignored by MSBuild anyway.
                return false;
            }

            return d.Severity >= minimalVerbosity
                   && !testInput.ShouldIgnoreDiagnostic( d.Id );
        }

        if ( !SyntaxTreeStructureVerifier.Verify( resultCompilation, out var diagnostics ) )
        {
            testResult.SetFailed( "Syntax tree verification failed." );
            testResult.OutputCompilationDiagnostics.Report( diagnostics );

            return false;
        }

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
                if ( !this.VerifyBinaryStream( testInput, testResult, peStream ) )
                {
                    return false;
                }

#if NET5_0_OR_GREATER
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

#if NET5_0_OR_GREATER
    private static async Task ExecuteTestProgramAsync( TestInput testInput, TestResult testResult, MemoryStream peStream, MemoryStream? pdbStream = null )
    {
        if ( !testInput.Options.ExecuteProgram.GetValueOrDefault( true ) )
        {
            return;
        }

        var mainMethod = FindProgramMain( testInput.Options, testResult );

        if ( mainMethod == null )
        {
            return;
        }

        var loadContext = new AssemblyLoadContext( testInput.TestName, true );

        try
        {
            foreach ( var reference in testResult.OutputCompilation.AssertNotNull().References.OfType<PortableExecutableReference>() )
            {
                if ( Path.GetFileName( reference.FilePath.AssertNotNull() ).StartsWith( "dependency_", StringComparison.Ordinal ) )
                {
                    loadContext.LoadFromAssemblyPath( reference.FilePath! );
                }
            }

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
                    var parameters = method.GetParameters();

                    var result = parameters switch
                    {
                        [] => method.Invoke( null, null ),
                        [not null] => method.Invoke( null, new object[] { Array.Empty<string>() } ),
                        _ => throw new InvalidOperationException( "Program.Main has unsupported signature." )
                    };

                    if ( mainMethod.GetAsyncInfo() is { IsAwaitable: true } )
                    {
                        switch ( result )
                        {
                            case Task task:
                                // Await normal task.
                                await task;

                                break;

                            default:
                                throw new InvalidOperationException( "Program.Main did not return Task." );
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

    private static IMethod? FindProgramMain( TestOptions testOptions, TestResult testResult )
    {
        if ( testResult.InitialCompilationModel == null )
        {
            return null;
        }

        var mainMethodName = testOptions.MainMethod ?? "Main";

        var programTypes = testResult.InitialCompilationModel!.Types.Where( t => t.Name == "Program" ).ToReadOnlyList();

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

        var mainMethods = programType.Methods.OfName( mainMethodName ).ToReadOnlyList();

        switch ( mainMethods.Count )
        {
            case 0:
                return null;

            case 1:
                break;

            default:
                testResult.SetFailed( $"The 'Program' class can contain a single method called '{mainMethodName}'." );

                return null;
        }

        var mainMethod = mainMethods.Single();

        if ( !mainMethod.IsStatic )
        {
            testResult.SetFailed( $"The 'Program.{mainMethodName}' method must be static." );

            return null;
        }

        if ( !(mainMethod is
                { ReturnType: INamedType { SpecialType: SpecialType.Void or SpecialType.Int32 or SpecialType.Task } }
                or { ReturnType: INamedType { Definition.SpecialType: SpecialType.Task_T, TypeArguments: [{ SpecialType: SpecialType.Int32 }] } }) )
        {
            testResult.SetFailed( $"The 'Program.{mainMethodName}' method must return void, int, Task or Task<int>." );

            return null;
        }

        if ( !(mainMethod.Parameters is [] or [{ Type: IArrayType { ElementType.SpecialType: SpecialType.String } }]) )
        {
            testResult.SetFailed( $"The 'Program.{mainMethodName}' method must not have parameters or have a single 'string[]' parameter." );

            return null;
        }

        return mainMethod;
    }
#endif

    private protected override void SaveResults( TestInput testInput, TestResult testResult, TestTextResult textResult )
    {
        base.SaveResults( testInput, testResult, textResult );

        var expectedProgramOutputPath = Path.Combine(
            Path.GetDirectoryName( testInput.FullPath )!,
            Path.GetFileNameWithoutExtension( testInput.FullPath ) + FileExtensions.ProgramOutput );

        // Compare with expected program outputs.
        string? expectedProgramOutput;

        var actualProgramOutput = TestOutputNormalizer.NormalizeEndOfLines( testResult.ProgramOutput );

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

            expectedProgramOutput = TestOutputNormalizer.NormalizeEndOfLines( File.ReadAllText( expectedProgramOutputPath ) );

            if ( actualProgramOutput != expectedProgramOutput )
            {
                File.WriteAllText( actualProgramOutputPath, actualProgramOutput );
            }

            this.Logger?.WriteLine( "=== ACTUAL PROGRAM OUTPUT ===" );
            this.Logger?.WriteLine( actualProgramOutput );
            this.Logger?.WriteLine( "=====================" );
        }
        else
        {
            expectedProgramOutput = "";

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

        var aspectTestTextResult = (AspectTestTextResult) textResult;

        aspectTestTextResult.SetProgramOutput( actualProgramOutput, actualProgramOutputPath, expectedProgramOutput, expectedProgramOutputPath );
    }

    protected override void ExecuteAssertions( TestInput testInput, TestResult testResult, TestTextResult textResult )
    {
        base.ExecuteAssertions( testInput, testResult, textResult );

        if ( testInput.Options.CompareProgramOutput ?? true )
        {
            var aspectTestTextResult = (AspectTestTextResult) textResult;

            this.AssertTextEqual(
                aspectTestTextResult.ExpectedProgramOutputText!,
                aspectTestTextResult.ExpectedProgramOutputPath!,
                aspectTestTextResult.ActualProgramOutputText!,
                aspectTestTextResult.ActualProgramOutputPath! );
        }

#if DEBUG
        if ( testInput.Options.AcceptInvalidInput != true && testResult.IntermediateLinkerCompilation != null )
        {
            // The following is useful when debugging weird linker problems.
            // Linker should never produce invalid compilation by itself, but usually recovers quite well.

            // TODO: Commented out because we have some tests that generate invalid syntax either into the intermediate compilation or even to the final one.

            /*
            var intermediateDiagnostics = testResult.IntermediateLinkerCompilation.Compilation.GetDiagnostics();

            var intermediateLinkerError =
                intermediateDiagnostics.FirstOrDefault( d => d is { Severity: DiagnosticSeverity.Error } or { Severity: DiagnosticSeverity.Warning, IsWarningAsError: true } );

            if ( intermediateLinkerError != null )
            {
                throw new InvalidOperationException( $"Invalid intermediate compilation: {intermediateLinkerError}" );
            }
            */
        }
#endif
    }

    protected override TestTextResult CreateTestState() => new AspectTestTextResult();

    private sealed class AspectTestTextResult : TestTextResult
    {
        public string? ActualProgramOutputText { get; private set; }

        public string? ActualProgramOutputPath { get; private set; }

        public string? ExpectedProgramOutputText { get; private set; }

        public string? ExpectedProgramOutputPath { get; private set; }

        public void SetProgramOutput(
            string actualProgramOutputText,
            string actualProgramOutputPath,
            string expectedProgramOutputText,
            string expectedProgramOutputPath )
        {
            this.ActualProgramOutputText = actualProgramOutputText;
            this.ActualProgramOutputPath = actualProgramOutputPath;
            this.ExpectedProgramOutputText = expectedProgramOutputText;
            this.ExpectedProgramOutputPath = expectedProgramOutputPath;
        }
    }
}