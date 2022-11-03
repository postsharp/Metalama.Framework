// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;

namespace Metalama.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="IAttribute"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    public interface IAttributeCollection : IReadOnlyCollection<IAttribute>
    {
        IEnumerable<IAttribute> OfAttributeType( INamedType type );

        IEnumerable<IAttribute> OfAttributeType( Type type );

        bool Any( INamedType type );
        
        bool Any( Type type );
    }
}