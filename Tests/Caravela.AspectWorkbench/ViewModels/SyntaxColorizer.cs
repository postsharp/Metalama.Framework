using Caravela.AspectWorkbench.Model;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Caravela.AspectWorkbench.ViewModels
{
    class SyntaxColorizer
    {
        private readonly WorkbenchTestRunner testRunner;

        public SyntaxColorizer( WorkbenchTestRunner testRunner )
        {
            this.testRunner = testRunner;
        }

        #region Colors

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

        #endregion

        public async Task<FlowDocument> WriteSyntaxColoring( SourceText text, ImmutableList<TextSpan> metaSpans )
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

            return new FlowDocument( paragraph ) { PageWidth = 2000 };

            // This is how to enable wrapping on the document:
            // richTextBox.Document.SetBinding( FlowDocument.PageWidthProperty, 
            //    new Binding( "ActualWidth" ) { Source = richTextBox } );
        }
    }
}
