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
    internal class IntroduceFieldAdvice : IntroduceMemberAdvice, IIntroduceFieldAdvice
    {
        public IFieldBuilder Builder { get; }

        public new INamedType TargetDeclaration => base.TargetDeclaration!;

        public IntroduceFieldAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            string name,
            IntroductionScope scope,
            ConflictBehavior conflictBehavior,
            AspectLinkerOptions? linkerOptions )
            : base(aspect, targetDeclaration, scope, conflictBehavior, linkerOptions)
        {
            this.Builder = new FieldBuilder( this, this.TargetDeclaration, name, linkerOptions );
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
