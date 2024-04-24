// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders;

public interface INamedTypeBuilder : IMemberOrNamedTypeBuilder, INamedType
{
    /// <summary>
    /// Gets or sets the type from which the current type derives.
    /// </summary>
    new INamedType? BaseType { get; set; }
}