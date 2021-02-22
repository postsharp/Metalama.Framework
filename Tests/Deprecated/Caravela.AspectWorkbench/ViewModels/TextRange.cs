using System.Collections.Generic;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.AspectWorkbench.ViewModels
{
    internal class TextRange
    {
        public ClassifiedSpan ClassifiedSpan { get; }

        public string Text { get; }

        public TextRange( string classification, TextSpan span, SourceText text ) :
            this( classification, span, text.GetSubText( span ).ToString() )
        {
        }

        public TextRange( string classification, TextSpan span, string text ) :
            this( new ClassifiedSpan( classification, span ), text )
        {
        }

        public TextRange( ClassifiedSpan classifiedSpan, string text )
        {
            this.ClassifiedSpan = classifiedSpan;
            this.Text = text;
        }

        public static IEnumerable<TextRange> FillGaps( SourceText text, IEnumerable<TextRange> ranges )
        {
            const string whitespaceClassification = "";
            var current = 0;
            TextRange? previous = null;

            foreach ( var range in ranges )
            {
                var start = range.TextSpan.Start;
                if ( start > current )
                {
                    yield return new TextRange( whitespaceClassification, TextSpan.FromBounds( current, start ), text );
                }

                if ( previous == null || range.TextSpan != previous.TextSpan )
                {
                    yield return range;
                }

                previous = range;
                current = range.TextSpan.End;
            }

            if ( current < text.Length )
            {
                yield return new TextRange( whitespaceClassification, TextSpan.FromBounds( current, text.Length ), text );
            }
        }

        public string ClassificationType => this.ClassifiedSpan.ClassificationType;

        public TextSpan TextSpan => this.ClassifiedSpan.TextSpan;

        public override string ToString() => this.ClassificationType ?? "null" + ":" + this.Text;
    }
}