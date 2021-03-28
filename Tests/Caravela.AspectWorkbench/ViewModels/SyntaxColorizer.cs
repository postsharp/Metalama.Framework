using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Caravela.AspectWorkbench.Model;
using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Tests.Integration.Highlighting;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.AspectWorkbench.ViewModels
{
    internal class SyntaxColorizer
    {
        private static readonly Dictionary<string, Color> _classificationToColor = new Dictionary<string, Color>
        {
            { ClassificationTypeNames.Comment, Colors.Green },
            { ClassificationTypeNames.ExcludedCode, Colors.Black },
            { ClassificationTypeNames.Identifier, Colors.Black },
            { ClassificationTypeNames.Keyword, Colors.Blue },
            { ClassificationTypeNames.ControlKeyword, Colors.Blue },
            { ClassificationTypeNames.NumericLiteral, Colors.Black },
            { ClassificationTypeNames.Operator, Colors.Black },
            { ClassificationTypeNames.OperatorOverloaded, Colors.Black },
            { ClassificationTypeNames.PreprocessorKeyword, Colors.Black },
            { ClassificationTypeNames.StringLiteral, Colors.Brown },
            { ClassificationTypeNames.WhiteSpace, Colors.Black },
            { ClassificationTypeNames.Text, Colors.Black },
            { ClassificationTypeNames.StaticSymbol, Colors.Black },
            { ClassificationTypeNames.PreprocessorText, Colors.Gray },
            { ClassificationTypeNames.Punctuation, Colors.Black },
            { ClassificationTypeNames.VerbatimStringLiteral, Colors.Blue },
            { ClassificationTypeNames.StringEscapeCharacter, Colors.LightSeaGreen },
            { ClassificationTypeNames.ClassName, Colors.Teal },
            { ClassificationTypeNames.DelegateName, Colors.Teal },
            { ClassificationTypeNames.EnumName, Colors.Teal },
            { ClassificationTypeNames.InterfaceName, Colors.Teal },
            { ClassificationTypeNames.ModuleName, Colors.Black },
            { ClassificationTypeNames.StructName, Colors.Teal },
            { ClassificationTypeNames.TypeParameterName, Colors.Black },
            { ClassificationTypeNames.FieldName, Colors.Teal },
            { ClassificationTypeNames.EnumMemberName, Colors.Black },
            { ClassificationTypeNames.ConstantName, Colors.Black },
            { ClassificationTypeNames.LocalName, Colors.Black },
            { ClassificationTypeNames.ParameterName, Colors.Brown },
            { ClassificationTypeNames.MethodName, Colors.Teal },
            { ClassificationTypeNames.ExtensionMethodName, Colors.Teal },
            { ClassificationTypeNames.PropertyName, Colors.Black },
            { ClassificationTypeNames.EventName, Colors.Black },
            { ClassificationTypeNames.NamespaceName, Colors.Black },
            { ClassificationTypeNames.LabelName, Colors.Black },
            { ClassificationTypeNames.XmlDocCommentAttributeName, Colors.Black },
            { ClassificationTypeNames.XmlDocCommentAttributeQuotes, Colors.Black },
            { ClassificationTypeNames.XmlDocCommentAttributeValue, Colors.Black },
            { ClassificationTypeNames.XmlDocCommentCDataSection, Colors.Black },
            { ClassificationTypeNames.XmlDocCommentComment, Colors.Black },
            { ClassificationTypeNames.XmlDocCommentDelimiter, Colors.Black },
            { ClassificationTypeNames.XmlDocCommentEntityReference, Colors.Black },
            { ClassificationTypeNames.XmlDocCommentName, Colors.Black },
            { ClassificationTypeNames.XmlDocCommentProcessingInstruction, Colors.Black },
            { ClassificationTypeNames.XmlDocCommentText, Colors.Black },
            { ClassificationTypeNames.XmlLiteralAttributeName, Colors.Gray },
            { ClassificationTypeNames.XmlLiteralAttributeQuotes, Colors.Gray },
            { ClassificationTypeNames.XmlLiteralAttributeValue, Colors.Gray },
            { ClassificationTypeNames.XmlLiteralCDataSection, Colors.Gray },
            { ClassificationTypeNames.XmlLiteralComment, Colors.Gray },
            { ClassificationTypeNames.XmlLiteralDelimiter, Colors.Gray },
            { ClassificationTypeNames.XmlLiteralEmbeddedExpression, Colors.Gray },
            { ClassificationTypeNames.XmlLiteralEntityReference, Colors.Gray },
            { ClassificationTypeNames.XmlLiteralName, Colors.Gray },
            { ClassificationTypeNames.XmlLiteralProcessingInstruction, Colors.Gray },
            { ClassificationTypeNames.XmlLiteralText, Colors.Gray },
            { ClassificationTypeNames.RegexComment, Colors.Indigo },
            { ClassificationTypeNames.RegexCharacterClass, Colors.Indigo },
            { ClassificationTypeNames.RegexAnchor, Colors.Indigo },
            { ClassificationTypeNames.RegexQuantifier, Colors.Indigo },
            { ClassificationTypeNames.RegexGrouping, Colors.Indigo },
            { ClassificationTypeNames.RegexAlternation, Colors.Indigo },
            { ClassificationTypeNames.RegexText, Colors.Indigo },
            { ClassificationTypeNames.RegexSelfEscapedCharacter, Colors.Indigo },
            { ClassificationTypeNames.RegexOtherEscape, Colors.Indigo },
        };

        private readonly WorkbenchHighlightingTestRunner _testRunner;

        public SyntaxColorizer( WorkbenchHighlightingTestRunner testRunner )
        {
            this._testRunner = testRunner;
        }

        public async Task<FlowDocument> WriteSyntaxColoring( SourceText text, IReadOnlyClassifiedTextSpanCollection? compileTimeSpans )
        {
            static Color WithAlpha( Color brush, double alpha )
            {
                return Color.FromArgb( (byte) (255 * alpha), brush.R, brush.G, brush.B );
            }

            var project = _testRunner.CreateProject();
            var document = project.AddDocument( "name.cs", text.ToString() );

            var roslynClassifiedSpans =
                await Classifier.GetClassifiedSpansAsync( document, TextSpan.FromBounds( 0, text.Length ) );

            var ranges = roslynClassifiedSpans.Select( classifiedSpan =>
                 new TextRange( classifiedSpan, text.GetSubText( classifiedSpan.TextSpan ).ToString() ) );

            ranges = TextRange.FillGaps( text, ranges );

            var paragraph = new Paragraph();

            foreach ( var range in ranges )
            {
                Color foreground, background;
                FontWeight fontWeight;
                var category = compileTimeSpans?.GetCategory( range.TextSpan ) ?? TextSpanClassification.Default;

                // Choose foreground.
                switch ( category )
                {
                    case TextSpanClassification.TemplateKeyword:
                    case TextSpanClassification.CompileTimeVariable:
                    case TextSpanClassification.Dynamic:
                        foreground = Colors.Fuchsia;
                        fontWeight = FontWeights.Heavy;
                        break;

                    default:
                        if ( string.IsNullOrWhiteSpace( range.Text ) )
                        {
                            foreground = Colors.Black;
                        }
                        else if ( range.ClassificationType == null || !_classificationToColor.TryGetValue( range.ClassificationType, out foreground ) )
                        {
                            // A good point to have a breakpoint.
                            foreground = Colors.Green;
                        }

                        fontWeight = FontWeights.Normal;
                        break;
                }

                // Choose background.
                switch ( category )
                {
                    case TextSpanClassification.TemplateKeyword:
                    case TextSpanClassification.CompileTimeVariable:
                    case TextSpanClassification.CompileTime:
                    case TextSpanClassification.Dynamic:
                        background = WithAlpha( Colors.SlateGray, 0.2 );
                        break;

                    default:
                        background = Colors.White;
                        break;
                }

                var run = new Run( range.Text )
                {
                    Foreground = new SolidColorBrush( foreground ),
                    Background = new SolidColorBrush( background ),
                    ToolTip = range.ClassificationType,
                    FontWeight = fontWeight
                };
                ToolTipService.SetShowOnDisabled( run, true );
                paragraph.Inlines.Add( run );
            }

            return new FlowDocument( paragraph ) { PageWidth = 2000 };

            // This is how to enable wrapping on the document:
            // richTextBox.Document.SetBinding( FlowDocument.PageWidthProperty,
            //    new Binding( "ActualWidth" ) { Source = richTextBox } );
        }
    }
}
