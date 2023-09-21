// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Serialization;

namespace Metalama.Framework.Options;

/// <summary>
/// An item in a <see cref="HierarchicalOptionItemCollection{T}"/>.
/// </summary>
public interface IHierarchicalOptionItem : IOverridable, ICompileTimeSerializable
{
    /// <summary>
    /// Gets the key that uniquely identifies the item in the collection.
    /// </summary>
    public object GetKey();
}