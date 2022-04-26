// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advices
{
    internal partial class ImplementInterfaceAdvice
    {
        private class IntroducedInterfaceSpecification
        {
            public INamedType InterfaceType { get; }

            public IReadOnlyList<MemberSpecification> MemberSpecifications { get; }

            public OverrideStrategy OverrideStrategy { get; }

            public IntroducedInterfaceSpecification(
                INamedType interfaceType,
                IReadOnlyList<MemberSpecification> memberSpecification,
                OverrideStrategy overrideStrategy )
            {
                this.InterfaceType = interfaceType;
                this.MemberSpecifications = memberSpecification;
                this.OverrideStrategy = overrideStrategy;
            }

            public override string ToString() => $"{this.InterfaceType}, {this.OverrideStrategy}";
        }
    }
}