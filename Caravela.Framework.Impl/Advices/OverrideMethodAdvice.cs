using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Advices
{
    class OverrideMethodAdvice : Advice, IOverrideMethodAdvice, IAdviceImplementation
    {
        public IMethod TemplateMethod { get; }
        public new IMethod TargetDeclaration => (IMethod) base.TargetDeclaration;

        public OverrideMethodAdvice( IAspect aspect, IMethod targetDeclaration, IMethod templateMethod ) : base( aspect, targetDeclaration )
        {
            this.TemplateMethod = templateMethod;
        }

        public AdviceResult ToResult( ICompilation compilation )
        {
            return new AdviceResult(
                ImmutableArray<Diagnostic>.Empty,
                ImmutableArray.Create<Transformation>(
                    new OverriddenMethod( this, this.TargetDeclaration, this.TemplateMethod )
                    )
                );
        }
    }
}
