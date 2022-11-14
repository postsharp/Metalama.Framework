// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Introspection;

/// <summary>
/// Wraps an <see cref="IAttribute"/> (which represents a custom attribute) into a <see cref="IIntrospectionAspectPredecessor"/>.
/// </summary>
public interface IIntrospectionAttributeAsPredecessor : IIntrospectionAspectPredecessorInternal
{
    /// <summary>
    /// Gets the custom attribute.
    /// </summary>
    IAttribute Attribute { get; }
}