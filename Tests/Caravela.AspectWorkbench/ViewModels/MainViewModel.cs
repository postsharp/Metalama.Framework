using Caravela.AspectWorkbench.Model;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Caravela.TestFramework.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using PostSharp.Patterns.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Caravela.AspectWorkbench.Model;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using PostSharp.Patterns.Model;

namespace Caravela.AspectWorkbench.ViewModels
{
    [NotifyPropertyChanged]
    public class MainViewModel
    {
        private readonly WorkbenchTestRunner _testRunner;
        private readonly SyntaxColorizer _syntaxColorizer;
        private readonly TestSerializer _testSerializer;
        private TemplateTest? _currentTest;

        public MainViewModel()
        {
            this._testRunner = new WorkbenchTestRunner();
            this._syntaxColorizer = new SyntaxColorizer( this._testRunner );
            this._testSerializer = new TestSerializer();
        }

        public string Title => this.CurrentPath == null ? "Aspect Workbench" : $"Aspect Workbench - {this.CurrentPath}";

        public string? TemplateText { get; set; }

        public string? TargetText { get; set; }

        public string? ExpectedOutputText { get; set; }

        public FlowDocument? ColoredTemplateDocument { get; set; }

        public FlowDocument? CompiledTemplateDocument { get; set; }

        public FlowDocument? TransformedTargetDocument { get; set; }

        public string? ErrorsText { get; set; }

        public bool IsNewTest => string.IsNullOrEmpty( this.CurrentPath );

        private string? CurrentPath { get; set; }

        public async Task RunTestAsync()
        {
            if ( this.TemplateText == null )
            {
                throw new InvalidOperationException( $"Property {nameof( this.TemplateText )} not set." );
            }

            if ( this.TargetText == null )
            {
                throw new InvalidOperationException( $"Property {nameof( this.TargetText )} not set." );
            }

            this.ErrorsText = string.Empty;
            this.TransformedTargetDocument = null;

            var stopwatch = Stopwatch.StartNew();
            var testResult = await this._testRunner.Run( new TestInput( this.TemplateText, this.TargetText ) );
            stopwatch.Stop();

            if ( testResult.AnnotatedSyntaxRoot != null )
            {
                // Display the annotated syntax tree.
                var document2 = testResult.TemplateDocument.WithSyntaxRoot( testResult.AnnotatedSyntaxRoot );
                var text2 = await document2.GetTextAsync();

                var marker = new TextSpanClassifier( text2, true );
                marker.Visit( await document2.GetSyntaxRootAsync() );
                var metaSpans = marker.ClassifiedTextSpans;

                this.ColoredTemplateDocument = await this._syntaxColorizer.WriteSyntaxColoring( text2, metaSpans );
            }

            if ( testResult.TransformedSyntaxRoot != null )
            {
                // Render the transformed tree.
                var project3 = this._testRunner.CreateProject();
                var document3 = project3.AddDocument( "name.cs", testResult.TransformedSyntaxRoot );
                var optionSet = (await document3.GetOptionsAsync()).WithChangedOption( FormattingOptions.IndentationSize, 4 );

                var formattedTransformedSyntaxRoot = Formatter.Format( testResult.TransformedSyntaxRoot, project3.Solution.Workspace, optionSet );
                var text4 = formattedTransformedSyntaxRoot.GetText( Encoding.UTF8 );
                var spanMarker = new TextSpanClassifier( text4, true );
                spanMarker.Visit( formattedTransformedSyntaxRoot );
                this.CompiledTemplateDocument = await this._syntaxColorizer.WriteSyntaxColoring( text4, spanMarker.ClassifiedTextSpans );
            }

            if ( testResult.TemplateOutputSource != null )
            {
                // Display the transformed code.
                this.TransformedTargetDocument = await this._syntaxColorizer.WriteSyntaxColoring( testResult.TemplateOutputSource, null );
            }

            var errorsTextBuilder = new StringBuilder();

            var errors = testResult.Diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error );
            foreach ( var e in errors )
            {
                errorsTextBuilder.AppendLine( e.Location + ":" + e.Id + " " + e.GetMessage() );
            }

            if ( !string.IsNullOrEmpty( testResult.TestErrorMessage ) )
            {
                errorsTextBuilder.AppendLine( testResult.TestErrorMessage );
            }

            errorsTextBuilder.AppendLine( $"It took {stopwatch.Elapsed.TotalSeconds:f1} s." );

            this.ErrorsText = errorsTextBuilder.ToString();
        }

        public void NewTest()
        {
            this.TemplateText = NewTestDefaults.TemplateSource;
            this.TargetText = NewTestDefaults.TargetSource;
            this.ExpectedOutputText = null;
            this.ColoredTemplateDocument = null;
            this.CompiledTemplateDocument = null;
            this.TransformedTargetDocument = null;
            this.CurrentPath = null;
        }

        public async Task LoadTestAsync( string filePath )
        {
            this._currentTest = await this._testSerializer.LoadFromFileAsync( filePath );

            var input = this._currentTest.Input ?? throw new InvalidOperationException( $"The {nameof( this._currentTest.Input )} property cannot be null." );

            this.TemplateText = input.TemplateSource;
            this.TargetText = input.TargetSource;
            this.ExpectedOutputText = this._currentTest.ExpectedOutput;
            this.ColoredTemplateDocument = null;
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

            if ( string.IsNullOrEmpty( this.TemplateText ) )
            {
                throw new InvalidOperationException( $"The {nameof( this.TargetText )} property cannot be null." );
            }

            if ( string.IsNullOrEmpty( this.TargetText ) )
            {
                throw new InvalidOperationException( $"The {nameof( this.TargetText )} property cannot be null." );
            }

            if ( this._currentTest == null )
            {
                this._currentTest = new TemplateTest();
            }

            this._currentTest.Input = new TestInput( this.TemplateText, this.TargetText );
            this._currentTest.ExpectedOutput = this.ExpectedOutputText ?? string.Empty;

            if ( !string.Equals( filePath, this.CurrentPath, StringComparison.Ordinal ) )
            {
                this._currentTest.OriginalSyntaxRoot = null;
            }

            this.CurrentPath = filePath;

            await this._testSerializer.SaveToFileAsync( this._currentTest, this.CurrentPath );
        }
    }
}
