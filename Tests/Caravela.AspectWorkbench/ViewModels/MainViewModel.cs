// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using Caravela.AspectWorkbench.Model;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Tests.Integration.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using PostSharp.Patterns.Model;

namespace Caravela.AspectWorkbench.ViewModels
{
    [NotifyPropertyChanged]
    public class MainViewModel
    {
        private readonly TestSerializer _testSerializer;

        private TemplateTest? _currentTest;

        public MainViewModel()
        {
            this._testSerializer = new();
        }

        public string Title => this.CurrentPath == null ? "Aspect Workbench" : $"Aspect Workbench - {this.CurrentPath}";

        public string? TestText { get; set; }

        public string? ExpectedOutputText { get; set; }

        public FlowDocument? ColoredTemplateDocument { get; set; }

        public FlowDocument? CompiledTemplateDocument { get; set; }

        public FlowDocument? TransformedTargetDocument { get; set; }

        public FlowDocument? ErrorsDocument { get; set; }

        public bool IsNewTest => string.IsNullOrEmpty( this.CurrentPath );

        private string? CurrentPath { get; set; }

        public async Task RunTestAsync()
        {
            if ( this.TestText == null )
            {
                throw new InvalidOperationException( $"Property {nameof( this.TestText )} not set." );
            }

            try
            {

                this.ErrorsDocument = new FlowDocument();
                this.TransformedTargetDocument = null;

                TestRunnerBase testRunner = this.TestText.Contains( "[TestTemplate]" ) ? new TemplatingTestRunner() : new AspectTestRunner();
                var syntaxColorizer = new SyntaxColorizer( testRunner.CreateProject() );

                var testInput = new TestInput( "interactive", this.TestText );

                var compilationStopwatch = Stopwatch.StartNew();

                var testResult = await testRunner.RunTestAsync( testInput );
                compilationStopwatch.Stop();

                if ( testResult.AnnotatedTemplateSyntax != null )
                {
                    // Display the annotated syntax tree.
                    var sourceText = await testResult.TemplateDocument.GetTextAsync();
                    var classifier = new TextSpanClassifier( sourceText, testRunner is TemplatingTestRunner );
                    classifier.Visit( testResult.AnnotatedTemplateSyntax );
                    var metaSpans = classifier.ClassifiedTextSpans;

                    this.ColoredTemplateDocument = await syntaxColorizer.WriteSyntaxColoring( sourceText, metaSpans );
                }

                if ( testResult.TransformedTemplateSyntax != null )
                {
                    // Render the transformed tree.
                    var project3 = testRunner.CreateProject();
                    var document3 = project3.AddDocument( "name.cs", testResult.TransformedTemplateSyntax );
                    var optionSet = (await document3.GetOptionsAsync()).WithChangedOption( FormattingOptions.IndentationSize, 4 );

                    var formattedTransformedSyntaxRoot = Formatter.Format( testResult.TransformedTemplateSyntax, project3.Solution.Workspace, optionSet );
                    var text4 = formattedTransformedSyntaxRoot.GetText( Encoding.UTF8 );
                    var spanMarker = new TextSpanClassifier( text4 );
                    spanMarker.Visit( formattedTransformedSyntaxRoot );
                    this.CompiledTemplateDocument = await syntaxColorizer.WriteSyntaxColoring( text4, spanMarker.ClassifiedTextSpans );
                }

                var errorsDocument = new FlowDocument();
                
                if ( testResult.TransformedTargetSourceText != null )
                {
                    // Display the transformed code.
                    this.TransformedTargetDocument = await syntaxColorizer.WriteSyntaxColoring( testResult.TransformedTargetSourceText, null );
                    
                    // Compare the output and shows the result.
                    if ( UnitTestBase.NormalizeString( this.ExpectedOutputText ) ==
                         UnitTestBase.NormalizeString( testResult.TransformedTargetSourceText.ToString() ) )
                    {
                        errorsDocument.Blocks.Add( new Paragraph(new Run("The transformed target code is equal to expectations.") { Foreground = Brushes.Green }) );
                    }
                    else
                    {
                        errorsDocument.Blocks.Add( new Paragraph(new Run("The transformed target code is different than expectations.") { Foreground = Brushes.Red }) );
                    }
                }

                var errors = testResult.Diagnostics;

                errorsDocument.Blocks.AddRange( 
                    errors.Select( e => new Paragraph(
                        inline: new Run(e.ToString() ) 
                        {
                            Foreground = e.Severity switch
                            {
                                DiagnosticSeverity.Error => Brushes.Red,
                                DiagnosticSeverity.Warning => Brushes.Chocolate,
                                _ => Brushes.Black
                            }
                        }) ) );

                if ( !string.IsNullOrEmpty( testResult.ErrorMessage ) )
                {
                    errorsDocument.Blocks.Add( new Paragraph( new Run( testResult.ErrorMessage ) { Foreground = Brushes.Red } ) );
                }

                this.ErrorsDocument = errorsDocument;
            }
            catch ( Exception e )
            {
                var errorsDocument = new FlowDocument();
                errorsDocument.Blocks.Add( new Paragraph( new Run( e.ToString() ) { Foreground = Brushes.Red } ) );
            }
        }

        public void NewTest()
        {
            this.TestText = NewTestDefaults.TemplateSource;
            this.ExpectedOutputText = null;
            this.CompiledTemplateDocument = null;
            this.TransformedTargetDocument = null;
            this.CurrentPath = null;
        }

        public async Task LoadTestAsync( string filePath )
        {
            this._currentTest = await TestSerializer.LoadFromFileAsync( filePath );

            var input = this._currentTest.Input ?? throw new InvalidOperationException( $"The {nameof( this._currentTest.Input )} property cannot be null." );

            this.TestText = input.TestSource;
            this.ExpectedOutputText = this._currentTest.ExpectedOutput;
            this.CompiledTemplateDocument = null;
            this.TransformedTargetDocument = null;
            this.CurrentPath = filePath;
        }

        public async Task SaveTestAsync( string? filePath )
        {
            filePath ??= this.CurrentPath ?? throw new ArgumentNullException( nameof( filePath ) );

            if ( string.IsNullOrEmpty( filePath ) )
            {
                throw new ArgumentException( "The path cannot be null or empty." );
            }

            if ( string.IsNullOrEmpty( this.TestText ) )
            {
                throw new InvalidOperationException( $"The {nameof( this.TestText )} property cannot be null." );
            }

            if ( this._currentTest == null )
            {
                this._currentTest = new TemplateTest();
            }

            this._currentTest.Input = new TestInput( "interactive", this.TestText );
            this._currentTest.ExpectedOutput = this.ExpectedOutputText ?? string.Empty;

            this.CurrentPath = filePath;

            await TestSerializer.SaveToFileAsync( this._currentTest, this.CurrentPath );
        }
    }
}
