using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Advices
{

    internal class OverrideMethodAdvice : Advice, IOverrideMethodAdvice
    {
        public new IMethod TargetDeclaration => (IMethod) base.TargetDeclaration;

        public OverriddenMethod Transformation { get; }

        public OverrideMethodAdvice( IMethod targetDeclaration, OverriddenMethod transformation ) : base( targetDeclaration )
        {
            this.Transformation = transformation;
        }
    }
}
