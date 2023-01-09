﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed partial class ImplementInterfaceAdvice
    {
        private readonly struct MemberSpecification
        {
            public IMember InterfaceMember { get; }

            public IMember? TargetMember { get; }

            public TemplateMember<IMember>? Template { get; }

            public bool IsExplicit => ((InterfaceMemberAttribute) this.Template.TemplateClassMember.TemplateInfo.Attribute!).IsExplicit;

            public IObjectReader? Tags { get; }

            public InterfaceMemberOverrideStrategy OverrideStrategy => ((InterfaceMemberAttribute) this.Template.TemplateClassMember.TemplateInfo.Attribute!).WhenExists;

            public MemberSpecification(
                IMember interfaceMember,
                IMember? targetMember,
                TemplateMember<IMember>? template,
                IObjectReader? tags )
            {
                Invariant.AssertNot( targetMember == null && template == null );

                this.InterfaceMember = interfaceMember;
                this.TargetMember = targetMember;
                this.Template = template;
                this.Tags = tags;
            }
        }
    }
}