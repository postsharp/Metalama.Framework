// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// A specialization of <see cref="ISdkRef{T}"/> that exposes a <see cref="Name"/> property.
    /// This allows for efficient implementation of name-based lookups.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IMemberRef<out T> : IRefImpl<T>
        where T : class, IMemberOrNamedType
    {
        /// <summary>
        /// Gets the member name without resolving to the target.
        /// </summary>
        [Obfuscation( Exclude = true )] // Working around an obfuscator bug. 
        string Name { get; }
    }
}