// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;

namespace Metalama.Framework.Engine.Advices
{
    internal partial class ImplementInterfaceAdvice
    {
        private readonly struct MemberSpecification
        {
            public IMember InterfaceMember { get; }

            public IMember? TargetMember { get; }

            public IMember? AspectInterfaceMember { get; }

            public bool IsExplicit => ((InterfaceMemberAttribute) this.TemplateClassMember.TemplateInfo.Attribute!).IsExplicit;

            public TemplateClassMember TemplateClassMember { get; }

            public IObjectReader Tags { get; }

            public MemberSpecification(
                IMember interfaceMember,
                IMember? targetMember,
                IMember? aspectInterfaceMember,
                TemplateClassMember templateClassMember,
                IObjectReader tags )
            {
                this.InterfaceMember = interfaceMember;
                this.TargetMember = targetMember;
                this.AspectInterfaceMember = aspectInterfaceMember;
                this.TemplateClassMember = templateClassMember;
                this.Tags = tags;
            }
        }
    }
}