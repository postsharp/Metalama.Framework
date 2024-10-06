// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;

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
            => new( (ISymbolRef<T>) implementation.ToRef().As<T>(), templateClassMember, adviceAttribute, selectedKind, interpretedKind );

        public static TemplateMember<T> Create<T>(
            ISymbol symbol,
            TemplateClassMember templateClassMember,
            IAdviceAttribute adviceAttribute,
            CompilationContext compilationContext,
            TemplateKind selectedKind,
            TemplateKind interpretedKind )
            where T : class, IMemberOrNamedType
            => new( compilationContext.RefFactory.FromSymbol<T>( symbol ), templateClassMember, adviceAttribute, selectedKind, interpretedKind );

        public static TemplateMember<T> Create<T>(
            T implementation,
            TemplateClassMember templateClassMember,
            IAdviceAttribute adviceAttribute,
            TemplateKind selectedKind = TemplateKind.Default )
            where T : class, IMemberOrNamedType
            => new( (ISymbolRef<T>) implementation.ToRef().As<T>(), templateClassMember, adviceAttribute, selectedKind );

        public static TemplateMember<T> Create<T>(
            ISymbol symbol,
            TemplateClassMember templateClassMember,
            IAdviceAttribute adviceAttribute,
            CompilationContext compilationContext,
            TemplateKind selectedKind = TemplateKind.Default )
            where T : class, IMemberOrNamedType
            => new( compilationContext.RefFactory.FromSymbol<T>( symbol ), templateClassMember, adviceAttribute, selectedKind );

        public static TemplateMember<T> Create<T>(
            T implementation,
            TemplateClassMember templateClassMember,
            TemplateKind selectedKind = TemplateKind.Default )
            where T : class, IMemberOrNamedType
            => new(
                (ISymbolRef<T>) implementation.ToRef().As<T>(),
                templateClassMember,
                (ITemplateAttribute) templateClassMember.Attribute.AssertNotNull(),
                selectedKind );

        public static TemplateMember<T> Create<T>(
            ISymbol symbol,
            TemplateClassMember templateClassMember,
            CompilationContext compilationContext,
            TemplateKind selectedKind = TemplateKind.Default )
            where T : class, IMemberOrNamedType
            => new(
                compilationContext.RefFactory.FromSymbol<T>( symbol ),
                templateClassMember,
                (ITemplateAttribute) templateClassMember.Attribute.AssertNotNull(),
                selectedKind );
    }
}