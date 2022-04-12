// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders;

public interface IPropertyOrIndexerBuilder : IPropertyOrIndexer, IMemberBuilder
{
    /// <summary>
    /// Gets or sets the <see cref="Metalama.Framework.Code.RefKind"/> of the property
    /// (i.e. <see cref="Code.RefKind.Ref"/>, <see cref="Code.RefKind.Out"/>, ...).
    /// </summary>
    new RefKind RefKind { get; set; }

    /// <summary>
    /// Gets the <see cref="IMethodBuilder"/> for the getter.
    /// </summary>
    new IMethodBuilder? GetMethod { get; }

    /// <summary>
    /// Gets the <see cref="IMethodBuilder"/> for the setter.
    /// </summary>
    new IMethodBuilder? SetMethod { get; }
}