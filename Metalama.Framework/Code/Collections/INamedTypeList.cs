// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Metalama.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="INamedType"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    public interface INamedTypeList : IMemberList<INamedType>
    {
        [Obsolete( "Not implemented." )]
        IReadOnlyList<INamedType> DerivedFrom( Type type );

        [Obsolete( "Not implemented." )]
        IReadOnlyList<INamedType> DerivedFrom( INamedType type );
    }
}