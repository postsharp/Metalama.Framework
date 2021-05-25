// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroduceEventAdvice : IntroduceMemberAdvice<EventBuilder>, IIntroduceEventAdvice
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
            AspectLinkerOptions? linkerOptions,
            IReadOnlyDictionary<string, object?> tags )
            : base(aspect, targetDeclaration, null, scope, conflictBehavior, linkerOptions, tags )
        {
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            throw new System.NotImplementedException();
        }
    }
}