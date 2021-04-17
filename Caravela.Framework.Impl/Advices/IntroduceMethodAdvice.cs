// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Advices
{
    internal sealed class IntroduceMethodAdvice : Advice, IIntroduceMethodAdvice
    {
        private readonly MethodBuilder _methodBuilder;

        public IMethod TemplateMethod { get; }

        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public AspectLinkerOptions? LinkerOptions { get; }

        public IntroduceMethodAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            IMethod templateMethod,
            AspectLinkerOptions? linkerOptions = null ) : base( aspect, targetDeclaration )
        {
            this.TemplateMethod = templateMethod;
            this.LinkerOptions = linkerOptions;

            // TODO: Set name and all properties from the template.
            this._methodBuilder = new MethodBuilder( this, targetDeclaration, templateMethod.Name, this.LinkerOptions );

            this._methodBuilder.Accessibility = templateMethod.Accessibility;
            this._methodBuilder.IsStatic = templateMethod.IsStatic;
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            var overriddenMethod = new OverriddenMethod( this, this._methodBuilder, this.TemplateMethod, this.LinkerOptions );

            return new AdviceResult(
                ImmutableArray<Diagnostic>.Empty,
                ImmutableArray.Create<IObservableTransformation>( this._methodBuilder ),
                ImmutableArray.Create<INonObservableTransformation>( overriddenMethod ) );
        }

        public IMethodBuilder Builder => this._methodBuilder;
    }
}