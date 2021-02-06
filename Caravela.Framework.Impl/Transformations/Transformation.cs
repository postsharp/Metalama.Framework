using Caravela.Framework.Advices;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class Transformation
    {
        public IAdvice Advice { get; }

        public Transformation( IAdvice advice )
        {
            this.Advice = advice;
        }
    }
}
