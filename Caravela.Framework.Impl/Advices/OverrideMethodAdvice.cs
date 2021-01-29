using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Advices
{
    abstract class Advice : IAdvice
    {
        protected Advice( ICodeElement targetDeclaration )
        {
            this.TargetDeclaration = targetDeclaration;
        }

        public ICodeElement TargetDeclaration { get; }
    }
    
    class OverrideMethodAdvice : Advice, IOverrideMethodAdvice
    {
        public new IMethod TargetDeclaration => (IMethod) base.TargetDeclaration;
        public OverriddenMethod Transformation { get; }

        public OverrideMethodAdvice( IMethod targetDeclaration, OverriddenMethod transformation ) : base(targetDeclaration)
        {
            this.Transformation = transformation;
        }
        
    }
}
