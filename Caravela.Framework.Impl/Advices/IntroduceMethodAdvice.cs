// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Advices
{

    internal sealed class IntroduceMethodAdvice : Advice, IIntroduceMethodAdvice
    {
        private readonly MethodBuilder _methodBuilder;

        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public IMethod TemplateMethod { get; }

        public IntroduceMethodAdvice( AspectInstance aspect, INamedType targetDeclaration, IMethod templateMethod ) : base( aspect, targetDeclaration )
        {
            this.TemplateMethod = templateMethod;

            // TODO: Set name and all properties from the template.
            this._methodBuilder = new MethodBuilder( this, targetDeclaration, templateMethod.Name );
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            var overriddenMethod = new OverriddenMethod( this, this._methodBuilder, this.TemplateMethod );

            return new AdviceResult(
                ImmutableArray<Diagnostic>.Empty,
                ImmutableArray.Create<IObservableTransformation>( this._methodBuilder ),
                ImmutableArray.Create<INonObservableTransformation>( overriddenMethod ) );
        }

        public IMethodBuilder Builder => this._methodBuilder;
    }
}
