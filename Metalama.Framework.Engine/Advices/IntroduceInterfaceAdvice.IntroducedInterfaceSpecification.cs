// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advices
{
    internal partial class ImplementInterfaceAdvice
    {
        private class InterfaceSpecification
        {
            public INamedType InterfaceType { get; }

            public bool IsTopLevel { get; }

            public IReadOnlyList<MemberSpecification> MemberSpecifications { get; }

            public InterfaceSpecification(
                INamedType interfaceType,
                bool isTopLevel,
                IReadOnlyList<MemberSpecification> memberSpecification )
            {
                this.InterfaceType = interfaceType;
                this.IsTopLevel = isTopLevel;
                this.MemberSpecifications = memberSpecification;
            }

            public override string ToString() => $"{this.InterfaceType} (IsTopLevel={this.IsTopLevel})";
        }
    }
}