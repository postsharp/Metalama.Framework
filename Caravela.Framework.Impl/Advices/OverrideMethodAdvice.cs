using System.Collections.Immutable;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Advices
{
    internal class OverrideMethodAdvice : Advice, IOverrideMethodAdvice
    {
        public IMethod TemplateMethod { get; }

        public new IMethod TargetDeclaration => (IMethod) base.TargetDeclaration;

        public OverrideMethodAdvice( AspectInstance aspect, IMethod targetDeclaration, IMethod templateMethod ) : base( aspect, targetDeclaration )
        {
            this.TemplateMethod = templateMethod;
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            return new AdviceResult(
                ImmutableArray<Diagnostic>.Empty,
                ImmutableArray.Create<Transformation>(
                    new OverriddenMethod( this, this.TargetDeclaration, this.TemplateMethod ) ) );
        }
    }
}
