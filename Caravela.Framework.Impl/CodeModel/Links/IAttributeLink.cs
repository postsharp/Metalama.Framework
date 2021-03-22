// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    /// <summary>
    /// A specialization of <see cref="ICodeElementLink{T}"/> that exposes members that allows to look up
    /// attributes by type or declaring element without having to resolve their target.
    /// </summary>
    internal interface IAttributeLink : ICodeElementLink<IAttribute>
    {
        // Intentionally using the struct and not the interface to avoid memory allocation.
        CodeElementLink<INamedType> AttributeType { get; }

        CodeElementLink<ICodeElement> DeclaringElement { get; }
    }
}