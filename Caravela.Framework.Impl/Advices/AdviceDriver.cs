using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Advices
{
    sealed class AdviceDriver
    {
        public AdviceResult GetResult( ICompilation compilation, IAdvice advice )
        {
            return ((IAdviceImplementation) advice).ToResult( compilation );
        }
    }
}
