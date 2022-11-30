// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Testing.Api;
using Metalama.Testing.Api.Options;
using Metalama.Testing.Framework.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Document = Microsoft.CodeAnalysis.Document;

namespace Metalama.Testing.Framework;

/// <summary>
/// An abstract class for all template-base tests.
/// </summary>
public abstract partial class BaseTestRunner
{
    private static readonly Regex _spaceRegex = new( "\\s+", RegexOptions.Compiled );
    private static readonly Regex _newLineRegex = new( "(\\s*(\r\n|\r|\n)+)", RegexOptions.Compiled | RegexOptions.Multiline );
    private static readonly AsyncLocal<bool> _isTestRunning = new();

    private static readonly RemovePreprocessorDirectivesRewriter _removePreprocessorDirectivesRewriter =
        new( SyntaxKind.PragmaWarningDirectiveTrivia, SyntaxKind.NullableDirectiveTrivia );

    public GlobalServiceProvider BaseServiceProvider { get; }

    public TestProjectReferences References { get; }

    protected BaseTestRunner(
        GlobalServiceProvider serviceProvider,
        string? projectDirectory,
        TestProjectReferences references,
        ITestOutputHelper? logger )
    {
        this.References = references;
        this.BaseServiceProvider = serviceProvider;
        this.ProjectDirectory = projectDirectory;
        this.Logger = logger;
    }

    /// <summary>
    /// Gets the project directory, or <c>null</c> if it is unknown.
    /// </summary>
    public string? ProjectDirectory { get; }

    public ITestOutputHelper? Logger { get; }

    public async Task RunAndAssertAsync( TestInput testInput, TestProjectOptions projectOptions )
    {
        using ( TestExecutionContext.Open() )
        {
            try
            {
                await this.RunAndAssertCoreAsync( testInput, projectOptions );
            }
            finally
            {
                // This is a trick to make the current task, on the heap, stop having a reference to the previous
                // task. This allows TestExecutionContext.Dispose to perform a full GC. Without Task.Yield, we will
                // have references to the objects that are in the scope of the test.
                await Task.Yield();
            }
        }
    }

    private async Task RunAndAssertCoreAsync( TestInput testInput, TestProjectOptions projectOptions )
    {
        try
        {
            testInput.ProjectProperties.License?.ThrowIfNotLicensed();

            var transformedOptions = this.GetProjectOptions( projectOptions );

            Dictionary<string, object?> state = new( StringComparer.Ordinal );
            using var testResult = new TestResult();
            await this.RunAsync( testInput, testResult, transformedOptions, state );
            this.SaveResults( testInput, testResult, state );
            this.ExecuteAssertions( testInput, testResult, state );
        }
        catch ( Exception e ) when ( e.GetType().FullName == testInput.Options.ExpectedException )
        {
            return;
        }

        if ( testInput.Options.ExpectedException != null )
        {
            throw new AssertActualExpectedException( testInput.Options.ExpectedException, null, "Expected exception has not been thrown." );
        }
    }

    public Task RunAsync( TestInput testInput, TestResult testResult, TestProjectOptions projectOptions )
        => this.RunAsync(
            testInput,
            testResult,
            projectOptions,
            new Dictionary<string, object?>( StringComparer.InvariantCulture ) );

    protected virtual IProjectOptions GetProjectOptions( TestProjectOptions options ) => options;

    /// <summary>
    /// Runs a test. The present implementation of this method only prepares an input project and stores it in the <see cref="TestResult"/>.
    /// Derived classes must call this base method and continue with running the test.
    /// </summary>
    /// <param name="testInput"></param>
    /// <param name="testResult">The output object must be created by the caller and passed, so that the caller can get
    ///     a partial object in case of exception.</param>
    /// <param name="projectOptions"></param>
    /// <param name="state"></param>
    protected virtual async Task RunAsync(
        TestInput testInput,
        TestResult testResult,
        IProjectOptions projectOptions,
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

        testResult.TestInput = testInput;

        try
        {
            // Create parse options.
            var preprocessorSymbols = testInput.ProjectProperties.PreprocessorSymbols
                .Add( "TESTRUNNER" )
                .Add( "METALAMA" );

            var defaultParseOptions = SupportedCSharpVersions.DefaultParseOptions;

            if ( testInput.Options.LanguageVersion != null )
            {
                defaultParseOptions = defaultParseOptions.WithLanguageVersion( testInput.Options.LanguageVersion.Value );
            }

            if ( testInput.Options.LanguageFeatures.Count > 0 )
            {
                defaultParseOptions = defaultParseOptions.WithFeatures( testInput.Options.LanguageFeatures );
            }

            var emptyProject = this.CreateProject( testInput.Options );
            var parseOptions = defaultParseOptions.WithPreprocessorSymbols( preprocessorSymbols.AddRange( testInput.Options.DefinedConstants ) );
            var project = emptyProject.WithParseOptions( parseOptions );

            async Task<Document?> AddDocumentAsync( string fileName, string sourceCode, bool acceptFileWithoutMember = false )
            {
                // Note that we don't pass the full path to the Document because it causes call stacks of exceptions to have full paths,
                // which is more difficult to test.
                var parsedSyntaxTree = CSharpSyntaxTree.ParseText( sourceCode, parseOptions, fileName, Encoding.UTF8 );

                var prunedSyntaxRoot =
                    testInput.Options.KeepDisabledCode != true
                        ? _removePreprocessorDirectivesRewriter.Visit( await parsedSyntaxTree.GetRootAsync() )!
                        : await parsedSyntaxTree.GetRootAsync();

                if ( !acceptFileWithoutMember && prunedSyntaxRoot is CompilationUnitSyntax { Members: { Count: 0 } } )
                {
                    return null;
                }

                var transformedSyntaxRoot = this.PreprocessSyntaxRoot( testInput, prunedSyntaxRoot, state );
                var document = project.AddDocument( fileName, transformedSyntaxRoot, filePath: fileName );
                project = document.Project;

                return document;
            }

            // Add the main document.
            var sourceFileName = testInput.TestName + ".cs";
            var mainDocument = await AddDocumentAsync( sourceFileName, testInput.SourceCode );

            if ( mainDocument == null )
            {
                // Skip the test.
                return;
            }

            var syntaxTree = (await mainDocument.GetSyntaxTreeAsync())!;

            testResult.AddInputDocument( mainDocument, testInput.FullPath );

            var initialCompilation = CSharpCompilation.Create(
                project.Name,
                new[] { syntaxTree },
                project.MetadataReferences,
                (CSharpCompilationOptions?) project.CompilationOptions );

            string? dependencyLicenseKey = null;

            if ( testInput.Options.DependencyLicenseFile != null )
            {
                dependencyLicenseKey = File.ReadAllText( Path.Combine( testInput.ProjectDirectory, testInput.Options.DependencyLicenseFile ) );
            }

            // Add additional test documents.
            foreach ( var includedFile in testInput.Options.IncludedFiles )
            {
                var includedFullPath = Path.GetFullPath( Path.Combine( Path.GetDirectoryName( testInput.FullPath )!, includedFile ) );
                var includedText = File.ReadAllText( includedFullPath );

                if ( !includedFile.EndsWith( ".Dependency.cs", StringComparison.OrdinalIgnoreCase ) )
                {
                    var includedFileName = Path.GetFileName( includedFullPath );

                    var includedDocument = await AddDocumentAsync( includedFileName, includedText );

                    if ( includedDocument == null )
                    {
                        continue;
                    }

                    testResult.AddInputDocument( includedDocument, includedFullPath );

                    var includedSyntaxTree = (await includedDocument.GetSyntaxTreeAsync())!;
                    initialCompilation = initialCompilation.AddSyntaxTrees( includedSyntaxTree );
                }
                else
                {
                    // Dependencies must be compiled separately using Metalama.
                    var dependencyParseOptions = defaultParseOptions.WithPreprocessorSymbols(
                        preprocessorSymbols.AddRange( testInput.Options.DependencyDefinedConstants ) );

                    var dependencyProject = emptyProject.WithParseOptions( dependencyParseOptions );
                    var dependency = await this.CompileDependencyAsync( includedText, dependencyProject, testResult, projectOptions, dependencyLicenseKey );

                    if ( dependency == null )
                    {
                        return;
                    }

                    initialCompilation = initialCompilation.AddReferences( dependency );
                }
            }

            // Add system documents.
#if NETFRAMEWORK
            var platformDocument = await AddDocumentAsync(
                "___Platform.cs",
                "namespace System.Runtime.CompilerServices { internal static class IsExternalInit {}}" );

            initialCompilation = initialCompilation.AddSyntaxTrees( (await platformDocument!.GetSyntaxTreeAsync())! );
#endif

            if ( this.References.GlobalUsingsFile != null )
            {
                var path = Path.Combine( this.ProjectDirectory!, this.References.GlobalUsingsFile );

                if ( File.Exists( path ) )
                {
                    var code = File.ReadAllText( path );
                    var globalUsingsDocument = await AddDocumentAsync( "___GlobalUsings.cs", code, true );

                    initialCompilation = initialCompilation.AddSyntaxTrees( (await globalUsingsDocument!.GetSyntaxTreeAsync())! );
                }
            }

            ValidateCustomAttributes( initialCompilation );

            testResult.InputProject = project;
            testResult.InputCompilation = initialCompilation;
            testResult.ProjectScopedServiceProvider = this.BaseServiceProvider.Underlying.WithProjectScopedServices( projectOptions, initialCompilation );

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
        }
        finally
        {
            _isTestRunning.Value = false;
        }
    }

    /// <summary>
    /// Compiles a dependency using the Metalama pipeline, emits a binary assembly, and returns a reference to it.
    /// </summary>
    private async Task<MetadataReference?> CompileDependencyAsync(
        string code,
        Project emptyProject,
        TestResult testResult,
        IProjectOptions projectOptions,
        string? licenseKey = null )
    {
        // The assembly name must match the file name otherwise it wont be found by AssemblyLocator.
        var name = "dependency_" + RandomIdGenerator.GenerateId();
        var project = emptyProject.AddDocument( "dependency.cs", code ).Project;

        using var domain = new UnloadableCompileTimeDomain();

        var serviceProvider =
            (ProjectServiceProvider) this.BaseServiceProvider.Underlying.WithProjectScopedServices( projectOptions, this.References.MetadataReferences );

        if ( !string.IsNullOrEmpty( licenseKey ) )
        {
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            serviceProvider = serviceProvider.Underlying.AddLicenseConsumptionManagerForLicenseKey( licenseKey! );
        }

        // Transform with Metalama.

        var pipeline = new CompileTimeAspectPipeline(
            serviceProvider,
            domain );

        var compilation = (await project.GetCompilationAsync())!.WithAssemblyName( name );

        var pipelineResult = await pipeline.ExecuteAsync(
            testResult.InputCompilationDiagnostics,
            compilation,
            default );

        if ( !pipelineResult.IsSuccessful )
        {
            testResult.SetFailed( "Transformation of the dependency failed." );

            return null;
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

            return null;
        }

        return MetadataReference.CreateFromFile( outputPath );
    }

    /// <summary>
    /// Processes syntax root of the test file before it is added to the test project.
    /// </summary>
    /// <param name="testInput"></param>
    /// <param name="syntaxRoot"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private protected virtual SyntaxNode PreprocessSyntaxRoot( TestInput testInput, SyntaxNode syntaxRoot, Dictionary<string, object?> state ) => syntaxRoot;

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

    protected static string NormalizeEndOfLines( string? s, bool replaceWithSpace = false )
        => string.IsNullOrWhiteSpace( s ) ? "" : _newLineRegex.Replace( s, replaceWithSpace ? " " : Environment.NewLine ).Trim();

    public static string? NormalizeTestOutput( string? s, bool preserveFormatting, bool forComparison )
        => s == null ? null : NormalizeTestOutput( CSharpSyntaxTree.ParseText( s ).GetRoot(), preserveFormatting, forComparison );

    private static string? NormalizeTestOutput( SyntaxNode syntaxNode, bool preserveFormatting, bool forComparison )
    {
        if ( preserveFormatting )
        {
            return NormalizeEndOfLines( syntaxNode.ToFullString() );
        }
        else
        {
            var s = syntaxNode.NormalizeWhitespace( "  ", "\n" ).ToFullString();

            s = NormalizeEndOfLines( s, forComparison );

            if ( forComparison )
            {
                s = _spaceRegex.Replace( s, " " );
            }

            return s;
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

        var preserveWhitespace = testInput.Options.PreserveWhitespace ?? false;

        // Compare the "Target" region of the transformed code to the expected output.
        // If the region is not found then compare the complete transformed code.
        var sourceAbsolutePath = testInput.FullPath;

        var expectedTransformedPath = Path.Combine(
            Path.GetDirectoryName( sourceAbsolutePath )!,
            Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + FileExtensions.TransformedCode );

        var testOutputs = testResult.GetTestOutputsWithDiagnostics();
        var actualTransformedNonNormalizedText = JoinSyntaxTrees( testOutputs );
        var actualTransformedSourceTextForComparison = NormalizeTestOutput( actualTransformedNonNormalizedText, preserveWhitespace, true );
        var actualTransformedSourceTextForStorage = NormalizeTestOutput( actualTransformedNonNormalizedText, preserveWhitespace, false );

        // If the expectation file does not exist, create it with some placeholder content.
        if ( !File.Exists( expectedTransformedPath ) )
        {
            // Coverage: ignore

            File.WriteAllText(
                expectedTransformedPath,
                "// TODO: Replace this file with the correct transformed code. See the test output for the actual transformed code." );
        }

        // Read expectations from the file.
        var expectedSourceText = File.ReadAllText( expectedTransformedPath );
        var expectedSourceTextForComparison = NormalizeTestOutput( expectedSourceText, preserveWhitespace, true );

        // Update the file in obj/transformed if it is different.
        var actualTransformedPath = Path.Combine(
            this.ProjectDirectory,
            "obj",
            "transformed",
            testInput.ProjectProperties.TargetFramework,
            Path.GetDirectoryName( testInput.RelativePath ) ?? "",
            Path.GetFileNameWithoutExtension( testInput.RelativePath ) + FileExtensions.TransformedCode );

        Directory.CreateDirectory( Path.GetDirectoryName( actualTransformedPath )! );

        var storedTransformedSourceText =
            File.Exists( actualTransformedPath ) ? File.ReadAllText( actualTransformedPath ) : null;

        // Write the transformed file into the obj directory so that it can be copied by to the test source directory using
        // the `dotnet build /t:AcceptTestOutput` command. We do not override the file if the only difference with the expected file is in
        // ends of lines, because otherwise `dotnet build /t:AcceptTestOutput` command would copy files that differ by EOL only.       
        if ( expectedSourceTextForComparison == actualTransformedSourceTextForComparison )
        {
            if ( NormalizeEndOfLines( expectedSourceText ) != NormalizeEndOfLines( actualTransformedSourceTextForStorage ) )
            {
                // The test output is correct but it must be formatted.
                File.WriteAllText( actualTransformedPath, actualTransformedSourceTextForStorage );
            }
            else if ( expectedSourceText != storedTransformedSourceText )
            {
                // Write the exact expected file to the actual file because the only differences are in EOL.
                File.WriteAllText( actualTransformedPath, expectedSourceText );
            }
        }
        else
        {
            File.WriteAllText( actualTransformedPath, actualTransformedSourceTextForStorage );
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
    internal Project CreateProject( TestOptions options )
    {
        var compilation = TestCompilationFactory.CreateEmptyCSharpCompilation(
            null,
            this.References.MetadataReferences,
            options.OutputAssemblyType switch
            {
                "Exe" => OutputKind.ConsoleApplication,
                _ => OutputKind.DynamicallyLinkedLibrary
            },
            nullableContextOptions: options.NullabilityDisabled == true ? NullableContextOptions.Disable : NullableContextOptions.Enable );

        var projectName = "test";

        var workspace1 = WorkspaceHelper.CreateWorkspace();
        var solution = workspace1.CurrentSolution;

        var project = solution.AddProject( projectName, projectName, LanguageNames.CSharp )
            .WithCompilationOptions( compilation.Options )
            .AddMetadataReferences( compilation.References );

        // Don't add the assembly containing the code to test because it would result in duplicate symbols.

        return project;
    }

    private protected async Task WriteHtmlAsync( TestInput testInput, TestResult testResult )
    {
        var htmlCodeWriter = this.CreateHtmlCodeWriter( testResult.ProjectScopedServiceProvider, testInput.Options );

        var htmlDirectory = Path.Combine(
            this.ProjectDirectory!,
            "obj",
            "html",
            testInput.ProjectProperties.TargetFramework,
            Path.GetDirectoryName( testInput.RelativePath ) ?? "" );

        if ( !Directory.Exists( htmlDirectory ) )
        {
            Directory.CreateDirectory( htmlDirectory );
        }

        // Write each document individually.
        if ( testInput.Options.WriteInputHtml.GetValueOrDefault() )
        {
            foreach ( var syntaxTree in testResult.SyntaxTrees )
            {
                await this.WriteHtmlAsync( syntaxTree, htmlDirectory, htmlCodeWriter );
            }
        }

        // Write the consolidated output.
        if ( testInput.Options.WriteOutputHtml.GetValueOrDefault() )
        {
            // Multi file tests are not supported for html output.
            var output = testResult.GetTestOutputsWithDiagnostics().Single().GetRoot();
            var outputDocument = testResult.InputProject!.AddDocument( "Consolidated.cs", output );

            var formattedOutput = await OutputCodeFormatter.FormatAsync( outputDocument );
            var outputHtmlPath = Path.Combine( htmlDirectory, testInput.TestName + FileExtensions.OutputHtml );
            var formattedOutputDocument = testResult.InputProject.AddDocument( "ConsolidatedFormatted.cs", formattedOutput.Syntax );

            var outputHtml = File.CreateText( outputHtmlPath );

            using ( outputHtml.IgnoreAsyncDisposable() )
            {
                await htmlCodeWriter.WriteAsync( formattedOutputDocument, outputHtml );
            }

            testResult.OutputHtmlPath = outputHtmlPath;
        }
    }

    protected virtual HtmlCodeWriter CreateHtmlCodeWriter( ProjectServiceProvider serviceProvider, TestOptions options )
        => new( serviceProvider, new HtmlCodeWriterOptions( options.AddHtmlTitles.GetValueOrDefault() ) );

    private async Task WriteHtmlAsync( TestSyntaxTree testSyntaxTree, string htmlDirectory, HtmlCodeWriter htmlCodeWriter )
    {
        var inputHtmlPath = Path.Combine(
            htmlDirectory,
            Path.GetFileNameWithoutExtension( testSyntaxTree.InputDocument.FilePath ) + FileExtensions.InputHtml );

        testSyntaxTree.HtmlInputRunTimePath = inputHtmlPath;

        this.Logger?.WriteLine( "HTML of input: " + inputHtmlPath );

        // Write the input document.
        var inputTextWriter = File.CreateText( inputHtmlPath );

        using ( inputTextWriter.IgnoreAsyncDisposable() )
        {
            await htmlCodeWriter.WriteAsync(
                testSyntaxTree.InputDocument,
                inputTextWriter );
        }

        // We have no use case to write the output document because all cases use the consolidated output document instead.
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
                var directory = Path.Combine( Path.GetTempPath(), "Metalama", "InvalidAssemblies" );

                if ( !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );
                }

                var diagnosticFile = Path.Combine( directory, RandomIdGenerator.GenerateId() + ".dll" );

                using ( var diagnosticStream = File.Create( diagnosticFile ) )
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