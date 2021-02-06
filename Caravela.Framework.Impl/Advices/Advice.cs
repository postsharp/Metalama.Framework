using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
{
    internal abstract class Advice : IAdvice
    {
        public IAspect Aspect { get; }

        public ICodeElement TargetDeclaration { get; }

        protected Advice( IAspect aspect, ICodeElement targetDeclaration )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = targetDeclaration;
        }
    }
}
