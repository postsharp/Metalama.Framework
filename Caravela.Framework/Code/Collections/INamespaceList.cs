// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Validation;
using System.Collections.Generic;

namespace Caravela.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="INamespace"/>.
    /// </summary>
    [InternalImplement]
    public interface INamespaceList : IReadOnlyList<INamespace>
    {
        /// <summary>
        /// Gets a child <see cref="INamespace"/> by name (not by <see cref="INamespace.FullName"/>).
        /// </summary>
        INamespace? OfName( string name );
    }
}