﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.AspectWorkbench.Model;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Tests.Integration.Runners;
using Metalama.Testing.UnitTesting;
using Metalama.Testing.AspectTesting;
using Metalama.Testing.AspectTesting.Licensing;
using Microsoft.CodeAnalysis;
using PostSharp.Patterns.Model;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Metalama.AspectWorkbench.ViewModels
{
    [NotifyPropertyChanged]
    public class MainViewModel
    {
        static MainViewModel()
        {
            // Make sure a few assemblies are loaded.
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            typeof(Console).ToString();
        }

        private static readonly TestProjectProperties _projectProperties = new(
            null,
            ImmutableArray.Create( "NET5_0_OR_GREATER", "NET6_0_OR_GREATER" ),
            "net6.0",
            new TestFrameworkLicenseStatus( typeof(MainViewModel).Assembly.GetName().Name!, null, false ) );

        private TemplateTest? _currentTest;

        public string Title => this.CurrentPath == null ? "Aspect Workbench" : $"Aspect Workbench - {this.CurrentPath}";

        public string? SourceCode { get; set; }

        public string? ExpectedTransformedCode { get; set; }

        public FlowDocument? ColoredSourceCodeDocument { get; set; }

        public FlowDocument? CompiledTemplateDocument { get; set; }

        public string? CompiledTemplatePath { get; set; }

        public FlowDocument? TransformedCodeDocument { get; set; }

        public FlowDocument? IntermediateLinkerCodeCodeDocument { get; set; }

        public FlowDocument? ErrorsDocument { get; set; }

        public bool IsNewTest => string.IsNullOrEmpty( this.CurrentPath );

        private string? CurrentPath { get; set; }

        public DetailPaneContent DetailPaneContent { get; set; }

        public string? ActualProgramOutput { get; set; }

        public string? ExpectedProgramOutput { get; set; }

        public Visibility CompiledTemplateVisibility
            => this.DetailPaneContent == DetailPaneContent.CompiledTemplate ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ProgramOutputVisibility => this.DetailPaneContent == DetailPaneContent.ProgramOutput ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IntermediateLinkerCodeVisibility
            => this.DetailPaneContent == DetailPaneContent.IntermediateLinkerCode ? Visibility.Visible : Visibility.Collapsed;

        public Visibility HighlightedTemplateVisibility
            => this.DetailPaneContent == DetailPaneContent.HighlightedTemplate ? Visibility.Visible : Visibility.Collapsed;

        public async Task RunTestAsync()
        {
            if ( this.SourceCode == null )
            {
                throw new InvalidOperationException( $"Property {nameof(this.SourceCode)} not set." );
            }
            else if ( this.CurrentPath == null )
            {
                throw new InvalidOperationException( "The test must be saved before you can run it." );
            }

            this.ErrorsDocument = new FlowDocument();
            this.TransformedCodeDocument = null;

            var testInput = TestInput.FromSource( _projectProperties, this.SourceCode, this.CurrentPath );

            var metadataReferences = TestCompilationFactory.GetMetadataReferences().ToMutableList();
            metadataReferences.Add( MetadataReference.CreateFromFile( typeof(TestTemplateAttribute).Assembly.Location ) );

            // This is a dirty trick. We should read options from the directory instead.
            if ( this.SourceCode.Contains( "[TestTemplate]", StringComparison.Ordinal ) )
            {
                testInput.Options.TestRunnerFactoryType = typeof(TemplatingTestRunnerFactory).AssemblyQualifiedName;
            }

            var testContextOptions =
                new TestContextOptions() { FormatCompileTimeCode = true, References = metadataReferences.ToImmutableArray<MetadataReference>() };

            using var testContext = new TestContext( testContextOptions );

            var serviceProvider = testContext.ServiceProvider;

            var syntaxColorizer = new SyntaxColorizer( serviceProvider );

            var testRunner = TestRunnerFactory.CreateTestRunner(
                testInput,
                serviceProvider,
                new TestProjectReferences(
                    metadataReferences.ToImmutableArray<MetadataReference>(),
                    ImmutableArray<object>.Empty,
                    null ),
                null );

            var compilationStopwatch = Stopwatch.StartNew();
            using var testResult = new TestResult();

            Exception? exception = null;

            try
            {
                await testRunner.RunAsync( testInput, testResult, testContext );
            }
            catch ( Exception e )
            {
                exception = e;
            }
            finally
            {
                compilationStopwatch.Stop();
            }

            var errorsDocument = new FlowDocument();

            var testSyntaxTree = testResult.SyntaxTrees.FirstOrDefault();

            if ( testSyntaxTree != null )
            {
                var annotatedTemplateSyntax = testSyntaxTree.AnnotatedSyntaxRoot;

                if ( annotatedTemplateSyntax != null )
                {
                    // Display the annotated syntax tree.
                    this.ColoredSourceCodeDocument = await syntaxColorizer.WriteSyntaxColoringAsync(
                        testResult.SyntaxTrees.First().InputDocument,
                        diagnostics: testResult.Diagnostics );
                }

                var transformedTemplateSyntax = testSyntaxTree.OutputCompileTimeSyntaxRoot;

                if ( transformedTemplateSyntax != null )
                {
                    // Render the transformed tree.
                    var project3 = testRunner.CreateProject( testInput.Options );

                    var document3 = project3.AddDocument(
                        testSyntaxTree.OutputCompileTimePath ?? "TransformedTemplate.cs",
                        transformedTemplateSyntax,
                        filePath: testSyntaxTree.OutputCompileTimePath );

                    var formattedDocument3 = await OutputCodeFormatter.FormatAsync( document3, testResult.CompileTimeCompilationDiagnostics );

                    this.CompiledTemplateDocument = await syntaxColorizer.WriteSyntaxColoringAsync( formattedDocument3.Document, true );

                    if ( testResult.CompileTimeCompilation != null )
                    {
                        if ( !SyntaxTreeStructureVerifier.VerifyMetaSyntax( testResult.CompileTimeCompilation, serviceProvider ) )
                        {
                            testResult.SetFailed(
                                "The compiled template syntax tree object model is incorrect: roundloop parsing verification failed. Add a breakpoint in SyntaxTreeStructureVerifier.Verify and diff manually." );
                        }
                    }
                }
            }

            // Multi file tests are not supported.
            var consolidatedOutputSyntax = await testResult.GetTestOutputsWithDiagnostics().Single().GetRootAsync();

            if ( !testInput.Options.FormatOutput.GetValueOrDefault() )
            {
                consolidatedOutputSyntax = consolidatedOutputSyntax.NormalizeWhitespace();
            }

            var consolidatedOutputText = await consolidatedOutputSyntax.SyntaxTree.GetTextAsync();

            var project = testResult.OutputProject ?? testResult.InputProject;

            if ( project != null )
            {
                var consolidatedOutputDocument = project.AddDocument( "ConsolidatedOutput.cs", consolidatedOutputSyntax );

                // Display the transformed code.
                this.TransformedCodeDocument = await syntaxColorizer.WriteSyntaxColoringAsync( consolidatedOutputDocument );
            }

            // Display the intermediate linker code.
            if ( testResult.IntermediateLinkerCompilation != null )
            {
                var intermediateSyntaxTree = testResult.IntermediateLinkerCompilation.Compilation.SyntaxTrees.First();
                var linkerProject = testRunner.CreateProject( testInput.Options );

                var linkerDocument = linkerProject.AddDocument(
                    "IntermediateLinkerCode.cs",
                    RenderAspectReferences( await intermediateSyntaxTree.GetRootAsync() ) );

                this.IntermediateLinkerCodeCodeDocument = await syntaxColorizer.WriteSyntaxColoringAsync( linkerDocument );
            }

            if ( exception != null )
            {
                errorsDocument.Blocks.Add( new Paragraph( new Run( exception.ToString() ) { Foreground = Brushes.Red } ) );
            }
            else
            {
                // Compare the output and shows the result.
                if ( BaseTestRunner.NormalizeTestOutput( this.ExpectedTransformedCode, false, true ) ==
                     BaseTestRunner.NormalizeTestOutput( consolidatedOutputText.ToString(), false, true ) )
                {
                    errorsDocument.Blocks.Add(
                        new Paragraph( new Run( "The transformed target code is equal to expectations." ) { Foreground = Brushes.Green } ) );
                }
                else
                {
                    errorsDocument.Blocks.Add(
                        new Paragraph( new Run( "The transformed target code is different than expectations." ) { Foreground = Brushes.Red } ) );
                }
            }

            var errors = testResult.Diagnostics;

            errorsDocument.Blocks.AddRange(
                errors.Select(
                    e => new Paragraph(
                        new Run( e.ToString() )
                        {
                            Foreground = e.Severity switch
                            {
                                DiagnosticSeverity.Error => Brushes.Red,
                                DiagnosticSeverity.Warning => Brushes.Chocolate,
                                _ => Brushes.Black
                            }
                        } ) ) );

            if ( !string.IsNullOrEmpty( testResult.ErrorMessage ) )
            {
                errorsDocument.Blocks.Add( new Paragraph( new Run( testResult.ErrorMessage ) { Foreground = Brushes.Red } ) );
            }

            this.ErrorsDocument = errorsDocument;

            this.ActualProgramOutput = testResult.ProgramOutput;

            if ( this.ActualProgramOutput == this.ExpectedProgramOutput )
            {
                errorsDocument.Blocks.Add( new Paragraph( new Run( "The program output is equal to expectations." ) { Foreground = Brushes.Green } ) );
            }
            else
            {
                errorsDocument.Blocks.Add( new Paragraph( new Run( "The program output code is different than expectations." ) { Foreground = Brushes.Red } ) );
            }
        }

        private static SyntaxNode RenderAspectReferences( SyntaxNode rootNode ) => new AnnotationRenderingRewriter().Visit( rootNode ).AssertNotNull();

        public void NewTest( string path )
        {
            var projectDirectory = TestInput.FromSource( _projectProperties, "", path ).ProjectDirectory;
            var pathParts = Path.GetRelativePath( projectDirectory, path ).Split( "\\" ).SelectArray( Path.GetFileNameWithoutExtension ).Skip( 1 );
            var ns = Path.GetFileName( projectDirectory ) + "." + string.Join( ".", pathParts );
            this.SourceCode = NewTestDefaults.TemplateSource.Replace( "$ns", ns, StringComparison.OrdinalIgnoreCase );
            this.ExpectedTransformedCode = null;
            this.CompiledTemplateDocument = null;
            this.TransformedCodeDocument = null;
            this.ExpectedProgramOutput = null;
            this.ActualProgramOutput = null;
            this.CurrentPath = path;
        }

        public async Task LoadTestAsync( string filePath )
        {
            this._currentTest = await TestSerializer.LoadFromFileAsync( _projectProperties, filePath );

            var input = this._currentTest.Input ?? throw new InvalidOperationException( $"The {nameof(this._currentTest.Input)} property cannot be null." );

            this.SourceCode = input.SourceCode;
            this.ExpectedTransformedCode = this._currentTest.ExpectedTransformedCode;
            this.ExpectedProgramOutput = this._currentTest.ExpectedProgramOutput;
            this.CompiledTemplateDocument = null;
            this.TransformedCodeDocument = null;
            this.ActualProgramOutput = null;
            this.CurrentPath = filePath;
        }

        public async Task SaveTestAsync( string? filePath )
        {
            filePath ??= this.CurrentPath ?? throw new ArgumentNullException( nameof(filePath) );

            if ( string.IsNullOrEmpty( filePath ) )
            {
                throw new ArgumentException( "The path cannot be null or empty." );
            }

            if ( string.IsNullOrEmpty( this.SourceCode ) )
            {
                throw new InvalidOperationException( $"The {nameof(this.SourceCode)} property cannot be null." );
            }

            this._currentTest ??= new TemplateTest();

            this._currentTest.Input = TestInput.FromSource( _projectProperties, this.SourceCode, filePath );
            this._currentTest.ExpectedTransformedCode = this.ExpectedTransformedCode ?? string.Empty;
            this._currentTest.ExpectedProgramOutput = this.ExpectedProgramOutput ?? string.Empty;

            this.CurrentPath = filePath;

            await TestSerializer.SaveToFileAsync( this._currentTest, this.CurrentPath );
        }
    }
}