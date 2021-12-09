// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Metalama.Framework.Code.Collections
{
    /// <summary>
    /// List of interfaces implemented by a named type.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    public interface IImplementedInterfaceList : IReadOnlyList<INamedType>
    {
        /// <summary>
        /// Determines whether the current collection contains a given <see cref="Type"/>.
        /// </summary>
        bool Contains( Type type );
    }
}