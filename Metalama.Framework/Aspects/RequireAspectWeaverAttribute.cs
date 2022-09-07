// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Custom attribute that, when applied an an aspect class, means that this aspect class is implemented by a low-level built with Metalama SDK.
/// When the <see cref="RequireAspectWeaverAttribute"/> is added to a type, the <see cref="IAspect{T}.BuildAspect"/> method is not invoked.
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
[CompileTime]
public sealed class RequireAspectWeaverAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequireAspectWeaverAttribute"/> class.
    /// </summary>
    /// <param name="weaverType">Full name (namespace and name but not assembly name) of the type implemented the aspect. This type must implement the
    /// <c>IAspectWeaver</c> interface, be annotated with the <c>[MetalamaPlugin]</c> attribute, and the assembly must be included as an analyzer in the project.</param>
    public RequireAspectWeaverAttribute( string weaverType )
    {
        this.Type = weaverType;
    }

    /// <summary>
    /// Gets the namespace-qualified name of the type implementing the aspect.
    /// </summary>
    public string Type { get; }
}