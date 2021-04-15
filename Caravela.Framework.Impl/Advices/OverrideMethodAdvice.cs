// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Advices
{
    internal class OverrideMethodAdvice : Advice, IOverrideMethodAdvice
    {
        public IMethod TemplateMethod { get; }

        public AspectLinkerOptions? LinkerOptions { get; }

        public new IMethod TargetDeclaration => (IMethod) base.TargetDeclaration;

        public OverrideMethodAdvice( AspectInstance aspect, IMethod targetDeclaration, IMethod templateMethod, AspectLinkerOptions? linkerOptions = null ) : base( aspect, targetDeclaration )
        {
            this.TemplateMethod = templateMethod;
            this.LinkerOptions = linkerOptions;
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            return AdviceResult.Create( new OverriddenMethod( this, this.TargetDeclaration, this.TemplateMethod, this.LinkerOptions ) );
        }
    }
}
