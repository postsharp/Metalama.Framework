using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Caravela.Framework.DesignTime.Contracts
{
    // The type identifier cannot be modified even during refactoring. 


    // The type identifier cannot be modified even during refactoring. 
    [Guid("da58deff-93d5-4d5a-bf6e-11df8bdbd74d")]
    public interface IReadOnlyClassifiedTextSpanCollection : IReadOnlyCollection<ClassifiedTextSpan>
    {
        TextSpanClassification GetCategory( in TextSpan textSpan );

        IEnumerable<ClassifiedTextSpan> GetClassifiedSpans( TextSpan textSpan );
    }
}