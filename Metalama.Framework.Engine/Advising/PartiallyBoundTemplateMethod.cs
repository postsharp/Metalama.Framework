// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Advising;

/// <summary>
/// Represents binding of the template method against template arguments at the point where the target declaration is not yet known.
/// It is used to bind type parameters of introduction templates before the final declaration is known.
/// The template may become invalid after binding to the final target declaration.
/// </summary>
internal sealed class PartiallyBoundTemplateMethod
{
    /// <summary>
    /// Gets the template member of the aspect.
    /// </summary>
    public TemplateMember<IMethod> TemplateMember { get; }

    /// <summary>
    /// Gets the template declaration.
    /// </summary>
    public IRef<IMethod> Declaration => this.TemplateMember.Declaration;

    /// <summary>
    /// Gets arguments of the template.
    /// </summary>
    public IObjectReader? TemplateArguments { get; }

    /// <summary>
    /// Gets bound template type arguments.
    /// </summary>
    public object?[] TypeArguments { get; }

    public PartiallyBoundTemplateMethod( TemplateMember<IMethod> template, object?[] typeArguments, IObjectReader? argumentReader )
    {
        this.TemplateMember = template;
        this.TemplateArguments = argumentReader;
        this.TypeArguments = typeArguments;
    }
}