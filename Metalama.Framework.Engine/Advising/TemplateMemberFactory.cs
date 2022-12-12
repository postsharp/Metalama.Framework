// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;

namespace Metalama.Framework.Engine.Advising
{
    internal static class TemplateMemberFactory
    {
        public static TemplateMember<T> Create<T>(
            T implementation,
            TemplateClassMember templateClassMember,
            IAdviceAttribute adviceAttribute,
            TemplateKind selectedKind,
            TemplateKind interpretedKind )
            where T : class, IMemberOrNamedType
            => new( implementation, templateClassMember, adviceAttribute, selectedKind, interpretedKind );

        public static TemplateMember<T> Create<T>(
            T implementation,
            TemplateClassMember templateClassMember,
            IAdviceAttribute adviceAttribute,
            TemplateKind selectedKind = TemplateKind.Default )
            where T : class, IMemberOrNamedType
            => new( implementation, templateClassMember, adviceAttribute, selectedKind );

        public static TemplateMember<T> Create<T>(
            T implementation,
            TemplateClassMember templateClassMember,
            TemplateKind selectedKind = TemplateKind.Default )
            where T : class, IMemberOrNamedType
            => new( implementation, templateClassMember, (ITemplateAttribute) templateClassMember.TemplateInfo.Attribute.AssertNotNull(), selectedKind );
    }
}