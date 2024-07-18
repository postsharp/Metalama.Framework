// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Utilities;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Testing.AspectTesting.Utilities;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Document = Microsoft.CodeAnalysis.Document;

// ReSharper disable MethodHasAsyncOverload

namespace Metalama.Testing.AspectTesting;

/// <summary>
/// An abstract class for all template-base tests.
/// </summary>
internal abstract partial class BaseTestRunner
{
    private static readonly AsyncLocal<bool> _isTestRunning = new();

    private readonly TestProjectReferences _references;
    private readonly IFileSystem _fileSystem;

    private protected BaseTestRunner(
        GlobalServiceProvider serviceProvider,
        string? projectDirectory,
        TestProjectReferences references,
        ITestOutputHelper? logger )
    {
        this._references = references;
        this.ProjectDirectory = projectDirectory;
        this.Logger = logger;
        this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
    }

    /// <summary>
    /// Gets the project directory, or <c>null</c> if it is unknown.
    /// </summary>
    [PublicAPI]
    public string? ProjectDirectory { get; }

    protected ITestOutputHelper? Logger { get; }

    public async Task RunAndAssertAsync( TestInput testInput, TestContextOptions testContextOptions )
    {
        CollectibleExecutionContext? collectibleExecutionContext;

        if ( testInput.Options.CheckMemoryLeaks == true )
        {
            collectibleExecutionContext = CollectibleExecutionContext.Open();

            CollectibleExecutionContext.RegisterDisposeAction( () => this.Logger?.WriteLine( "Disposing the CollectibleExecutionContext." ) );
        }
        else
        {
            collectibleExecutionContext = null;
        }

        try
        {
            try
            {
                await this.RunAndAssertCoreAsync( testInput, testContextOptions );
            }
            finally
            {
                // This is a trick to make the current task, on the heap, stop having a reference to the previous
                // task. This allows TestExecutionContext.Dispose to perform a full GC. Without Task.Yield, we will
                // have references to the objects that are in the scope of the test.
                await Task.Yield();
            }
        }
        catch ( Exception ex1 )
        {
            // If the test throws an exception due to a bug, it may also prevent unloading.
            // In that case, throw both the exception from the test and the unloading exception, wrapped in AggregateException.
            try
            {
                collectibleExecutionContext?.Dispose();
                collectibleExecutionContext = null;
            }
            catch ( Exception ex2 )
            {
                collectibleExecutionContext = null;

                throw new AggregateException( ex1, ex2 );
            }

            throw;
        }
        finally
        {
            collectibleExecutionContext?.Dispose();
        }
    }

    private async Task RunAndAssertCoreAsync( TestInput testInput, TestContextOptions testContextOptions )
    {
        // Avoid run too many tests in parallel regardless of the way the runners are scheduled.
        using ( await TestThrottlingHelper.StartTestAsync() )
        {
            var originalCulture = CultureInfo.CurrentCulture;

            try
            {
                // Change the culture to invariant to get invariant diagnostic messages.
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

                testInput.ProjectProperties.License?.ThrowIfNotLicensed();

                var transformedOptions = this.GetContextOptions( testContextOptions )
                    with
                    {
                        ProjectName = testInput.Options.ProjectName ?? testInput.TestName
                    };

                using var testContext = new TestContext( transformedOptions );

                Dictionary<string, object?> state = new( StringComparer.Ordinal );
                using var testResult = new TestResult();
                await this.RunAsync( testInput, testResult, testContext, state );
                this.SaveResults( testInput, testResult, state );
                this.ExecuteAssertions( testInput, testResult, state );
            }
            catch ( Exception e ) when ( e.GetType().FullName == testInput.Options.ExpectedException
                                         || (e.InnerException?.GetType().FullName is { } innerException
                                             && innerException == testInput.Options.ExpectedException) )
            {
                return;
            }
            finally
            {
                // Restore the culture.
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        if ( testInput.Options.ExpectedException != null )
        {
            throw new AssertionFailedException( $"The expected exception {testInput.Options.ExpectedException} has not been thrown." );
        }
    }

    [PublicAPI]
    public Task RunAsync( TestInput testInput, TestResult testResult, TestContext testContext )
        => this.RunAsync(
            testInput,
            testResult,
            testContext,
            new Dictionary<string, object?>( StringComparer.InvariantCulture ) );

    protected virtual TestContextOptions GetContextOptions( TestContextOptions options ) => options;

    /// <summary>
    /// Runs a test. The present implementation of this method only prepares an input project and stores it in the <see cref="TestResult"/>.
    /// Derived classes must call this base method and continue with running the test.
    /// </summary>
    /// <param name="testInput"></param>
    /// <param name="testResult">The output object must be created by the caller and passed, so that the caller can get
    ///     a partial object in case of exception.</param>
    /// <param name="testContext"></param>
    /// <param name="state"></param>
    protected virtual async Task RunAsync(
        TestInput testInput,
        TestResult testResult,
        TestContext testContext,
        Dictionary<string, object?> state )
    {
        if ( testInput.Options.InvalidSourceOptions.Count > 0 )
        {
            throw new InvalidOperationException(
                "Invalid option(s) in source code: " +
                string.Join( ", ", testInput.Options.InvalidSourceOptions ) );
        }

        if ( _isTestRunning.Value )
        {
            throw new InvalidOperationException( "A test is already running." );
        }
        else
        {
            _isTestRunning.Value = true;
        }

        if ( testInput.Options.LaunchDebugger == true )
        {
            Debugger.Launch();
        }

        testResult.TestInput = testInput;
        var testDirectory = Path.GetDirectoryName( testInput.FullPath )!;

        // ReSharper disable once RedundantAssignment
        string? dependencyLicenseKey = null;

        if ( testInput.Options.DependencyLicenseFile != null )
        {
            // ReSharper disable once RedundantAssignment
            dependencyLicenseKey = this._fileSystem.ReadAllText( Path.Combine( testInput.SourceDirectory, testInput.Options.DependencyLicenseFile ) );
        }
        else if ( testInput.Options.DependencyLicenseExpression != null )
        {
            // ReSharper disable once RedundantAssignment
            dependencyLicenseKey = TestOptions.ReadLicenseExpression( testInput.Options.DependencyLicenseExpression );
        }

        try
        {
            // Create parse options.
            var preprocessorSymbols = testInput.ProjectProperties.PreprocessorSymbols
                .Add( "TESTRUNNER" )
                .Add( "METALAMA" );

            var defaultParseOptions = SupportedCSharpVersions.DefaultParseOptions;
            var mainParseOptions = defaultParseOptions;

            if ( testInput.Options.LanguageVersion != null )
            {
                mainParseOptions = mainParseOptions.WithLanguageVersion( testInput.Options.LanguageVersion.Value );
            }

            if ( testInput.Options.LanguageFeatures.Count > 0 )
            {
                mainParseOptions = mainParseOptions.WithFeatures( testInput.Options.LanguageFeatures );
            }

            mainParseOptions = mainParseOptions.WithPreprocessorSymbols( preprocessorSymbols.AddRange( testInput.Options.DefinedConstants ) );

            var emptyProject = this.CreateProject( testInput.Options );
            var mainProject = emptyProject.WithParseOptions( mainParseOptions );

            async Task<(Project Project, Document? Document)> AddDocumentAsync(
                Project project,
                CSharpParseOptions parseOptions,
                string fileName,
                string sourceCode,
                bool acceptFileWithoutMember = false )
            {
                // Note that we don't pass the full path to the Document because it causes call stacks of exceptions to have full paths,
                // which is more difficult to test.
                var parsedSyntaxTree = CSharpSyntaxTree.ParseText( sourceCode, parseOptions, fileName, Encoding.UTF8 );

                var prunedSyntaxRoot =
                    testInput.Options.KeepDisabledCode != true
                        ? new RemovePreprocessorDirectivesRewriter( SyntaxKind.PragmaWarningDirectiveTrivia, SyntaxKind.NullableDirectiveTrivia )
                            .Visit( await parsedSyntaxTree.GetRootAsync() )!
                        : await parsedSyntaxTree.GetRootAsync();

                if ( !acceptFileWithoutMember && prunedSyntaxRoot is CompilationUnitSyntax { Members.Count: 0, AttributeLists.Count: 0 } )
                {
                    return (project, null);
                }

                var transformedSyntaxRoot = this.PreprocessSyntaxRoot( prunedSyntaxRoot, state );
                var document = project.AddDocument( fileName, transformedSyntaxRoot, filePath: fileName );

                return (document.Project, document);
            }

            // Add the main document.
            var sourceFileName = testInput.TestName + ".cs";
            (mainProject, var mainDocument) = await AddDocumentAsync( mainProject, mainParseOptions, sourceFileName, testInput.SourceCode );

            if ( mainDocument == null )
            {
                // Skip the test.
                return;
            }

            await testResult.AddInputDocumentAsync( mainDocument, testInput.FullPath );

            if ( !string.IsNullOrEmpty( testInput.FullPath ) )
            {
                (mainProject, _) = await AddDependencyProjectAsync( mainProject, testInput.FullPath );
            }

            // Add additional input documents.

            foreach ( var includedFile in testInput.Options.IncludedFiles.Where( f => !f.EndsWith( ".Dependency.cs", StringComparison.OrdinalIgnoreCase ) ) )
            {
                var includedFullPath = Path.GetFullPath( Path.Combine( testDirectory, includedFile ) );
                var includedText = this._fileSystem.ReadAllText( includedFullPath );

                var includedFileName = Path.GetFileName( includedFullPath );

                (mainProject, var includedDocument) = await AddDocumentAsync( mainProject, mainParseOptions, includedFileName, includedText );

                if ( includedDocument == null )
                {
                    continue;
                }

                (mainProject, _) = await AddDependencyProjectAsync( mainProject, includedFileName );

                await testResult.AddInputDocumentAsync( includedDocument, includedFullPath );
            }

            if ( testInput.Options.SkipAddingSystemFiles != true )
            {
                // Add system files.
                mainProject = await AddPlatformDocuments( mainProject, mainParseOptions );
            }

            mainProject = await AddAdditionalDocuments( mainProject, mainParseOptions );

            // We are done creating the project.

            var initialCompilation = (await mainProject.GetCompilationAsync())!;

            ValidateCustomAttributes( initialCompilation );

            testResult.InputProject = mainProject;
            testResult.InputCompilation = initialCompilation;
            testResult.TestContext = testContext.WithReferences( initialCompilation.References.OfType<PortableExecutableReference>() );

            if ( this.ShouldStopOnInvalidInput( testInput.Options ) )
            {
                var diagnostics = initialCompilation.GetDiagnostics();
                var errors = diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).ToArray();

                if ( errors.Any() )
                {
                    testResult.InputCompilationDiagnostics.Report( errors );
                    testResult.SetFailed( "The initial compilation failed." );
                }
            }

            async Task<Project> AddAdditionalDocuments( Project project, CSharpParseOptions parseOptions )
            {
                if ( this._references.GlobalUsingsFile != null )
                {
                    var path = Path.Combine( testInput.SourceDirectory, this._references.GlobalUsingsFile );

                    if ( this._fileSystem.FileExists( path ) )
                    {
                        var code = this._fileSystem.ReadAllText( path );
                        (project, _) = await AddDocumentAsync( project, parseOptions, "___GlobalUsings.cs", code, true );
                    }
                }

                return project;
            }

#pragma warning disable CS1998

            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once LocalFunctionCanBeMadeStatic
            async Task<Project> AddPlatformDocuments( Project project, CSharpParseOptions parseOptions )
            {
                // ReSharper enable UnusedParameter.Local
                // Add system documents.
#if NETFRAMEWORK
                var (newProject, _) = await AddDocumentAsync(
                    project,
                    parseOptions,
                    "___Platform.cs",
                    "namespace System.Runtime.CompilerServices { internal static class IsExternalInit {}}" );

                return newProject;
#else
                return project;
#endif
            }
#pragma warning restore CS1998

            async Task<(Project Project, ImmutableArray<MetadataReference> References)> AddDependencyProjectAsync( Project baseProject, string basePath = "" )
            {
                var dependencyName = Path.GetFileNameWithoutExtension( basePath ) + ".Dependency.cs";
                var dependencyPath = Path.GetFullPath( Path.Combine( testDirectory, dependencyName ) );

                if ( !this._fileSystem.FileExists( dependencyPath ) )
                {
                    return (baseProject, ImmutableArray<MetadataReference>.Empty);
                }

                // Add documents to the dependency project.
                var includedText = this._fileSystem.ReadAllText( dependencyPath );

                var dependencyParseOptions = defaultParseOptions
                    .WithPreprocessorSymbols( preprocessorSymbols.AddRange( testInput.Options.DependencyDefinedConstants ) );

                if ( testInput.Options.DependencyLanguageVersion != null )
                {
                    dependencyParseOptions = dependencyParseOptions.WithLanguageVersion( testInput.Options.DependencyLanguageVersion.Value );
                }

                var dependencyProject = emptyProject.WithParseOptions( dependencyParseOptions );

                if ( testInput.Options.SkipAddingSystemFiles != true )
                {
                    dependencyProject = await AddPlatformDocuments( dependencyProject, dependencyParseOptions );
                }

                dependencyProject = await AddAdditionalDocuments( dependencyProject, dependencyParseOptions );

                // Add dependencies recursively.
                (dependencyProject, var recursiveReferences) = await AddDependencyProjectAsync( dependencyProject, dependencyName );

                // Compile the dependency.
                var (dependencyReference, _) =
                    await this.CompileDependencyAsync(
                        includedText,
                        dependencyProject,
                        testResult,
                        testContext,
                        dependencyLicenseKey );

                if ( dependencyReference == null )
                {
                    return (baseProject, ImmutableArray<MetadataReference>.Empty);
                }

                var allReferences = recursiveReferences.Add( dependencyReference );

                return (baseProject.AddMetadataReferences( allReferences ), allReferences);
            }
        }
        finally
        {
            _isTestRunning.Value = false;
        }
    }

    /// <summary>
    /// Compiles a dependency using the Metalama pipeline, emits a binary assembly, and returns a reference to it.
    /// </summary>
    private async Task<(MetadataReference? Reference, Project Project)> CompileDependencyAsync(
        string code,
        Project emptyProject,
        TestResult testResult,
        TestContext testContext,
        string? licenseKey = null )
    {
        // The assembly name must match the file name otherwise it wont be found by AssemblyLocator.
        var name = "dependency_" + RandomIdGenerator.GenerateId();
        var project = emptyProject.AddDocument( "dependency.cs", code ).Project;

        var serviceProvider =
            (ProjectServiceProvider) testContext.ServiceProvider.Global.Underlying.WithProjectScopedServices(
                testContext.ProjectOptions,
                this._references.MetadataReferences );

        if ( !string.IsNullOrEmpty( licenseKey ) )
        {
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            serviceProvider = serviceProvider.Underlying.AddProjectLicenseConsumptionManager( licenseKey! );
        }

        // Transform with Metalama.

        var pipeline = new CompileTimeAspectPipeline( serviceProvider, testContext.Domain );

        var compilation = (await project.GetCompilationAsync())!.WithAssemblyName( name );

        var pipelineResult = await pipeline.ExecuteAsync(
            testResult.InputCompilationDiagnostics,
            compilation,
            default );

        if ( !pipelineResult.IsSuccessful )
        {
            testResult.SetFailed( "Transformation of the dependency failed." );

            return default;
        }

        // Emit the binary assembly.
        var testOptions = serviceProvider.GetRequiredService<TestProjectOptions>();
        var outputPath = Path.Combine( testOptions.BaseDirectory, name + ".dll" );

        var emitResult = pipelineResult.Value.ResultingCompilation.Compilation.Emit(
            outputPath,
            manifestResources: pipelineResult.Value.AdditionalResources.Select( r => r.Resource ) );

        if ( !emitResult.Success )
        {
            testResult.InputCompilationDiagnostics.Report( emitResult.Diagnostics );
            testResult.SetFailed( "Compilation of the dependency failed." );

            return default;
        }

        return (MetadataReference.CreateFromFile( outputPath ), project);
    }

    /// <summary>
    /// Processes syntax root of the test file before it is added to the test project.
    /// </summary>
    /// <param name="syntaxRoot"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private protected virtual SyntaxNode PreprocessSyntaxRoot( SyntaxNode syntaxRoot, Dictionary<string, object?> state ) => syntaxRoot;

    private static void ValidateCustomAttributes( Compilation compilation )
    {
        // We want to validate that the custom attributes are properly resolved because unresolved attributes are
        // a frequent source of errors and confusion. In a production execution context, we would get a compilation error, so that would be ok.
        ValidateAttributesVisitor visitor = new( compilation );

        foreach ( var syntaxTree in compilation.SyntaxTrees )
        {
            visitor.Visit( syntaxTree.GetRoot() );
        }
    }

    protected virtual bool CompareTransformedCode => true;

    private protected virtual void SaveResults( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
    {
        if ( this.ProjectDirectory == null )
        {
            throw new InvalidOperationException( "This method cannot be called when the test path is unknown." );
        }

        if ( !this.CompareTransformedCode )
        {
            return;
        }

        var compareWhitespace = testInput.Options.CompareWhitespace ?? false;

        // Compare the "Target" region of the transformed code to the expected output.
        // If the region is not found then compare the complete transformed code.
        var sourceAbsolutePath = testInput.FullPath;

        var expectedTransformedPath = Path.Combine(
            Path.GetDirectoryName( sourceAbsolutePath )!,
            Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + FileExtensions.TransformedCode );

        var testOutputs = testResult.GetTestOutputsWithDiagnostics();
        var actualTransformedNonNormalizedText = JoinSyntaxTrees( testOutputs );
        var actualTransformedSourceTextForComparison = TestOutputNormalizer.NormalizeTestOutput( actualTransformedNonNormalizedText, compareWhitespace, true );
        var actualTransformedSourceTextForStorage = TestOutputNormalizer.NormalizeTestOutput( actualTransformedNonNormalizedText, compareWhitespace, false );

        // If the expectation file does not exist, create it with some placeholder content.
        if ( !this._fileSystem.FileExists( expectedTransformedPath ) )
        {
            // Coverage: ignore

            this._fileSystem.WriteAllText(
                expectedTransformedPath,
                "// TODO: Replace this file with the correct transformed code. See the test output for the actual transformed code." );
        }

        // Read expectations from the file.
        var expectedSourceText = this._fileSystem.ReadAllText( expectedTransformedPath );
        var expectedSourceTextForComparison = TestOutputNormalizer.NormalizeTestOutput( expectedSourceText, compareWhitespace, true );

        // Update the file in obj/transformed if it is different.
        var actualTransformedPath = Path.Combine(
            this.ProjectDirectory,
            "obj",
            "transformed",
            testInput.ProjectProperties.TargetFramework,
            Path.GetDirectoryName( testInput.RelativePath ) ?? "",
            Path.GetFileNameWithoutExtension( testInput.RelativePath ) + FileExtensions.TransformedCode );

        this._fileSystem.CreateDirectory( Path.GetDirectoryName( actualTransformedPath )! );

        var storedTransformedSourceText =
            this._fileSystem.FileExists( actualTransformedPath ) ? this._fileSystem.ReadAllText( actualTransformedPath ) : null;

        // Write the transformed file into the obj directory so that it can be copied by to the test source directory using
        // the `dotnet build /t:AcceptTestOutput` command. We do not override the file if the only difference with the expected file is in
        // ends of lines, because otherwise `dotnet build /t:AcceptTestOutput` command would copy files that differ by EOL only.       
        if ( expectedSourceTextForComparison == actualTransformedSourceTextForComparison )
        {
            if ( TestOutputNormalizer.NormalizeEndOfLines( expectedSourceText )
                 != TestOutputNormalizer.NormalizeEndOfLines( actualTransformedSourceTextForStorage ) )
            {
                // The test output is correct but it must be formatted.
                this._fileSystem.WriteAllText( actualTransformedPath, actualTransformedSourceTextForStorage ?? "" );
            }
            else if ( expectedSourceText != storedTransformedSourceText )
            {
                // Write the exact expected file to the actual file because the only differences are in EOL.
                this._fileSystem.WriteAllText( actualTransformedPath, expectedSourceText );
            }
        }
        else
        {
            this._fileSystem.WriteAllText( actualTransformedPath, actualTransformedSourceTextForStorage ?? "" );
        }

        if ( this.Logger != null )
        {
            var logger = this.Logger!;
            logger.WriteLine( "Expected transformed file: " + expectedTransformedPath );
            logger.WriteLine( "Actual transformed file: " + actualTransformedPath );
            logger.WriteLine( "" );
            logger.WriteLine( "=== ACTUAL TRANSFORMED CODE ===" );
            logger.WriteLine( actualTransformedSourceTextForStorage );
            logger.WriteLine( "=====================" );

            // Write all diagnostics to the logger.
            foreach ( var diagnostic in testResult.Diagnostics )
            {
                logger.WriteLine( diagnostic.ToString() );
            }
        }

        state["expectedTransformedSourceText"] = expectedSourceTextForComparison;
        state["actualTransformedNormalizedSourceText"] = actualTransformedSourceTextForComparison;
        state["actualTransformedSourceTextForStorage"] = actualTransformedSourceTextForStorage;

        static string JoinSyntaxTrees( IReadOnlyList<SyntaxTree> compilationUnits )
        {
            switch ( compilationUnits.Count )
            {
                case 0:
                    return "// --- No output compilation units ---";

                case 1:
                    return compilationUnits[0].GetRoot().ToFullString();

                default:
                    var sb = new StringBuilder();

                    foreach ( var syntaxTree in compilationUnits )
                    {
                        sb.AppendLine();
                        sb.AppendLineInvariant( $"// --- {syntaxTree.FilePath} ---" );
                        sb.AppendLine();
                        sb.AppendLine( syntaxTree.GetRoot().ToFullString() );
                    }

                    return sb.ToString();
            }
        }
    }

    protected virtual void ExecuteAssertions( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
    {
        if ( !this.CompareTransformedCode )
        {
            return;
        }

        var expectedTransformedSourceText = (string) state["expectedTransformedSourceText"]!;
        var actualTransformedNormalizedSourceText = (string) state["actualTransformedNormalizedSourceText"]!;

        Assert.Equal( expectedTransformedSourceText, actualTransformedNormalizedSourceText );
    }

    private protected virtual bool ShouldStopOnInvalidInput( TestOptions testOptions ) => !testOptions.AcceptInvalidInput.GetValueOrDefault( true );

    /// <summary>
    /// Creates a new project that is used to compile the test source.
    /// </summary>
    /// <returns>A new project instance.</returns>
    [PublicAPI]
    public Project CreateProject( TestOptions options )
    {
        var compilation = TestCompilationFactory.CreateEmptyCSharpCompilation(
            null,
            this._references.MetadataReferences,
            options.OutputAssemblyType switch
            {
                "Exe" => OutputKind.ConsoleApplication,
                _ => OutputKind.DynamicallyLinkedLibrary
            },
            nullableContextOptions: options.NullabilityDisabled == true ? NullableContextOptions.Disable : NullableContextOptions.Enable );

        const string projectName = "test";

        var workspace1 = new AdhocWorkspace();
        var solution = workspace1.CurrentSolution;

        var project = solution.AddProject( projectName, projectName, LanguageNames.CSharp )
            .WithCompilationOptions( compilation.Options )
            .AddMetadataReferences( compilation.References );

        // Don't add the assembly containing the code to test because it would result in duplicate symbols.

        return project;
    }

    private protected async Task WriteHtmlAsync( TestInput testInput, TestResult testResult )
    {
        var serviceProvider = testResult.TestContext.AssertNotNull().ServiceProvider;
        var htmlCodeWriter = this.CreateHtmlCodeWriter( serviceProvider, testInput.Options );

        var htmlDirectory = Path.Combine(
            this.ProjectDirectory!,
            "obj",
            "html",
            testInput.ProjectProperties.TargetFramework,
            Path.GetDirectoryName( testInput.RelativePath ) ?? "" );

        if ( !this._fileSystem.DirectoryExists( htmlDirectory ) )
        {
            this._fileSystem.CreateDirectory( htmlDirectory );
        }

        // Write each document individually.
        if ( testInput.Options.WriteInputHtml.GetValueOrDefault() || testInput.Options.WriteOutputHtml.GetValueOrDefault() )
        {
            var pipeline = new TestDesignTimeAspectPipeline( serviceProvider, testResult.TestContext.AssertNotNull().Domain );
            var inputCompilation = testResult.InputCompilation.AssertNotNull();
            var designTimePipelineResult = await pipeline.ExecuteAsync( inputCompilation );

            var compilationWithDesignTimeTrees =
                inputCompilation
                    .AddSyntaxTrees( designTimePipelineResult.AdditionalSyntaxTrees.Select( x => x.GeneratedSyntaxTree ) );

            foreach ( var syntaxTree in testResult.SyntaxTrees )
            {
                var isTargetCode = Path.GetFileName( syntaxTree.InputPath )!.Count( c => c == '.' ) == 1;

                await this.WriteHtmlAsync(
                    compilationWithDesignTimeTrees,
                    testResult,
                    syntaxTree,
                    htmlDirectory,
                    htmlCodeWriter,
                    isTargetCode,
                    designTimePipelineResult.Suppressions );
            }
        }
    }

    private HtmlCodeWriter CreateHtmlCodeWriter( in ProjectServiceProvider serviceProvider, TestOptions options )
        => new( serviceProvider, this.GetHtmlCodeWriterOptions( options ) );

    protected virtual HtmlCodeWriterOptions GetHtmlCodeWriterOptions( TestOptions options ) => new( options.AddHtmlTitles.GetValueOrDefault() );

    private async Task WriteHtmlAsync(
        Compilation compilationWithDesignTimeTrees,
        TestResult testResult,
        TestSyntaxTree testSyntaxTree,
        string htmlDirectory,
        HtmlCodeWriter htmlCodeWriter,
        bool writeDiff,
        ImmutableArray<ScopedSuppression> suppressions )
    {
        StreamWriter? inputTextWriter = null;
        StreamWriter? outputTextWriter = null;
        List<Diagnostic>? inputDiagnostics = null;

        if ( testResult.TestInput!.Options.WriteInputHtml == true )
        {
            testSyntaxTree.HtmlInputPath = Path.Combine(
                htmlDirectory,
                Path.GetFileNameWithoutExtension( testSyntaxTree.InputDocument.FilePath ) + FileExtensions.InputHtml );

            this.Logger?.WriteLine( "HTML of input: " + testSyntaxTree.HtmlInputPath );

            inputTextWriter = new StreamWriter( this._fileSystem.Open( testSyntaxTree.HtmlInputPath, FileMode.Create ) );

            // Add diagnostics to the input tree.
            inputDiagnostics = new List<Diagnostic>();
            inputDiagnostics.AddRange( testResult.Diagnostics.Where( d => d.Location.SourceTree?.FilePath == testSyntaxTree.InputSyntaxTree.FilePath ) );
            var semanticModel = compilationWithDesignTimeTrees.AssertNotNull().GetSemanticModel( testSyntaxTree.InputSyntaxTree );

            foreach ( var diagnostic in semanticModel.GetDiagnostics().Where( d => !testResult.TestInput.ShouldIgnoreDiagnostic( d.Id ) ) )
            {
                // Check if any suppression applies to this diagnostic.
                if ( suppressions.Any(
                        s => s.Definition.SuppressedDiagnosticId == diagnostic.Id
                             && s.GetScopeSymbolOrNull( compilationWithDesignTimeTrees )
                                 ?.DeclaringSyntaxReferences.Any(
                                     r =>
                                         r.SyntaxTree == diagnostic.Location.SourceTree &&
                                         r.Span.Contains( diagnostic.Location.SourceSpan ) ) == true ) )
                {
                    return;
                }

                // Add the diagnostic.
                inputDiagnostics.Add( diagnostic );
            }
        }

        if ( testResult.TestInput.Options.WriteOutputHtml == true && testResult.OutputProject != null )
        {
            testSyntaxTree.HtmlOutputPath = Path.Combine(
                htmlDirectory,
                Path.GetFileNameWithoutExtension( testSyntaxTree.InputDocument.FilePath ) + FileExtensions.TransformedHtml );

            this.Logger?.WriteLine( "HTML of output: " + testSyntaxTree.HtmlOutputPath );

            outputTextWriter = new StreamWriter( this._fileSystem.Open( testSyntaxTree.HtmlOutputPath, FileMode.Create ) );
        }

        if ( writeDiff && inputTextWriter != null && outputTextWriter != null )
        {
            using ( inputTextWriter.IgnoreAsyncDisposable() )
            using ( outputTextWriter.IgnoreAsyncDisposable() )
            {
                await htmlCodeWriter.WriteDiffAsync(
                    testSyntaxTree.InputDocument,
                    testSyntaxTree.OutputDocument.AssertNotNull(),
                    inputTextWriter,
                    outputTextWriter,
                    inputDiagnostics! );
            }
        }
        else
        {
            if ( inputTextWriter != null )
            {
                using ( inputTextWriter.IgnoreAsyncDisposable() )
                {
                    await htmlCodeWriter.WriteAsync(
                        testSyntaxTree.InputDocument,
                        inputTextWriter,
                        inputDiagnostics! );
                }
            }

            if ( outputTextWriter != null )
            {
                using ( outputTextWriter.IgnoreAsyncDisposable() )
                {
                    await htmlCodeWriter.WriteAsync(
                        testSyntaxTree.OutputDocument.AssertNotNull(),
                        outputTextWriter );
                }
            }
        }
    }

    protected bool VerifyBinaryStream( TestInput testInput, TestResult testResult, MemoryStream stream )
    {
        if ( testInput.Options.AllowCompileTimeDynamicCode.GetValueOrDefault() )
        {
            return true;
        }

        stream.Seek( 0, SeekOrigin.Begin );
        using var peReader = new PEReader( stream, PEStreamOptions.LeaveOpen );
        var metadataReader = peReader.GetMetadataReader();

        foreach ( var typeRefHandle in metadataReader.TypeReferences )
        {
            var typeRef = metadataReader.GetTypeReference( typeRefHandle );
            var ns = metadataReader.GetString( typeRef.Namespace );
            var typeName = metadataReader.GetString( typeRef.Name );

            if ( ns.ContainsOrdinal( "Microsoft.CSharp.RuntimeBinder" ) &&
                 string.Equals( typeName, "CSharpArgumentInfo", StringComparison.Ordinal ) )
            {
                var directory = Path.Combine( MetalamaPathUtilities.GetTempPath(), "Metalama", "InvalidAssemblies" );

                if ( !this._fileSystem.DirectoryExists( directory ) )
                {
                    this._fileSystem.CreateDirectory( directory );
                }

                var diagnosticFile = Path.Combine( directory, RandomIdGenerator.GenerateId() + ".dll" );

                using ( var diagnosticStream = this._fileSystem.Open( diagnosticFile, FileMode.Create ) )
                {
                    stream.Seek( 0, SeekOrigin.Begin );
                    stream.CopyTo( diagnosticStream );
                }

                this.Logger?.WriteLine( "Compiled compile-time assembly: " + diagnosticFile );

                testResult.SetFailed( "The compiled assembly contains dynamic code." );

                return false;
            }
        }

        stream.Seek( 0, SeekOrigin.Begin );

        return true;
    }
}