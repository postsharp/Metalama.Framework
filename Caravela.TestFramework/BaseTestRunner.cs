// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public virtual TestResult RunTest( TestInput testInput )
        {
            var testResult = this.CreateTestResult();

            // Source. Note that we don't pass the full path to the Document because it causes call stacks of exceptions to have full paths,
            // which is more difficult to test.
            var project = this.CreateProject().WithParseOptions( CSharpParseOptions.Default.WithPreprocessorSymbols( "TESTRUNNER" ) );
            var sourceFileName = testInput.TestName + ".cs";
            var mainDocument = project.AddDocument( sourceFileName, SourceText.From( testInput.SourceCode, Encoding.UTF8 ), filePath: sourceFileName );
            project = mainDocument.Project;

            var syntaxTree = mainDocument.GetSyntaxTreeAsync().Result!;

            testResult.AddInputDocument( mainDocument );

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

                var includedDocument = project.AddDocument( includedFileName, SourceText.From( includedText, Encoding.UTF8 ), filePath: includedFileName );
                project = mainDocument.Project;

                testResult.AddInputDocument( includedDocument );

                var includedSyntaxTree = includedDocument.GetSyntaxTreeAsync().Result;
                initialCompilation = initialCompilation.AddSyntaxTrees( includedSyntaxTree );
            }

            ValidateCustomAttributes( initialCompilation );

            testResult.Project = project;
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

        public static string? NormalizeTestOutput( string? s ) => s == null ? null : NormalizeTestOutput( CSharpSyntaxTree.ParseText( s ).GetRoot() );

        public static string? NormalizeTestOutput( SyntaxNode syntaxNode )
        {
            var s = syntaxNode.NormalizeWhitespace().ToFullString();

            s = _newLineRegex.Replace( s, "\n" );
            s = _spaceRegex.Replace( s, " " );

            return s;
        }

        public virtual void ExecuteAssertions( TestInput testInput, TestResult testResult )
        {
            if ( this.ProjectDirectory == null )
            {
                throw new InvalidOperationException( "This method cannot be called when the test path is unknown." );
            }

            // Compare the "Target" region of the transformed code to the expected output.
            // If the region is not found then compare the complete transformed code.
            var sourceAbsolutePath = testInput.FullPath;

            var expectedTransformedPath = Path.Combine(
                Path.GetDirectoryName( sourceAbsolutePath )!,
                Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + FileExtensions.TransformedCode );

            var consolidatedTestOutput = testResult.GetConsolidatedTestOutput();
            var actualTransformedNonNormalizedText = consolidatedTestOutput.ToFullString();
            var actualTransformedNormalizedSourceText = NormalizeTestOutput( consolidatedTestOutput );

            // If the expectation file does not exist, create it with some placeholder content.
            if ( !File.Exists( expectedTransformedPath ) )
            {
                File.WriteAllText(
                    expectedTransformedPath,
                    "// TODO: Replace this file with the correct transformed code. See the test output for the actual transformed code." );
            }

            // Read expectations from the file.
            var expectedNonNormalizedSourceText = File.ReadAllText( expectedTransformedPath );
            var expectedTransformedSourceText = NormalizeTestOutput( expectedNonNormalizedSourceText );

            // Update the file in obj/transformed if it is different.
            var actualTransformedPath = Path.Combine(
                this.ProjectDirectory,
                "obj",
                "transformed",
                Path.GetDirectoryName( testInput.RelativePath ) ?? "",
                Path.GetFileNameWithoutExtension( testInput.RelativePath ) + FileExtensions.TransformedCode );

            Directory.CreateDirectory( Path.GetDirectoryName( actualTransformedPath ) );

            var storedTransformedSourceText =
                File.Exists( actualTransformedPath ) ? NormalizeTestOutput( File.ReadAllText( actualTransformedPath ) ) : null;

            if ( expectedTransformedSourceText == actualTransformedNormalizedSourceText
                 && storedTransformedSourceText != expectedNonNormalizedSourceText )
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
        public Project CreateProject()
        {
            var compilation = TestCompilationFactory.CreateEmptyCSharpCompilation( null, this._additionalAssemblies );

            var guid = Guid.NewGuid();
            var workspace1 = new AdhocWorkspace();
            var solution = workspace1.CurrentSolution;

            var project = solution.AddProject( guid.ToString(), guid.ToString(), LanguageNames.CSharp )
                .WithCompilationOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ) )
                .AddMetadataReferences( compilation.References );

            // Don't add the assembly containing the code to test because it would result in duplicate symbols.

            return project;
        }

        protected void WriteHtml( TestInput testInput, TestResult testResult )
        {
            foreach ( var syntaxTree in testResult.SyntaxTrees )
            {
                this.WriteHtml( testInput, syntaxTree );
            }
        }

        protected virtual void WriteHtmlProlog( TextWriter writer ) { }
        protected virtual void WriteHtmlEpilogue( TextWriter writer ) { }

        private void WriteHtml( TestInput testInput, TestSyntaxTree testSyntaxTree )
        {
            var htmlPath = Path.Combine(
                this.ProjectDirectory!,
                "obj",
                "highlighted",
                Path.GetDirectoryName( testInput.RelativePath ) ?? "",
                Path.GetFileNameWithoutExtension( testSyntaxTree.InputDocument.FilePath ) + FileExtensions.Html );

            if ( testSyntaxTree.AnnotatedSyntaxRoot == null )
            {
                // The tree has not been annotated, so there is nothing to write.
                return;
            }

            testSyntaxTree.OutputHtmlPath = htmlPath;

            var htmlDirectory = Path.GetDirectoryName( htmlPath );

            if ( !Directory.Exists( htmlDirectory ) )
            {
                Directory.CreateDirectory( htmlDirectory );
            }

            this.Logger?.WriteLine( "HTML output file: " + htmlPath );

            var sourceText = testSyntaxTree.InputDocument.GetTextAsync().Result;
            var classifier = new TextSpanClassifier( sourceText, detectRegion: true );
            classifier.Visit( testSyntaxTree.AnnotatedSyntaxRoot );

            using var textWriter = File.CreateText( htmlPath );
            
            this.WriteHtmlProlog( textWriter );
            textWriter.Write( "<pre><code class=\"lang-csharp\">" );

            var i = 0;

            foreach ( var classifiedSpan in classifier.ClassifiedTextSpans )
            {
                // Write the text between the previous span and the current one.
                if ( i < classifiedSpan.Span.Start )
                {
                    var textBefore = sourceText.GetSubText( new TextSpan( i, classifiedSpan.Span.Start - i ) ).ToString();
                    textWriter.Write( HtmlEncode( textBefore ) );
                }

                var spanText = sourceText.GetSubText( classifiedSpan.Span ).ToString();

                if ( classifiedSpan.Classification != TextSpanClassification.Excluded )
                {
                    textWriter.Write( $"<span class='caravelaClassification_{classifiedSpan.Classification}'" );

                    if ( testInput.Options.AddHtmlTitles.GetValueOrDefault() )
                    {
                        var title = classifiedSpan.Classification switch
                        {
                            TextSpanClassification.Dynamic => "Dynamic member",
                            TextSpanClassification.CompileTime => "Compile-time code",
                            TextSpanClassification.RunTime => "Run-time code",
                            TextSpanClassification.TemplateKeyword => "Special member",
                            TextSpanClassification.CompileTimeVariable => "Compile-time variable",
                            _ => null
                        };

                        if ( title != null )
                        {
                            textWriter.Write( $" title='{title}'" );
                        }
                    }

                    textWriter.Write( ">" );
                    textWriter.Write( HtmlEncode( spanText ) );
                    textWriter.Write( "</span>" );
                }

                i = classifiedSpan.Span.End;
            }

            // Write the remaining text.
            if ( i < sourceText.Length )
            {
                textWriter.Write( HtmlEncode( sourceText.GetSubText( i ).ToString() ) );
            }

            textWriter.WriteLine( "</code></pre>" );
            this.WriteHtmlEpilogue( textWriter );
        }

        private static string HtmlEncode( string s )
        {
            var stringBuilder = new StringBuilder( s.Length );

            foreach ( var c in s )
            {
                switch ( c )
                {
                    case '<':
                        stringBuilder.Append( "&lt;" );

                        break;

                    case '>':
                        stringBuilder.Append( "&gt;" );

                        break;

                    case '&':
                        stringBuilder.Append( "&amp;" );

                        break;

                    default:
                        stringBuilder.Append( c );

                        break;
                }
            }

            return stringBuilder.ToString();
        }

    }
}