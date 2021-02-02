using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Caravela.Framework.DesignTime.Contracts
{
    public readonly struct ClassifiedTextSpan
    {
        public TextSpan Span { get; }
        public TextSpanClassification Classification { get; }

        public ClassifiedTextSpan( TextSpan span, TextSpanClassification classification )
        {
            this.Span = span;
            this.Classification = classification;
        }
    }

    public interface IReadOnlyClassifiedTextSpanCollection : IReadOnlyCollection<ClassifiedTextSpan>
    {
        TextSpanClassification GetCategory( in TextSpan textSpan );

        IEnumerable<ClassifiedTextSpan> GetClassifiedSpans( TextSpan textSpan );
    }
}