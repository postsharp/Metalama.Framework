// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

// ReSharper disable UnusedMemberInSuper.Global

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// A specialization of <see cref="ISdkRef{T}"/> that exposes a <see cref="Name"/> property.
    /// This allows for efficient implementation of name-based lookups.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IMemberRef<out T> : IRefImpl<T>
        where T : class, INamedDeclaration
    {
        /// <summary>
        /// Gets the member name without resolving to the target.
        /// </summary>
        string Name { get; }
    }
}