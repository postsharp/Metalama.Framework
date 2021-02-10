using Caravela.Framework.Advices;

namespace Caravela.Framework.Impl.Advices
{
    internal readonly struct AdviceInstance
    {
        public IAdvice Advice { get; }

        public AdviceInstance( IAdvice advice )
        {
            this.Advice = advice;
        }
    }
}