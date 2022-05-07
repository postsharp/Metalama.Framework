// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Advices;

internal readonly struct BoundTemplateMethod
{
    /// <summary>
    /// Gets the overridden method in case of override, or <c>null</c> in case of introduction.
    /// </summary>
    public IMethod? OverriddenMethod { get; }

    public TemplateMember<IMethod> Template { get; }

    public BoundTemplateMethod( TemplateMember<IMethod> template, IMethod? overriddenMethod, object?[] templateParameters )
    {
        this.OverriddenMethod = overriddenMethod;
        this.Template = template;
        this.TemplateParameters = templateParameters;

#if DEBUG
        if ( template.Declaration?.MethodKind is MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove && templateParameters.Length != 1 )
        {
            throw new AssertionFailedException();
        }
#endif
    }

    public bool IsNull => this.Template.IsNull;

    public bool IsNotNull => this.Template.IsNotNull;

    public object?[] TemplateParameters { get; }
}