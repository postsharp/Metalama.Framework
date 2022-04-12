// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders;

public interface IPropertyLikeBuilder : IPropertyLike, IMemberBuilder
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