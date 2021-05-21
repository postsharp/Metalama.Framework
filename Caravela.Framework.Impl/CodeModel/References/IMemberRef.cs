// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.References
{
    /// <summary>
    /// A specialization of <see cref="IDeclarationRef{T}"/> that exposes a <see cref="Name"/> property.
    /// This allows for efficient implementation of name-based lookups.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IMemberRef<out T> : IDeclarationRef<T>
        where T : IMember
    {
        /// <summary>
        /// Gets the member name without resolving to the target.
        /// </summary>
        string Name { get; }
    }
}