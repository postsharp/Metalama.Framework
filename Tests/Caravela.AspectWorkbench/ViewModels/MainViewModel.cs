// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Caravela.AspectWorkbench.Model;
using Caravela.Framework.Impl.Templating;
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
        private readonly WorkbenchTemplatingTestRunner _templatingTestRunner; // TODO: WorkbenchAspectTestRunner
        private readonly WorkbenchHighlightingTestRunner _highlightingTestRunner;
        private readonly SyntaxColorizer _syntaxColorizer;

        private TemplateTest? _currentTest;

        public MainViewModel()
        {
            this._testSerializer = new();
            this._templatingTestRunner = new();
            this._highlightingTestRunner = new();
            this._syntaxColorizer = new( this._highlightingTestRunner );
        }

        public string Title => this.CurrentPath == null ? "Aspect Workbench" : $"Aspect Workbench - {this.CurrentPath}";

        public string? TestText { get; set; }

        public string? ExpectedOutputText { get; set; }

        public FlowDocument? ColoredTemplateDocument { get; set; }

        public FlowDocument? CompiledTemplateDocument { get; set; }

        public FlowDocument? TransformedTargetDocument { get; set; }

        public string? ErrorsText { get; set; }

        public bool IsNewTest => string.IsNullOrEmpty( this.CurrentPath );

        private string? CurrentPath { get; set; }

        public async Task RunTestAsync()
        {
            if ( this.TestText == null )
            {
                throw new InvalidOperationException( $"Property {nameof( this.TestText )} not set." );
            }

            this.ErrorsText = string.Empty;
            this.TransformedTargetDocument = null;

            var testInput = new TestInput( "interactive", null, this.TestText, null );

            var compilationStopwatch = Stopwatch.StartNew();
            var testResult = await this._templatingTestRunner.RunAsync( testInput );
            compilationStopwatch.Stop();

            var highlightingStopwatch = Stopwatch.StartNew();
            var highlightingResult = await this._highlightingTestRunner.RunAsync( testInput );
            highlightingStopwatch.Stop();
            
            if ( highlightingResult.AnnotatedTemplateSyntax != null )
            {
                // Display the annotated syntax tree.
                var sourceText = await highlightingResult.TemplateDocument.GetTextAsync();
                var classifier = new TextSpanClassifier( sourceText );
                classifier.Visit( highlightingResult.AnnotatedTemplateSyntax );
                var metaSpans = classifier.ClassifiedTextSpans;

                this.ColoredTemplateDocument = await this._syntaxColorizer.WriteSyntaxColoring( sourceText, metaSpans );
            }

            if ( testResult.TransformedTemplateSyntax != null )
            {
                // Render the transformed tree.
                var project3 = this._templatingTestRunner.CreateProject();
                var document3 = project3.AddDocument( "name.cs", testResult.TransformedTemplateSyntax );
                var optionSet = (await document3.GetOptionsAsync()).WithChangedOption( FormattingOptions.IndentationSize, 4 );

                var formattedTransformedSyntaxRoot = Formatter.Format( testResult.TransformedTemplateSyntax, project3.Solution.Workspace, optionSet );
                var text4 = formattedTransformedSyntaxRoot.GetText( Encoding.UTF8 );
                var spanMarker = new TextSpanClassifier( text4 );
                spanMarker.Visit( formattedTransformedSyntaxRoot );
                this.CompiledTemplateDocument = await this._syntaxColorizer.WriteSyntaxColoring( text4, spanMarker.ClassifiedTextSpans );
            }

            if ( testResult.TransformedTargetSource != null )
            {
                // Display the transformed code.
                this.TransformedTargetDocument = await this._syntaxColorizer.WriteSyntaxColoring( testResult.TransformedTargetSource, null );
            }

            var errorsTextBuilder = new StringBuilder();

            var errors = testResult.Diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error );
            foreach ( var e in errors )
            {
                errorsTextBuilder.AppendLine( e.Location + ":" + e.Id + " " + e.GetMessage() );
            }

            if ( !string.IsNullOrEmpty( testResult.ErrorMessage ) )
            {
                errorsTextBuilder.AppendLine( testResult.ErrorMessage );
            }

            errorsTextBuilder.AppendLine( $"It took {compilationStopwatch.Elapsed.TotalSeconds:f1} s to compile and {highlightingStopwatch.Elapsed.TotalSeconds:f1} s to highlight." );

            this.ErrorsText = errorsTextBuilder.ToString();
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
            this._currentTest = await this._testSerializer.LoadFromFileAsync( filePath );

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

            this._currentTest.Input = new TestInput( "interactive", null, this.TestText, null );
            this._currentTest.ExpectedOutput = this.ExpectedOutputText ?? string.Empty;

            this.CurrentPath = filePath;

            await this._testSerializer.SaveToFileAsync( this._currentTest, this.CurrentPath );
        }
    }
}
