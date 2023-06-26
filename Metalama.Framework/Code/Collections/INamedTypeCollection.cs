// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Collections.Generic;

namespace Metalama.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="INamedType"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    public interface INamedTypeCollection : IMemberOrNamedTypeCollection<INamedType>
    {
        IEnumerable<INamedType> OfTypeDefinition( INamedType typeDefinition );
    }

    [CompileTime]
    public interface IAssemblyCollection : IReadOnlyCollection<IAssembly>
    {
        IEnumerable<IAssembly> OfName( string name );
    }
}