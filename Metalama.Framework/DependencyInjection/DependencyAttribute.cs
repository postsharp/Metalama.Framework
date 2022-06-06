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
    public sealed override void BuildAspect( IMemberOrNamedType templateMember, string templateMemberId, IAspectBuilder<IDeclaration> builder )
    {
        var context = new WeaveDependencyContext( (IFieldOrProperty) templateMember, this, builder.Target.GetDeclaringType()!, builder.Diagnostics );

        if ( !builder.Project.DependencyInjectionOptions().TryGetFramework( context, out var framework ) )
        {
            builder.SkipAspect();

            return;
        }

        framework.Weave( templateMember, templateMemberId, builder );
    }
}