// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Validation;
using System.Collections.Generic;

namespace Caravela.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="IMemberOrNamedType"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    [InternalImplement]
    public interface IMemberList<out T> : IReadOnlyList<T>
        where T : IMemberOrNamedType
    {
        /// <summary>
        /// Gets the set of members of a given name. Note that for named types, the short name will be matched,
        /// as opposed to the full, namespace-prefixed name.
        /// </summary>
        /// <param name="name">The member name (not including the namespace, for types).</param>
        /// <returns></returns>
        IEnumerable<T> OfName( string name );
    }
}