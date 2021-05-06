// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using System;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroducePropertyAdvice : IntroduceMemberAdvice, IIntroducePropertyAdvice
    {
        private IProperty? templateProperty;
        private IMethod? getTemplateMethod;
        private IMethod? setTemplateMethod;

        public IPropertyBuilder Builder { get; }

        public new INamedType TargetDeclaration => base.TargetDeclaration!;

        public IntroducePropertyAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            IProperty? templateProperty,
            IMethod? getTemplateMethod,
            IMethod? setTemplateMethod,
            IntroductionScope scope,
            ConflictBehavior conflictBehavior,
            AspectLinkerOptions? linkerOptions )
            : base( aspect, targetDeclaration, scope, conflictBehavior, linkerOptions )
        {
            this.templateProperty = templateProperty;                 
            this.getTemplateMethod = getTemplateMethod;
            this.setTemplateMethod = setTemplateMethod;

            // TODO: Determine name from templates.

            this.Builder = new PropertyBuilder( this, this.TargetDeclaration, "foo", linkerOptions );
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            throw new NotImplementedException();
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            throw new NotImplementedException();
        }
    }
}
