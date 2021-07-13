// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// An abstract class for all template-base tests.
    /// </summary>
    public abstract partial class BaseTestRunner
    {
        private static readonly Regex _spaceRegex = new( " +", RegexOptions.Compiled );
        private static readonly Regex _newLineRegex = new( "[\n|\r]+", RegexOptions.Compiled );
        private readonly MetadataReference[] _additionalAssemblies;

        public IServiceProvider ServiceProvider { get; }

        public BaseTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
        {
            this._additionalAssemblies = metadataReferences
                .Append( MetadataReference.CreateFromFile( typeof(BaseTestRunner).Assembly.Location ) )
                .ToArray();

            this.ServiceProvider = serviceProvider;
            this.ProjectDirectory = projectDirectory;
            this.Logger = logger;
        }

        /// <summary>
        /// Gets the project directory, or <c>null</c> if it is unknown.
        /// </summary>
        public string? ProjectDirectory { get; }

        public ITestOutputHelper? Logger { get; }

        protected virtual TestResult CreateTestResult() => new();

        /// <summary>
        /// Runs a test.
        /// </summary>
        /// <param name="testInput"></param>
        /// <returns></returns>
        public virtual async Task<TestResult> RunTestAsync( TestInput testInput )
        {
            var testResult = this.CreateTestResult();

            // Source. Note that we don't pass the full path to the Document because it causes call stacks of exceptions to have full paths,
            // which is more difficult to test.
            var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols( "TESTRUNNER", "CARAVELA" );
            var project = this.CreateProject( testInput.Options ).WithParseOptions( parseOptions );

            Document AddDocument( string fileName, string sourceCode )
            {
                var parsedSyntaxTree = CSharpSyntaxTree.ParseText( sourceCode, parseOptions, fileName, Encoding.UTF8 );
                var prunedSyntaxRoot = new InactiveCodeRemover().Visit( parsedSyntaxTree.GetRoot() );
                var document = project.AddDocument( fileName, prunedSyntaxRoot, filePath: fileName );
                project = document.Project;

                return document;
            }

            var sourceFileName = testInput.TestName + ".cs";
            var mainDocument = AddDocument( sourceFileName, testInput.SourceCode );

            var syntaxTree = await mainDocument.GetSyntaxTreeAsync()!;

            testResult.AddInputDocument( mainDocument, testInput.FullPath );

            var initialCompilation = CSharpCompilation.Create(
                "test",
                new[] { syntaxTree },
                project.MetadataReferences,
                (CSharpCompilationOptions?) project.CompilationOptions );

            foreach ( var includedFile in testInput.Options.IncludedFiles )
            {
                var includedFullPath = Path.GetFullPath( Path.Combine( Path.GetDirectoryName( testInput.FullPath )!, includedFile ) );
                var includedText = File.ReadAllText( includedFullPath );
                var includedFileName = Path.GetFileName( includedFullPath );

                var includedDocument = AddDocument( includedFileName, includedText );

                testResult.AddInputDocument( includedDocument, includedFullPath );

                var includedSyntaxTree = await includedDocument.GetSyntaxTreeAsync()!;
                initialCompilation = initialCompilation.AddSyntaxTrees( includedSyntaxTree );
            }

            ValidateCustomAttributes( initialCompilation );

            testResult.InputProject = project;
            testResult.TestInput = testInput;
            testResult.InputCompilation = initialCompilation;

            if ( this.ReportInvalidInputCompilation )
            {
                var diagnostics = initialCompilation.GetDiagnostics();
                var errors = diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).ToArray();

                if ( errors.Any() )
                {
                    testResult.Report( errors );
                    testResult.SetFailed( "The initial compilation failed." );

                    return testResult;
                }
            }

            return testResult;
        }

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

        public static string NormalizeEndOfLines( string s ) => _newLineRegex.Replace( s, "\n" );

        public static string? NormalizeTestOutput( string? s, bool preserveFormatting )
            => s == null ? null : NormalizeTestOutput( CSharpSyntaxTree.ParseText( s ).GetRoot(), preserveFormatting );

        private static string? NormalizeTestOutput( SyntaxNode syntaxNode, bool preserveFormatting )
        {
            if ( preserveFormatting )
            {
                return syntaxNode.ToFullString().Replace( "\r\n", "\n" );
            }
            else
            {
                var s = syntaxNode.NormalizeWhitespace().ToFullString();

                s = NormalizeEndOfLines( s );
                s = _spaceRegex.Replace( s, " " );

                return s;
            }
        }

        public virtual void ExecuteAssertions( TestInput testInput, TestResult testResult )
        {
            if ( this.ProjectDirectory == null )
            {
                throw new InvalidOperationException( "This method cannot be called when the test path is unknown." );
            }

            var formatCode = testInput.Options.FormatOutput.GetValueOrDefault( true );

            // Compare the "Target" region of the transformed code to the expected output.
            // If the region is not found then compare the complete transformed code.
            var sourceAbsolutePath = testInput.FullPath;

            var expectedTransformedPath = Path.Combine(
                Path.GetDirectoryName( sourceAbsolutePath )!,
                Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + FileExtensions.TransformedCode );

            var consolidatedTestOutput = testResult.GetConsolidatedTestOutput();
            var actualTransformedNonNormalizedText = consolidatedTestOutput.ToFullString();
            var actualTransformedNormalizedSourceText = NormalizeTestOutput( consolidatedTestOutput, formatCode );

            // If the expectation file does not exist, create it with some placeholder content.
            if ( !File.Exists( expectedTransformedPath ) )
            {
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
                Path.GetDirectoryName( testInput.RelativePath ) ?? "",
                Path.GetFileNameWithoutExtension( testInput.RelativePath ) + FileExtensions.TransformedCode );

            Directory.CreateDirectory( Path.GetDirectoryName( actualTransformedPath ) );

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
                File.WriteAllText( actualTransformedPath, actualTransformedNonNormalizedText );
            }

            if ( this.Logger != null )
            {
                var logger = this.Logger!;
                logger.WriteLine( "Expected output file: " + expectedTransformedPath );
                logger.WriteLine( "Actual output file: " + actualTransformedPath );
                logger.WriteLine( "" );
                logger.WriteLine( "=== ACTUAL OUTPUT ===" );
                logger.WriteLine( actualTransformedNonNormalizedText );
                logger.WriteLine( "=====================" );

                // Write all diagnostics to the logger.
                foreach ( var diagnostic in testResult.Diagnostics )
                {
                    logger.WriteLine( diagnostic.ToString() );
                }
            }

            Assert.Equal( expectedTransformedSourceText, actualTransformedNormalizedSourceText );
        }

        protected virtual bool ReportInvalidInputCompilation => true;

        /// <summary>
        /// Creates a new project that is used to compile the test source.
        /// </summary>
        /// <returns>A new project instance.</returns>
        public Project CreateProject( TestOptions options )
        {
            var compilation = TestCompilationFactory.CreateEmptyCSharpCompilation( null, this._additionalAssemblies );

            var guid = Guid.NewGuid();
            var workspace1 = new AdhocWorkspace();
            var solution = workspace1.CurrentSolution;

            var project = solution.AddProject( guid.ToString(), guid.ToString(), LanguageNames.CSharp )
                .WithCompilationOptions(
                    new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        nullableContextOptions: options.NullabilityDisabled == true ? NullableContextOptions.Disable : NullableContextOptions.Enable ) )
                .AddMetadataReferences( compilation.References );

            // Don't add the assembly containing the code to test because it would result in duplicate symbols.

            return project;
        }

        protected async Task WriteHtmlAsync( TestInput testInput, TestResult testResult )
        {
            var htmlCodeWriter = this.CreateHtmlCodeWriter( testInput.Options );

            var htmlDirectory = Path.Combine(
                this.ProjectDirectory!,
                "obj",
                "html",
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
                    this.WriteHtml( syntaxTree, htmlDirectory, htmlCodeWriter );
                }
            }

            // Write the consolidated output.
            if ( testInput.Options.WriteOutputHtml.GetValueOrDefault() )
            {
                var output = testResult.GetConsolidatedTestOutput();
                var outputDocument = testResult.InputProject!.AddDocument( "Consolidated.cs", output );

                var formattedOutput = await OutputCodeFormatter.FormatAsync( outputDocument );
                var outputHtmlPath = Path.Combine( htmlDirectory, testInput.TestName + FileExtensions.OutputHtml );
                var formattedOutputDocument = testResult.InputProject.AddDocument( "ConsolidatedFormatted.cs", formattedOutput );

                using ( var outputHtml = File.CreateText( outputHtmlPath ) )
                {
                    htmlCodeWriter.Write( formattedOutputDocument, null, outputHtml );
                }

                testResult.OutputHtmlPath = outputHtmlPath;
            }
        }

        protected virtual HtmlCodeWriter CreateHtmlCodeWriter( TestOptions options )
            => new( new HtmlCodeWriterOptions( options.AddHtmlTitles.GetValueOrDefault() ) );

        private void WriteHtml( TestSyntaxTree testSyntaxTree, string htmlDirectory, HtmlCodeWriter htmlCodeWriter )
        {
            var inputHtmlPath = Path.Combine(
                htmlDirectory,
                Path.GetFileNameWithoutExtension( testSyntaxTree.InputDocument.FilePath ) + FileExtensions.InputHtml );

            testSyntaxTree.HtmlInputRunTimePath = inputHtmlPath;

            this.Logger?.WriteLine( "HTML of input: " + inputHtmlPath );

            // Write the input document.
            using ( var inputTextWriter = File.CreateText( inputHtmlPath ) )
            {
                htmlCodeWriter.Write(
                    testSyntaxTree.InputDocument,
                    testSyntaxTree.AnnotatedSyntaxRoot,
                    inputTextWriter );
            }

            // We have no use case to write the output document because all cases use the consolidated output document instead.
        }
    }
}