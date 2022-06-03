// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Metalama.TestFramework.Utilities;
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
using Document = Microsoft.CodeAnalysis.Document;

namespace Metalama.TestFramework
{
    /// <summary>
    /// An abstract class for all template-base tests.
    /// </summary>
    public abstract partial class BaseTestRunner
    {
        private static readonly Regex _spaceRegex = new( " +", RegexOptions.Compiled );
        private static readonly Regex _newLineRegex = new( "( *[\n|\r])+", RegexOptions.Compiled );
        private static readonly AsyncLocal<bool> _isTestRunning = new();

        public ServiceProvider BaseServiceProvider { get; }

        public TestProjectReferences References { get; }

        protected BaseTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger )
        {
            this.References = references;

            this.BaseServiceProvider = serviceProvider.WithMark( ServiceProviderMark.Test );
            this.ProjectDirectory = projectDirectory;
            this.Logger = logger;
        }

        /// <summary>
        /// Gets the project directory, or <c>null</c> if it is unknown.
        /// </summary>
        public string? ProjectDirectory { get; }

        public ITestOutputHelper? Logger { get; }

        public async Task RunAndAssertAsync( TestInput testInput )
        {
            using ( TestExecutionContext.Open() )
            {
                try
                {
                    await this.RunAndAssertCoreAsync( testInput );
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

        private async Task RunAndAssertCoreAsync( TestInput testInput )
        {
            Dictionary<string, object?> state = new( StringComparer.Ordinal );
            using var testResult = new TestResult();
            await this.RunAsync( testInput, testResult, state );
            this.SaveResults( testInput, testResult, state );
            this.ExecuteAssertions( testInput, testResult, state );
        }

        public Task RunAsync( TestInput testInput, TestResult testResult )
            => this.RunAsync(
                testInput,
                testResult,
                new Dictionary<string, object?>( StringComparer.InvariantCulture ) );

        /// <summary>
        /// Runs a test. The present implementation of this method only prepares an input project and stores it in the <see cref="TestResult"/>.
        /// Derived classes must call this base method and continue with running the test.
        /// </summary>
        /// <param name="testInput"></param>
        /// <param name="testResult">The output object must be created by the caller and passed, so that the caller can get
        ///     a partial object in case of exception.</param>
        /// <param name="state"></param>
        protected virtual async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
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

                var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols( preprocessorSymbols );

                var emptyProject = this.CreateProject( testInput.Options ).WithParseOptions( parseOptions );
                var project = emptyProject;

                async Task<Document?> AddDocumentAsync( string fileName, string sourceCode, bool acceptFileWithoutMember = false )
                {
                    // Note that we don't pass the full path to the Document because it causes call stacks of exceptions to have full paths,
                    // which is more difficult to test.
                    var parsedSyntaxTree = CSharpSyntaxTree.ParseText( sourceCode, parseOptions, fileName, Encoding.UTF8 );
                    var prunedSyntaxRoot = new InactiveCodeRemover().Visit( await parsedSyntaxTree.GetRootAsync() );

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
                        var dependency = await this.CompileDependencyAsync( includedText, emptyProject, testResult );

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
                testResult.ProjectScopedServiceProvider = this.BaseServiceProvider.WithProjectScopedServices( initialCompilation );

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
        private async Task<MetadataReference?> CompileDependencyAsync( string code, Project emptyProject, TestResult testResult )
        {
            // The assembly name must match the file name otherwise it wont be found bu AssemblyLocator.
            var name = "dependency_" + RandomIdGenerator.GenerateId();
            var project = emptyProject.AddDocument( "dependency.cs", code ).Project;

            using var domain = new UnloadableCompileTimeDomain();

            // Transform with Metalama.
            var pipeline = new CompileTimeAspectPipeline(
                this.BaseServiceProvider.WithProjectScopedServices( this.References.MetadataReferences ),
                true,
                domain );

            var compilation = (await project.GetCompilationAsync())!.WithAssemblyName( name );

            var pipelineResult = await pipeline.ExecuteAsync(
                testResult.InputCompilationDiagnostics,
                compilation,
                default,
                CancellationToken.None );

            if ( pipelineResult == null )
            {
                testResult.SetFailed( "Transformation of the dependency failed." );

                return null;
            }

            // Emit the binary assembly.
            var testOptions = this.BaseServiceProvider.GetRequiredService<TestProjectOptions>();
            var outputPath = Path.Combine( testOptions.BaseDirectory, name + ".dll" );

            var emitResult = pipelineResult.ResultingCompilation.Compilation.Emit(
                outputPath,
                manifestResources: pipelineResult.AdditionalResources.Select( r => r.Resource ) );

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
        private protected virtual SyntaxNode PreprocessSyntaxRoot( TestInput testInput, SyntaxNode syntaxRoot, Dictionary<string, object?> state )
            => syntaxRoot;

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

        protected static string NormalizeEndOfLines( string? s ) => string.IsNullOrWhiteSpace( s ) ? "" : _newLineRegex.Replace( s, "\n" ).Trim();

        public static string? NormalizeTestOutput( string? s, bool preserveFormatting )
            => s == null ? null : NormalizeTestOutput( CSharpSyntaxTree.ParseText( s ).GetRoot(), preserveFormatting );

        private static string? NormalizeTestOutput( SyntaxNode syntaxNode, bool preserveFormatting )
        {
            if ( preserveFormatting )
            {
                return syntaxNode.ToFullString().ReplaceOrdinal( "\r\n", "\n" );
            }
            else
            {
                var s = syntaxNode.NormalizeWhitespace().ToFullString();

                s = NormalizeEndOfLines( s );
                s = _spaceRegex.Replace( s, " " );

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

            var formatCode = testInput.Options.FormatOutput.GetValueOrDefault( true );

            // Compare the "Target" region of the transformed code to the expected output.
            // If the region is not found then compare the complete transformed code.
            var sourceAbsolutePath = testInput.FullPath;

            var expectedTransformedPath = Path.Combine(
                Path.GetDirectoryName( sourceAbsolutePath )!,
                Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + FileExtensions.TransformedCode );

            var testOutputs = testResult.GetTestOutputsWithDiagnostics();
            var actualTransformedNonNormalizedText = JoinSyntaxTrees( testOutputs );
            var actualTransformedNormalizedSourceText = NormalizeTestOutput( actualTransformedNonNormalizedText, formatCode );

            // If the expectation file does not exist, create it with some placeholder content.
            if ( !File.Exists( expectedTransformedPath ) )
            {
                // Coverage: ignore

                File.WriteAllText(
                    expectedTransformedPath,
                    "// TODO: Replace this file with the correct transformed code. See the test output for the actual transformed code." );
            }

            // Read expectations from the file.
            var expectedNonNormalizedSourceText = File.ReadAllText( expectedTransformedPath );
            var expectedTransformedSourceText = NormalizeTestOutput( expectedNonNormalizedSourceText, formatCode );

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
                File.Exists( actualTransformedPath ) ? NormalizeTestOutput( File.ReadAllText( actualTransformedPath ), formatCode ) : null;

            if ( expectedTransformedSourceText == actualTransformedNormalizedSourceText
                 && storedTransformedSourceText != expectedNonNormalizedSourceText
                 && !formatCode )
            {
                // Update the obj/transformed file to the non-normalized expected text, so that future call to update_transformed.txt
                // does not overwrite any whitespace change.
                File.WriteAllText( actualTransformedPath, expectedNonNormalizedSourceText );
            }
            else if ( storedTransformedSourceText == null || storedTransformedSourceText != actualTransformedNormalizedSourceText )
            {
                // Coverage: ignore

                File.WriteAllText( actualTransformedPath, actualTransformedNonNormalizedText );
            }

            if ( this.Logger != null )
            {
                var logger = this.Logger!;
                logger.WriteLine( "Expected transformed file: " + expectedTransformedPath );
                logger.WriteLine( "Actual transformed file: " + actualTransformedPath );
                logger.WriteLine( "" );
                logger.WriteLine( "=== ACTUAL TRANSFORMED CODE ===" );
                logger.WriteLine( actualTransformedNonNormalizedText );
                logger.WriteLine( "=====================" );

                // Write all diagnostics to the logger.
                foreach ( var diagnostic in testResult.Diagnostics )
                {
                    logger.WriteLine( diagnostic.ToString() );
                }
            }

            state["expectedTransformedSourceText"] = expectedTransformedSourceText;
            state["actualTransformedNormalizedSourceText"] = actualTransformedNormalizedSourceText;

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
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: options.NullabilityDisabled == true ? NullableContextOptions.Disable : NullableContextOptions.Enable );

            var projectName = "test";
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

                var formattedOutput = await OutputCodeFormatter.FormatToSyntaxAsync( outputDocument );
                var outputHtmlPath = Path.Combine( htmlDirectory, testInput.TestName + FileExtensions.OutputHtml );
                var formattedOutputDocument = testResult.InputProject.AddDocument( "ConsolidatedFormatted.cs", formattedOutput );

                var outputHtml = File.CreateText( outputHtmlPath );

                using ( outputHtml.IgnoreAsyncDisposable() )
                {
                    await htmlCodeWriter.WriteAsync( formattedOutputDocument, outputHtml );
                }

                testResult.OutputHtmlPath = outputHtmlPath;
            }
        }

        protected virtual HtmlCodeWriter CreateHtmlCodeWriter( IServiceProvider serviceProvider, TestOptions options )
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
}