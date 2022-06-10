// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.DependencyInjection;

/// <summary>
/// Custom attribute that, when be applied to a field or automatic property of an aspect, means that this field or property is a service dependency
/// that introduced into the target type and handled by a dependency injection framework. The implementation of this custom attribute depends
/// on the dependency injection framework. 
/// </summary>
public class DependencyAttribute : DeclarativeAdviceAttribute
{
    private bool? _isLazy;

    public sealed override void BuildAspect( IMemberOrNamedType templateMember, string templateMemberId, IAspectBuilder<IDeclaration> builder )
    {
        var context = new WeaveDependencyContext(
            (IFieldOrProperty) templateMember,
            templateMemberId,
            this,
            builder.Target.GetDeclaringType()!,
            builder.Diagnostics );

        if ( !builder.Project.DependencyInjectionOptions().TryGetFramework( context, out var framework ) )
        {
            builder.SkipAspect();

            return;
        }

        framework.InjectDependency( context, builder.WithTarget( builder.Target.GetDeclaringType()! ) );
    }

    /// <summary>
    /// Gets the value of the <see cref="IsLazy"/> if it has been assigned, or <c>null</c> if it has not been assigned.
    /// </summary>
    public bool? GetIsLazy() => this._isLazy;

    /// <summary>
    /// Gets or sets a value indicating whether the dependency should be pulled from the container lazily, i.e. upon first use.
    /// </summary>
    public bool IsLazy
    {
        get => this._isLazy.GetValueOrDefault();
        set => this._isLazy = value;
    }
}