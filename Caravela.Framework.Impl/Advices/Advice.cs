using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl.Advices
{
    internal abstract class Advice : IAdvice
    {
        public AspectInstance Aspect { get; }

        IAspect IAdvice.Aspect => this.Aspect.Aspect;

        public ICodeElement TargetDeclaration { get; }

        public AspectLayerId AspectLayerId { get; }

        public string? LayerName { get; set; }

        protected Advice( AspectInstance aspect, ICodeElement targetDeclaration )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = targetDeclaration;
            this.AspectLayerId = new AspectLayerId( this.Aspect.AspectType, this.LayerName );
        }

        public abstract AdviceResult ToResult( ICompilation compilation );
    }
    
    
}
