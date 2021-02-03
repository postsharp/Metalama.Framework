using Microsoft.CodeAnalysis.Text;
using System.Runtime.InteropServices;

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
}