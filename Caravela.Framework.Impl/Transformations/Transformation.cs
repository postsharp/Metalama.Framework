using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{
    abstract class Transformation
    {
        public IAdvice Advice { get; }

        public Transformation(IAdvice advice)
        {
            this.Advice = advice;
        }
    }
}
