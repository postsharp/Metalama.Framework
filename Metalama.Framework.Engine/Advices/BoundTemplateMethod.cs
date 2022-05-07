// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Advices;

internal readonly struct BoundTemplateMethod
{
    /// <summary>
    /// Gets the overridden method in case of override, or <c>null</c> in case of introduction.
    /// </summary>
    public IMethod? OverriddenMethod { get; }

    public TemplateMember<IMethod> Template { get; }

    public BoundTemplateMethod( TemplateMember<IMethod> template, IMethod? overriddenMethod, IObjectReader compileTimeParameters )
    {
        this.OverriddenMethod = overriddenMethod;
        this.Template = template;
        this.CompileTimeParameters = compileTimeParameters;
    }

    public IObjectReader CompileTimeParameters { get; }

    public bool IsNull => this.Template.IsNull;
}