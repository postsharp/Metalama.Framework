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

            public bool IsExplicit => ((InterfaceMemberAttribute) this.TemplateInfo.Attribute).IsExplicit;

            public TemplateInfo TemplateInfo { get; }

            public ITagReader Tags { get; }

            public MemberSpecification(
                IMember interfaceMember,
                IMember? targetMember,
                IMember? aspectInterfaceMember,
                TemplateInfo templateInfo,
                ITagReader tags )
            {
                this.InterfaceMember = interfaceMember;
                this.TargetMember = targetMember;
                this.AspectInterfaceMember = aspectInterfaceMember;
                this.TemplateInfo = templateInfo;
                this.Tags = tags;
            }
        }
    }
}