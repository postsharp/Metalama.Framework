using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.AspectWorkbench
{
    public partial class MainWindow
    {
        private WorkbenchTestRunner testRunner;

        public MainWindow()
        {
            this.InitializeComponent();

            testRunner = new WorkbenchTestRunner( this.errorsTextBlock );

            #region Initial test source

            this.sourceTextBox.Text = @"  
// Don't rename classes, methods, neither remove namespaces. Many things are hardcoded.  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.TestFramework.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Caravela.Framework.Impl.Templating.TemplateHelper;

class Aspect
{
  [Template]
  dynamic Template()
  {
    var parameters = new object[AdviceContext.Method.Parameters.Count];
    var stringBuilder = new StringBuilder();
    AdviceContext.BuildTime( stringBuilder );
    stringBuilder.Append(AdviceContext.Method.Name);
    stringBuilder.Append('(');
    int i = 0;
    foreach ( var p in AdviceContext.Method.Parameters )
    {
       string comma = i > 0 ? "", "" : """";

        if ( p.IsOut )
        {
            stringBuilder.Append( $""{comma}{p.Name} = <out>"" );
        }
        else
        {
            stringBuilder.Append( $""{comma}{p.Name} = {{{i}}}"" );
            parameters[i] = p.Value;
        }

        i++;
    }
    stringBuilder.Append(')');

    Console.WriteLine( stringBuilder.ToString(), parameters );

    try
    {
        dynamic result = AdviceContext.Proceed();
        Console.WriteLine( stringBuilder + "" returned "" + result, parameters );
        return result;
    }
    catch ( Exception _e )
    {
        Console.WriteLine( stringBuilder + "" failed: "" + _e, parameters );
        throw;
    }
  }
}

class TargetCode
{
    int Method(int a, int b )
    {
        return a + b;
    }
}

";
            #endregion
        }

        private static readonly Dictionary<string, Color> classificationToColor = new Dictionary<string, Color>
        {
            {ClassificationTypeNames.Comment, Colors.Green},
            {ClassificationTypeNames.ExcludedCode, Colors.Black},
            {ClassificationTypeNames.Identifier, Colors.Black},
            {ClassificationTypeNames.Keyword, Colors.Red},
            {ClassificationTypeNames.ControlKeyword, Colors.Black},
            {ClassificationTypeNames.NumericLiteral, Colors.Black},
            {ClassificationTypeNames.Operator, Colors.Black},
            {ClassificationTypeNames.OperatorOverloaded, Colors.Black},
            {ClassificationTypeNames.PreprocessorKeyword, Colors.Black},
            {ClassificationTypeNames.StringLiteral, Colors.Blue},
            {ClassificationTypeNames.WhiteSpace, Colors.Black},
            {ClassificationTypeNames.Text, Colors.Black},
            {ClassificationTypeNames.StaticSymbol, Colors.Black},
            {ClassificationTypeNames.PreprocessorText, Colors.Gray},
            {ClassificationTypeNames.Punctuation, Colors.Black},
            {ClassificationTypeNames.VerbatimStringLiteral, Colors.Blue},
            {ClassificationTypeNames.StringEscapeCharacter, Colors.LightSeaGreen},
            {ClassificationTypeNames.ClassName, Colors.Purple},
            {ClassificationTypeNames.DelegateName, Colors.Purple},
            {ClassificationTypeNames.EnumName, Colors.Purple},
            {ClassificationTypeNames.InterfaceName, Colors.Purple},
            {ClassificationTypeNames.ModuleName, Colors.Black},
            {ClassificationTypeNames.StructName, Colors.Purple},
            {ClassificationTypeNames.TypeParameterName, Colors.HotPink},
            {ClassificationTypeNames.FieldName, Colors.Teal},
            {ClassificationTypeNames.EnumMemberName, Colors.Teal},
            {ClassificationTypeNames.ConstantName, Colors.Teal},
            {ClassificationTypeNames.LocalName, Colors.Brown},
            {ClassificationTypeNames.ParameterName, Colors.Brown},
            {ClassificationTypeNames.MethodName, Colors.Teal},
            {ClassificationTypeNames.ExtensionMethodName, Colors.Chartreuse},
            {ClassificationTypeNames.PropertyName, Colors.Teal},
            {ClassificationTypeNames.EventName, Colors.Teal},
            {ClassificationTypeNames.NamespaceName, Colors.Black},
            {ClassificationTypeNames.LabelName, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentAttributeName, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentAttributeQuotes, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentAttributeValue, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentCDataSection, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentComment, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentDelimiter, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentEntityReference, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentName, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentProcessingInstruction, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentText, Colors.Black},
            {ClassificationTypeNames.XmlLiteralAttributeName, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralAttributeQuotes, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralAttributeValue, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralCDataSection, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralComment, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralDelimiter, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralEmbeddedExpression, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralEntityReference, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralName, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralProcessingInstruction, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralText, Colors.Gray},
            {ClassificationTypeNames.RegexComment, Colors.Indigo},
            {ClassificationTypeNames.RegexCharacterClass, Colors.Indigo},
            {ClassificationTypeNames.RegexAnchor, Colors.Indigo},
            {ClassificationTypeNames.RegexQuantifier, Colors.Indigo},
            {ClassificationTypeNames.RegexGrouping, Colors.Indigo},
            {ClassificationTypeNames.RegexAlternation, Colors.Indigo},
            {ClassificationTypeNames.RegexText, Colors.Indigo},
            {ClassificationTypeNames.RegexSelfEscapedCharacter, Colors.Indigo},
            {ClassificationTypeNames.RegexOtherEscape, Colors.Indigo},
        };


        private async void OnClick( object sender, RoutedEventArgs e )
        {
            var stopwatch = Stopwatch.StartNew();

            this.errorsTextBlock.Text = "";

            var testResult = await testRunner.Run( this.sourceTextBox.Text );

            if ( testResult.AnnotatedSyntaxRoot != null )
            {
                // Display the annotated syntax tree.
                var document2 = testResult.InputDocument.WithSyntaxRoot( testResult.AnnotatedSyntaxRoot );
                var text2 = await document2.GetTextAsync();

                var marker = new CompileTimeTextSpanMarker( text2 );
                marker.Visit( await document2.GetSyntaxRootAsync() );
                var metaSpans = marker.GetMarkedSpans();

                await WriteSyntaxColoring( text2, metaSpans, this.highlightedSourceRichBox );
            }

            if ( testResult.TransformedSyntaxRoot != null )
            {
                // Render the transformed tree.
                var project3 = testRunner.CreateProject();
                var document3 = project3.AddDocument( "name.cs", testResult.TransformedSyntaxRoot );
                var optionSet = (await document3.GetOptionsAsync()).WithChangedOption( FormattingOptions.IndentationSize, 4 );

                var formattedTransformedSyntaxRoot = Formatter.Format( testResult.TransformedSyntaxRoot, project3.Solution.Workspace, optionSet );
                var text4 = formattedTransformedSyntaxRoot.GetText( Encoding.UTF8 );
                var spanMarker = new CompileTimeTextSpanMarker( text4 );
                spanMarker.Visit( formattedTransformedSyntaxRoot );
                await WriteSyntaxColoring( text4, spanMarker.GetMarkedSpans(), this.compiledTemplateRichBox );
            }

            if ( testResult.TemplateOutputSource != null )
            {
                // Display the transformed code.
                await WriteSyntaxColoring( testResult.TemplateOutputSource, null, this.transformedCodeRichBox );
            }

            if ( !string.IsNullOrEmpty( testResult.TestErrorMessage ) )
            {
                this.errorsTextBlock.Text += Environment.NewLine + testResult.TestErrorMessage;
            }
            this.errorsTextBlock.Text += Environment.NewLine + $"It took {stopwatch.Elapsed.TotalSeconds:f1} s.";
        }

        private void ReportDiagnostics( IReadOnlyList<Diagnostic> diagnostics )
        {
            diagnostics = diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).ToList();

            if ( diagnostics.Count > 0 )
            {
                if ( this.errorsTextBlock.Text.Length > 0 )
                {
                    this.errorsTextBlock.Text += Environment.NewLine;
                }

                this.errorsTextBlock.Text += string.Join( Environment.NewLine,
                    diagnostics.Select( d => d.Location + ":" + d.Id + " " + d.GetMessage() ) );
            }
        }

        private async Task WriteSyntaxColoring( SourceText text, ImmutableList<TextSpan> metaSpans, RichTextBox richTextBox )
        {

            var project = testRunner.CreateProject();
            var document = project.AddDocument( "name.cs", text.ToString() );

            var classifiedSpans =
                await Classifier.GetClassifiedSpansAsync( document, TextSpan.FromBounds( 0, text.Length ) );

            var ranges = classifiedSpans.Select( classifiedSpan =>
                 new TextRange( classifiedSpan, text.GetSubText( classifiedSpan.TextSpan ).ToString() ) );

            ranges = TextRange.FillGaps( text, ranges );

            var paragraph = new Paragraph();

            foreach ( var range in ranges )
            {
                Color color;

                if ( range.ClassificationType == null || !classificationToColor.TryGetValue( range.ClassificationType, out color ) )
                {
                    color = Colors.Black;
                }

                var isConstant = metaSpans != null && metaSpans.Any( s => s.Contains( range.TextSpan ) );

                var run = new Run( range.Text )
                {
                    Foreground = new SolidColorBrush( color ),
                    Background = isConstant ? Brushes.Yellow : Brushes.White,
                    ToolTip = range.ClassificationType
                };
                ToolTipService.SetShowOnDisabled( run, true );
                paragraph.Inlines.Add( run );
            }


            richTextBox.Document = new FlowDocument( paragraph ) { PageWidth = 2000 };

            // This is how to enable wrapping on the document:
            // richTextBox.Document.SetBinding( FlowDocument.PageWidthProperty, 
            //    new Binding( "ActualWidth" ) { Source = richTextBox } );

        }
    }
}