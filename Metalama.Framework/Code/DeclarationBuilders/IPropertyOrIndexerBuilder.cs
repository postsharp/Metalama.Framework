// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders;

/// <summary>
/// Base interface for <see cref="IPropertyBuilder"/> and <see cref="IIndexerBuilder"/>.
/// </summary>
public interface IPropertyOrIndexerBuilder : IPropertyOrIndexer, IPropertyLikeBuilder
{
    /// <summary>
    /// Gets or sets the <see cref="Metalama.Framework.Code.RefKind"/> of the property
    /// (i.e. <see cref="Code.RefKind.Ref"/>, <see cref="Code.RefKind.Out"/>, ...).
    /// </summary>
    new RefKind RefKind { get; set; }
}