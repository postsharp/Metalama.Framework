// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal partial class IntroduceInterfaceAdvice
    {
        private class IntroducedInterfaceSpecification
        {
            public INamedType InterfaceType { get; }

            public IReadOnlyList<MemberSpecification> MemberSpecifications { get; }

            public ConflictBehavior ConflictBehavior { get; }

            public AdviceOptions? Options { get; }

            public IntroducedInterfaceSpecification(
                INamedType interfaceType,
                IReadOnlyList<MemberSpecification> memberSpecification,
                ConflictBehavior conflictBehavior,
                AdviceOptions? options )
            {
                this.InterfaceType = interfaceType;
                this.MemberSpecifications = memberSpecification;
                this.ConflictBehavior = conflictBehavior;
                this.Options = options;
            }
        }
    }
}