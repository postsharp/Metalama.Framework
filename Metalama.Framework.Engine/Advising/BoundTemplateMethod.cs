// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Advising;

internal readonly struct BoundTemplateMethod
{
    /// <summary>
    /// Gets the overridden method in case of override, or <c>null</c> in case of introduction.
    /// </summary>
    public Ref<IMethodBase> OverriddenMethodBase { get; }

    public TemplateMember<IMethod> Template { get; }

    public BoundTemplateMethod( TemplateMember<IMethod> template, IMethodBase? overriddenMethodBase, object?[] templateArguments )
    {
        this.OverriddenMethodBase = overriddenMethodBase != null ? overriddenMethodBase.ToTypedRef() : default;
        this.Template = template;
        this.TemplateArguments = templateArguments;

#if DEBUG
        if ( template.Declaration?.MethodKind is MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove && templateArguments.Length != 1 )
        {
            throw new AssertionFailedException();
        }
#endif
    }

    public bool IsNull => this.Template.IsNull;

    public bool IsNotNull => this.Template.IsNotNull;

    public object?[] TemplateArguments { get; }
}