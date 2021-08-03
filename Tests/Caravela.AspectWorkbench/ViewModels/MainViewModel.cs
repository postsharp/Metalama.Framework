// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.AspectWorkbench.Model;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Tests.Integration.Runners;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PostSharp.Patterns.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Caravela.AspectWorkbench.ViewModels
{
    public enum DetailPaneContent
    {
        ProgramOutput,
        CompiledTemplate,
        IntermediateLinkerCode,
        HighlightedTemplate
    }

    [NotifyPropertyChanged]
    public class MainViewModel
    {
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

            try
            {
                this.ErrorsDocument = new FlowDocument();
                this.TransformedCodeDocument = null;

                var testInput = TestInput.FromSource( this.SourceCode, this.CurrentPath );

                testInput.Options.References.AddRange(
                    TestCompilationFactory.GetMetadataReferences()
                        .Select( r => new TestAssemblyReference { Path = r.FilePath } ) );

                // This is a dirty trick. We should read options from the directory instead.
                if ( this.SourceCode.Contains( "[TestTemplate]", StringComparison.Ordinal ) )
                {
                    testInput.Options.TestRunnerFactoryType = typeof(TemplatingTestRunnerFactory).AssemblyQualifiedName;
                }

                using var testProjectOptions = new TestProjectOptions() { FormatCompileTimeCode = true };
                using var serviceProvider = ServiceProviderFactory.GetServiceProvider( testProjectOptions );

                var testRunner = TestRunnerFactory.CreateTestRunner( testInput, serviceProvider, null );

                var compilationStopwatch = Stopwatch.StartNew();

                var testResult = await testRunner.RunTestAsync( testInput );
                compilationStopwatch.Stop();

                var testSyntaxTree = testResult.SyntaxTrees.First();

                var annotatedTemplateSyntax = testSyntaxTree.AnnotatedSyntaxRoot;

                if ( annotatedTemplateSyntax != null )
                {
                    // Display the annotated syntax tree.
                    this.ColoredSourceCodeDocument = SyntaxColorizer.WriteSyntaxColoring(
                        testResult.SyntaxTrees.First().InputDocument!,
                        testResult.Diagnostics,
                        annotatedTemplateSyntax );
                }

                var errorsDocument = new FlowDocument();

                var transformedTemplateSyntax = testSyntaxTree.OutputCompileTimeSyntaxRoot;

                if ( transformedTemplateSyntax != null )
                {
                    SyntaxTreeStructureVerifier.Verify( testResult.CompileTimeCompilation! );

                    // Render the transformed tree.
                    var project3 = testRunner.CreateProject( testInput.Options );
                    var document3 = project3.AddDocument( "name.cs", transformedTemplateSyntax );

                    var formattedDocument3 = await OutputCodeFormatter.FormatToDocumentAsync( document3 );

                    this.CompiledTemplateDocument = SyntaxColorizer.WriteSyntaxColoring( formattedDocument3.Document, testResult.Diagnostics );
                }

                    var consolidatedOutputSyntax = testResult.GetConsolidatedTestOutput();
                    var consolidatedOutputText = await consolidatedOutputSyntax.SyntaxTree.GetTextAsync();

                if ( testResult.OutputProject != null )
                {

                    var consolidatedOutputDocument = testResult.OutputProject!.AddDocument( "ConsolidatedOutput", consolidatedOutputSyntax );

                    // Display the transformed code.
                    this.TransformedCodeDocument = SyntaxColorizer.WriteSyntaxColoring( consolidatedOutputDocument );

                }
                

                // Display the intermediate linker code.
                if ( testResult.IntermediateLinkerCompilation != null )
                {
                    var intermediateSyntaxTree = testResult.IntermediateLinkerCompilation.Compilation.SyntaxTrees.First();
                    var linkerProject = testRunner.CreateProject( testInput.Options );
                    var linkerDocument = linkerProject.AddDocument( "name.cs", await intermediateSyntaxTree.GetRootAsync() );
                    this.IntermediateLinkerCodeCodeDocument = SyntaxColorizer.WriteSyntaxColoring( linkerDocument );
                }

                // Compare the output and shows the result.
                if ( BaseTestRunner.NormalizeTestOutput( this.ExpectedTransformedCode, false ) ==
                     BaseTestRunner.NormalizeTestOutput( consolidatedOutputText.ToString(), false ) )
                {
                    errorsDocument.Blocks.Add(
                        new Paragraph( new Run( "The transformed target code is equal to expectations." ) { Foreground = Brushes.Green } ) );
                }
                else
                {
                    errorsDocument.Blocks.Add(
                        new Paragraph( new Run( "The transformed target code is different than expectations." ) { Foreground = Brushes.Red } ) );
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
                    errorsDocument.Blocks.Add(
                        new Paragraph( new Run( "The program output code is different than expectations." ) { Foreground = Brushes.Red } ) );
                }
            }
            catch ( Exception e )
            {
                var errorsDocument = new FlowDocument();
                errorsDocument.Blocks.Add( new Paragraph( new Run( e.ToString() ) { Foreground = Brushes.Red } ) );
                this.ErrorsDocument = errorsDocument;
            }
        }

        public void NewTest( string path )
        {
            this.SourceCode = NewTestDefaults.TemplateSource;
            this.ExpectedTransformedCode = null;
            this.CompiledTemplateDocument = null;
            this.TransformedCodeDocument = null;
            this.ExpectedProgramOutput = null;
            this.ActualProgramOutput = null;
            this.CurrentPath = path;
        }

        public async Task LoadTestAsync( string filePath )
        {
            this._currentTest = await TestSerializer.LoadFromFileAsync( filePath );

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

            if ( this._currentTest == null )
            {
                this._currentTest = new TemplateTest();
            }

            this._currentTest.Input = TestInput.FromSource( this.SourceCode, filePath );
            this._currentTest.ExpectedTransformedCode = this.ExpectedTransformedCode ?? string.Empty;
            this._currentTest.ExpectedProgramOutput = this.ExpectedProgramOutput ?? string.Empty;

            this.CurrentPath = filePath;

            await TestSerializer.SaveToFileAsync( this._currentTest, this.CurrentPath );
        }
    }
}