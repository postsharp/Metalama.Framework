using System.Collections.Immutable;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Advices
{

    internal sealed class IntroduceMethodAdvice : Advice, IIntroductionAdvice
    {
        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public IMethod TemplateMethod { get; }

        public IntroductionScope? Scope { get; set; }

        public string? Name { get; set; }

        public bool? IsStatic { get; set; }

        public bool? IsVirtual { get; set; }

        public Visibility? Visibility { get; set; }

        public IntroduceMethodAdvice( IAspect aspect, INamedType targetDeclaration, IMethod templateMethod ) : base( aspect, targetDeclaration )
        {
            this.TemplateMethod = templateMethod;
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            var introducedMethod = new IntroducedMethod( this, this.TargetDeclaration, this.TemplateMethod, this.Scope, this.Name, this.IsStatic, this.IsVirtual, this.Visibility );
            var overriddenMethod = new OverriddenMethod( this, introducedMethod, this.TemplateMethod );

            return new AdviceResult(
                ImmutableArray<Diagnostic>.Empty,
                ImmutableArray.Create<Transformation>(
                    introducedMethod,
                    overriddenMethod ) );
        }
    }
}
