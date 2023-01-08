// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed partial class ImplementInterfaceAdvice
    {
        private readonly struct MemberSpecification
        {
            public IMember InterfaceMember { get; }

            public IMember? TargetMember { get; }

            public IMember? AspectInterfaceMember { get; }

            public bool IsExplicit => ((InterfaceMemberAttribute) this.TemplateClassMember.TemplateInfo.Attribute!).IsExplicit;

            public TemplateClassMember TemplateClassMember { get; }

            public IObjectReader? Tags { get; }

            public InterfaceMemberOverrideStrategy OverrideStrategy => ((InterfaceMemberAttribute) this.TemplateClassMember.TemplateInfo.Attribute!).WhenExists;

            public MemberSpecification(
                IMember interfaceMember,
                IMember? targetMember,
                IMember? aspectInterfaceMember,
                TemplateClassMember templateClassMember,
                IObjectReader? tags )
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