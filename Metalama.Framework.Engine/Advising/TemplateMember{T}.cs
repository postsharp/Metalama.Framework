// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CompileTime;

namespace Metalama.Framework.Engine.Advising;

internal sealed class TemplateMember<T> : TemplateMember
    where T : class, IMemberOrNamedType
{
    protected override ISymbolRef<IMemberOrNamedType> GetDeclaration() => this.DeclarationRef;

    public new ISymbolRef<T> DeclarationRef { get; }

    public TemplateMember(
        ISymbolRef<T> implementation,
        TemplateClassMember templateClassMember,
        TemplateProvider templateProvider,
        IAdviceAttribute adviceAttribute,
        TemplateKind selectedTemplateKind = TemplateKind.Default ) : this(
        implementation,
        templateClassMember,
        templateProvider,
        adviceAttribute,
        selectedTemplateKind,
        selectedTemplateKind ) { }

    public TemplateMember(
        ISymbolRef<T> implementation,
        TemplateClassMember templateClassMember,
        TemplateProvider templateProvider,
        IAdviceAttribute adviceAttribute,
        TemplateKind selectedTemplateKind,
        TemplateKind interpretedTemplateKind ) : base(
        implementation,
        templateClassMember,
        templateProvider,
        adviceAttribute,
        selectedTemplateKind,
        interpretedTemplateKind )
    {
        this.DeclarationRef = (ISymbolRef<T>) implementation.As<T>();
    }

    public TemplateMember( TemplateMember prototype ) : base( prototype )
    {
        this.DeclarationRef = (ISymbolRef<T>) prototype.DeclarationRef.As<T>();
    }
}