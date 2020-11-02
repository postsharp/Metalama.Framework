using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Advices
{
    class OverrideMethodAdvice : IAdvice<IMethod>
    {
        public IMethod TargetDeclaration { get; }
        public OverriddenMethod Transformation { get; }

        public OverrideMethodAdvice( IMethod targetDeclaration, OverriddenMethod transformation )
        {
            this.TargetDeclaration = targetDeclaration;
            this.Transformation = transformation;
        }
    }
}
