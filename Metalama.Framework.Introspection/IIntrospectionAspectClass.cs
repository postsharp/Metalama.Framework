// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System.Collections.Immutable;

namespace Metalama.Framework.Introspection;

/// <summary>
/// Represents an aspect class (i.e. a type of aspect) and exposes all its instances in the current scope.
/// </summary>
public interface IIntrospectionAspectClass : IAspectClass
{
    /// <summary>
    /// Gets the instances of the aspect class in the current scope.
    /// </summary>
    ImmutableArray<IIntrospectionAspectInstance> Instances { get; }
}