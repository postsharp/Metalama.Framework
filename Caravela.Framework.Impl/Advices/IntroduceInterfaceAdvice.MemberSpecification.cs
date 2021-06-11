// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
{
    internal partial class IntroduceInterfaceAdvice
    {
        private struct MemberSpecification
        {
            public IMember InterfaceMember { get; }

            public IMember? TargetMember { get; }

            public IMember? AspectInterfaceTargetMember { get; }

            public bool IsExplicit { get; }

            public MemberSpecification( IMember interfaceMember, IMember? targetMember, IMember? aspectInterfaceMember, bool isExplicit )
            {
                this.InterfaceMember = interfaceMember;
                this.TargetMember = targetMember;
                this.AspectInterfaceTargetMember = aspectInterfaceMember;
                this.IsExplicit = isExplicit;
            }
        }
    }
}