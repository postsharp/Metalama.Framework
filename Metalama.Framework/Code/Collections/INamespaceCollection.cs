// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Validation;
using System.Collections.Generic;

namespace Metalama.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="INamespace"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    [InternalImplement]
    public interface INamespaceCollection : IReadOnlyCollection<INamespace>
    {
        /// <summary>
        /// Gets a child <see cref="INamespace"/> by name (not by <see cref="INamespaceOrNamedType.FullName"/>).
        /// </summary>
        INamespace? OfName( string name );
    }
}