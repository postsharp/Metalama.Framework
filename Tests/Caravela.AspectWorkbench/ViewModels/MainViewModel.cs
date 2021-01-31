using Caravela.AspectWorkbench.Model;
using Caravela.Framework.Impl.Templating;
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

namespace Caravela.AspectWorkbench.ViewModels
{
    [NotifyPropertyChanged]
    public class MainViewModel
    {
        private readonly WorkbenchTestRunner testRunner;
        private readonly SyntaxColorizer syntaxColorizer;
        private readonly TestSerializer testSerializer;
        private TemplateTest currentTest;

        public MainViewModel()
        {
            this.testRunner = new WorkbenchTestRunner();
            this.syntaxColorizer = new SyntaxColorizer( this.testRunner );
            this.testSerializer = new TestSerializer();
        }

        
        public string Title => this.CurrentPath == null ? "Aspect Workbench" : $"Aspect Workbench - {this.CurrentPath}";

        public string TemplateText { get; set; }

        public string TargetText { get; set; }

        public string ExpectedOutputText { get; set; }

        public FlowDocument ColoredTemplateDocument { get; set; }

        public FlowDocument CompiledTemplateDocument { get; set; }

        public FlowDocument TransformedTargetDocument { get; set; }

        public string ErrorsText { get; set; }

        public bool IsNewTest => string.IsNullOrEmpty( this.CurrentPath );

        private string CurrentPath { get; set; }


        public async Task RunTestAsync()
        {
            this.ErrorsText = string.Empty;
            this.TransformedTargetDocument = null;

            var stopwatch = Stopwatch.StartNew();
            var testResult = await this.testRunner.Run( new TestInput( this.TemplateText, this.TargetText ) );
            stopwatch.Stop();

            if ( testResult.AnnotatedSyntaxRoot != null )
            {
                // Display the annotated syntax tree.
                var document2 = testResult.TemplateDocument.WithSyntaxRoot( testResult.AnnotatedSyntaxRoot );
                var text2 = await document2.GetTextAsync();

                var marker = new CompileTimeTextSpanMarker( text2 );
                marker.Visit( await document2.GetSyntaxRootAsync() );
                var metaSpans = marker.Classifier;

                this.ColoredTemplateDocument = await this.syntaxColorizer.WriteSyntaxColoring( text2, metaSpans );
            }

            if ( testResult.TransformedSyntaxRoot != null )
            {
                // Render the transformed tree.
                var project3 = this.testRunner.CreateProject();
                var document3 = project3.AddDocument( "name.cs", testResult.TransformedSyntaxRoot );
                var optionSet = (await document3.GetOptionsAsync()).WithChangedOption( FormattingOptions.IndentationSize, 4 );

                var formattedTransformedSyntaxRoot = Formatter.Format( testResult.TransformedSyntaxRoot, project3.Solution.Workspace, optionSet );
                var text4 = formattedTransformedSyntaxRoot.GetText( Encoding.UTF8 );
                var spanMarker = new CompileTimeTextSpanMarker( text4 );
                spanMarker.Visit( formattedTransformedSyntaxRoot );
                this.CompiledTemplateDocument = await this.syntaxColorizer.WriteSyntaxColoring( text4, spanMarker.Classifier );
            }

            if ( testResult.TemplateOutputSource != null )
            {
                // Display the transformed code.
                this.TransformedTargetDocument = await this.syntaxColorizer.WriteSyntaxColoring( testResult.TemplateOutputSource, null );
            }

            StringBuilder errorsTextBuilder = new StringBuilder();

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
            this.currentTest = await this.testSerializer.LoadFromFileAsync( filePath );
            this.TemplateText = this.currentTest.Input.TemplateSource;
            this.TargetText = this.currentTest.Input.TargetSource;
            this.ExpectedOutputText = this.currentTest.ExpectedOutput;
            this.ColoredTemplateDocument = null;
            this.CompiledTemplateDocument = null;
            this.TransformedTargetDocument = null;
            this.CurrentPath = filePath;
        }

        public async Task SaveTestAsync( string filePath )
        {
            filePath ??= this.CurrentPath;

            if ( string.IsNullOrEmpty( filePath ) )
            {
                throw new ArgumentException( "The path cannot be null or empty." );
            }

            if ( this.currentTest == null )
            {
                this.currentTest = new TemplateTest();
            }

            this.currentTest.Input = new TestInput( this.TemplateText, this.TargetText );
            this.currentTest.ExpectedOutput = this.ExpectedOutputText ?? string.Empty;

            if ( !string.Equals( filePath, this.CurrentPath ) )
            {
                this.currentTest.OriginalSyntaxRoot = null;
            }

            this.CurrentPath = filePath;

            await this.testSerializer.SaveToFileAsync( this.currentTest, this.CurrentPath );
        }
    }
}
