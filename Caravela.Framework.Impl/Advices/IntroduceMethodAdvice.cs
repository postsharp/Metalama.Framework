using Caravela.Framework.Advices;
using System.Collections.Immutable;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Advices
{

    internal sealed class IntroduceIntroduceMethodAdvice : Advice, IIntroduceMethodAdvice
    {
        private readonly MethodBuilder _methodBuilder;
        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public IMethod TemplateMethod { get; }

        public IntroduceIntroduceMethodAdvice( AspectInstance aspect, INamedType targetDeclaration, IMethod templateMethod ) : base( aspect, targetDeclaration )
        {
            this.TemplateMethod = templateMethod;
            
            // TODO: Set name and all properties from the template.
            this._methodBuilder = new MethodBuilder( targetDeclaration, templateMethod, templateMethod.Name );
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            var overriddenMethod = new OverriddenMethod( this, this._methodBuilder, this.TemplateMethod );

            return new AdviceResult(
                ImmutableArray<Diagnostic>.Empty,
                ImmutableArray.Create<IIntroducedElement>( this._methodBuilder ),
                ImmutableArray.Create<Transformation>( overriddenMethod ) );
        }

        public IMethodBuilder Builder => this._methodBuilder;
    }
}
