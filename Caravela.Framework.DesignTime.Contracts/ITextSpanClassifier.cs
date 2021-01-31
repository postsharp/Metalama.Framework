using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Caravela.Framework.DesignTime.Contracts
{
    public interface ITextSpanClassifier
    {
        TextSpanCategory GetCategory( in TextSpan textSpan );

        IEnumerable<(TextSpan span, TextSpanCategory category)> GetClassifiedSpans( TextSpan textSpan );
    }
}