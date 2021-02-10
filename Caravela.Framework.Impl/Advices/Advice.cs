using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Advices
{
    internal abstract class Advice : IAdvice
    {
        protected Advice( ICodeElement targetDeclaration )
        {
            this.TargetDeclaration = targetDeclaration;
        }

        public ICodeElement TargetDeclaration { get; }
    }
}
