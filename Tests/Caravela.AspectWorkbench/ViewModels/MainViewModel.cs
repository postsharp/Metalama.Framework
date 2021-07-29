// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.AspectWorkbench.Model;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Tests.Integration.Runners;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using PostSharp.Patterns.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Caravela.AspectWorkbench.ViewModels
{
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

        public FlowDocument? ErrorsDocument { get; set; }

        public bool IsNewTest => string.IsNullOrEmpty( this.CurrentPath );

        private string CurrentPath { get; set; }
        
        public bool ShowCompiledTemplate { get; set; }
        
        public string? ActualProgramOutput { get; set; }
        
        public string? ExpectedProgramOutput { get; set; }

        public Visibility CompiledTemplateVisibility => this.ShowCompiledTemplate ? Visibility.Visible : Visibility.Collapsed;
        
        public Visibility ProgramOutputVisibility => this.ShowCompiledTemplate ? Visibility.Collapsed : Visibility.Visible;

        public async Task RunTestAsync()
        {
            if ( this.SourceCode == null )
            {
                throw new InvalidOperationException( $"Property {nameof(this.SourceCode)} not set." );
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

                var syntaxColorizer = new SyntaxColorizer( testRunner.CreateProject( testInput.Options ) );

                var compilationStopwatch = Stopwatch.StartNew();

                var testResult = await testRunner.RunTestAsync( testInput );
                compilationStopwatch.Stop();

                var testSyntaxTree = testResult.SyntaxTrees.First();

                var annotatedTemplateSyntax = testSyntaxTree.AnnotatedSyntaxRoot;

                if ( annotatedTemplateSyntax != null )
                {
                    // Display the annotated syntax tree.
                    var sourceText = await testResult.SyntaxTrees.First().InputDocument.GetTextAsync();
                    var classifier = new TextSpanClassifier( sourceText, testRunner is TemplatingTestRunner );
                    classifier.Visit( annotatedTemplateSyntax );
                    var metaSpans = classifier.ClassifiedTextSpans;

                    this.ColoredSourceCodeDocument = await syntaxColorizer.WriteSyntaxColoring( sourceText, metaSpans );
                }

                var errorsDocument = new FlowDocument();

                var transformedTemplateSyntax = testSyntaxTree.OutputCompileTimeSyntaxRoot;

                if ( transformedTemplateSyntax != null )
                {
                    SyntaxTreeStructureVerifier.Verify( testResult.CompileTimeCompilation! );

                    // Render the transformed tree.
                    var project3 = testRunner.CreateProject( testInput.Options );
                    var document3 = project3.AddDocument( "name.cs", transformedTemplateSyntax );
                    var optionSet = (await document3.GetOptionsAsync()).WithChangedOption( FormattingOptions.IndentationSize, 4 );

                    var formattedTransformedSyntaxRoot = Formatter.Format( transformedTemplateSyntax, project3.Solution.Workspace, optionSet );
                    var text4 = formattedTransformedSyntaxRoot.GetText( Encoding.UTF8 );
                    var spanMarker = new TextSpanClassifier( text4 );
                    spanMarker.Visit( formattedTransformedSyntaxRoot );
                    this.CompiledTemplateDocument = await syntaxColorizer.WriteSyntaxColoring( text4, spanMarker.ClassifiedTextSpans );
                }

                var consolidatedOutputSyntax = testResult.GetConsolidatedTestOutput();
                var consolidatedOutputText = await consolidatedOutputSyntax.SyntaxTree.GetTextAsync();

                // Display the transformed code.
                this.TransformedCodeDocument = await syntaxColorizer.WriteSyntaxColoring( consolidatedOutputText, null );

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
                    errorsDocument.Blocks.Add(
                        new Paragraph( new Run( "The program output is equal to expectations." ) { Foreground = Brushes.Green } ) );
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