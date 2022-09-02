// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders;

/// <summary>
/// Base interface for <see cref="IFieldBuilder"/>, <see cref="IPropertyBuilder"/> and <see cref="IIndexerBuilder"/>.
/// </summary>
public interface IFieldOrPropertyOrIndexerBuilder : IFieldOrPropertyOrIndexer, IMemberBuilder
{
    /// <summary>
    /// Gets the <see cref="IMethodBuilder"/> for the getter.
    /// </summary>
    new IMethodBuilder? GetMethod { get; }

    /// <summary>
    /// Gets the <see cref="IMethodBuilder"/> for the setter.
    /// </summary>
    new IMethodBuilder? SetMethod { get; }
}