// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroduceEventAdvice : IntroduceMemberAdvice<EventBuilder>
    {
        public IEventBuilder Builder => this.MemberBuilder;

        public IntroduceEventAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            string name,
            IMethod? addTemplateMethod,
            IMethod? removeTemplateMethod,
            IntroductionScope scope,
            ConflictBehavior conflictBehavior,
            string layerName,
            AdviceOptions? options )
            : base( aspect, targetDeclaration, null, scope, conflictBehavior, layerName, options ) { }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( declarativeAdvices, diagnosticAdder );
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            throw new NotImplementedException();
        }
    }
}