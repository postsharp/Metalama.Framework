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
        private readonly MethodTransformationBuilder _methodTransformationBuilder;
        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public IMethod TemplateMethod { get; }

        public IntroduceIntroduceMethodAdvice( AspectInstance aspect, INamedType targetDeclaration, IMethod templateMethod ) : base( aspect, targetDeclaration )
        {
            this.TemplateMethod = templateMethod;
            
            // TODO: Set name and all properties from the template.
            this._methodTransformationBuilder = new MethodTransformationBuilder( targetDeclaration, templateMethod, templateMethod.Name );
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            var overriddenMethod = new OverriddenMethod( this, this._methodTransformationBuilder, this.TemplateMethod );

            return new AdviceResult(
                ImmutableArray<Diagnostic>.Empty,
                ImmutableArray.Create<IObservableTransformation>( this._methodTransformationBuilder ),
                ImmutableArray.Create<INonObservableTransformation>( overriddenMethod ) );
        }

        public IMethodBuilder Builder => this._methodTransformationBuilder;
    }
}
