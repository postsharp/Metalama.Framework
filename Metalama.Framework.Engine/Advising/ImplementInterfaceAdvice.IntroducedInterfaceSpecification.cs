﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising
{
    internal partial class ImplementInterfaceAdvice
    {
        private sealed class InterfaceSpecification
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