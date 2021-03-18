// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Read-only list of <see cref="IMethod"/>.
    /// </summary>
    public interface IMethodList : IMemberList<IMethod>
    {
        IEnumerable<IMethod> OfCompatibleSignature( string name, int genericParameterCount, IReadOnlyList<IType> parameterTypes );

        IMethod? OfExactSignature( IMethod method);
    }
}