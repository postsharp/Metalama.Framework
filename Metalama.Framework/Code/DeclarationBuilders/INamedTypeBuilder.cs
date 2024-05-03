// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders;

public interface INamedTypeBuilder : IMemberOrNamedTypeBuilder, INamedType
{
    /// <summary>
    /// Gets or sets the type from which the current type derives.
    /// </summary>
    new INamedType? BaseType { get; set; }
    // TODO: Base types, interface implementations, type parameters

    /// <summary>
    /// Adds a generic parameter to the type.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>An <see cref="ITypeParameterBuilder"/> that allows you to further build the new parameter.</returns>
    ITypeParameterBuilder AddTypeParameter(string name);
}