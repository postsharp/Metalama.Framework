// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal partial class ImplementInterfaceAdvice
    {
        private class IntroducedInterfaceSpecification
        {
            public INamedType InterfaceType { get; }

            public IReadOnlyList<MemberSpecification> MemberSpecifications { get; }

            // ReSharper disable  UnusedAutoPropertyAccessor.Local

            public OverrideStrategy OverrideStrategy { get; }

            public Dictionary<string, object?>? Tags { get; }

            public IntroducedInterfaceSpecification(
                INamedType interfaceType,
                IReadOnlyList<MemberSpecification> memberSpecification,
                OverrideStrategy overrideStrategy,
                Dictionary<string, object?>? tags )
            {
                this.InterfaceType = interfaceType;
                this.MemberSpecifications = memberSpecification;
                this.OverrideStrategy = overrideStrategy;
                this.Tags = tags;
            }
        }
    }
}