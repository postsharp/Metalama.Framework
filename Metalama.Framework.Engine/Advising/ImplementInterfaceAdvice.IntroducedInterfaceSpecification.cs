// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising
{
    internal partial class ImplementInterfaceAdvice
    {
        private class InterfaceSpecification
        {
            public INamedType InterfaceType { get; }

            public IReadOnlyList<MemberSpecification> MemberSpecifications { get; }

            public InterfaceSpecification(
                INamedType interfaceType,
                IReadOnlyList<MemberSpecification> memberSpecification )
            {
                this.InterfaceType = interfaceType;
                this.MemberSpecifications = memberSpecification;
            }

            public override string ToString() => $"{this.InterfaceType}";
        }
    }
}