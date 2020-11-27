using Caravela.AspectWorkbench.Model;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using PostSharp.Patterns.Model;
using System;
using System.Diagnostics;
using System.IO;
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
        private string currentPath;

        public MainViewModel()
        {
            this.testRunner = new WorkbenchTestRunner();
            this.syntaxColorizer = new SyntaxColorizer( this.testRunner );
            this.testSerializer = new TestSerializer();
        }

        public string TemplateText { get; set; }

        public string TargetText { get; set; }

        public string ExpectedOutputText { get; set; }

        public FlowDocument ColoredTemplateDocument { get; set; }

        public FlowDocument CompiledTemplateDocument { get; set; }

        public FlowDocument TransformedTargetDocument { get; set; }

        public string ErrorsText { get; set; }

        public bool IsUnsaved => string.IsNullOrEmpty( this.currentPath );

        public async Task RunTestAsync()
        {
            this.ErrorsText = string.Empty;

            var stopwatch = Stopwatch.StartNew();
            var testResult = await this.testRunner.Run( new TestInput( this.TemplateText, this.TargetText ) );
            stopwatch.Stop();

            if ( testResult.AnnotatedSyntaxRoot != null )
            {
                // Display the annotated syntax tree.
                var document2 = testResult.InputDocument.WithSyntaxRoot( testResult.AnnotatedSyntaxRoot );
                var text2 = await document2.GetTextAsync();

                var marker = new CompileTimeTextSpanMarker( text2 );
                marker.Visit( await document2.GetSyntaxRootAsync() );
                var metaSpans = marker.GetMarkedSpans();

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
                this.CompiledTemplateDocument = await this.syntaxColorizer.WriteSyntaxColoring( text4, spanMarker.GetMarkedSpans() );
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
            this.currentPath = filePath;
        }

        public async Task SaveTestAsync( string filePath )
        {
            filePath ??= this.currentPath;

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

            if ( !string.Equals( filePath, this.currentPath ) )
            {
                this.currentTest.OriginalSyntaxRoot = null;
            }

            this.currentPath = filePath;

            await this.testSerializer.SaveToFileAsync( this.currentTest, this.currentPath );
        }
    }
}
