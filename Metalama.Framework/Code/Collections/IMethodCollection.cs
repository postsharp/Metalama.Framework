// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Code.Collections
{
    /// <summary>
    /// Read-only list of <see cref="IMethod"/>.
    /// </summary>
    /// <remarks>
    ///  <para>The order of items in this list is undetermined and may change between versions.</para>
    /// </remarks>
    public interface IMethodCollection : IMemberCollection<IMethod>
    {
        IEnumerable<IMethod> OfKind( MethodKind kind );

        IEnumerable<IMethod> OfKind( OperatorKind kind );
    }
}