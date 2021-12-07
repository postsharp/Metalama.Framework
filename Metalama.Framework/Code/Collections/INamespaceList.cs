// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Validation;
using System.Collections.Generic;

namespace Caravela.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="INamespace"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    [InternalImplement]
    public interface INamespaceList : IReadOnlyList<INamespace>
    {
        /// <summary>
        /// Gets a child <see cref="INamespace"/> by name (not by <see cref="INamespace.FullName"/>).
        /// </summary>
        INamespace? OfName( string name );
    }
}