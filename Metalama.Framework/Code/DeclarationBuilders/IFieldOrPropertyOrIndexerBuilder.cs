// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders;

/// <summary>
/// Base interface for <see cref="IFieldBuilder"/>, <see cref="IPropertyBuilder"/> and <see cref="IIndexerBuilder"/>.
/// </summary>
public interface IFieldOrPropertyOrIndexerBuilder : IFieldOrPropertyOrIndexer, IMemberBuilder
{
    /// <summary>
    /// Gets or sets the type of the field or property.
    /// </summary>
    new IType Type { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Metalama.Framework.Code.RefKind"/> of the property, indexer or property
    /// (i.e. <see cref="Code.RefKind.Ref"/>, <see cref="Code.RefKind.Out"/>, ...).
    /// </summary>
    new RefKind RefKind { get; set; }
}