// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Read-only list of <see cref="IMethod"/>.
    /// </summary>
    public interface IMethodList : IMemberList<IMethod>
    {
        // TODO: Document.

        IEnumerable<IMethod> OfCompatibleSignature( string name, int? genericParameterCount, IReadOnlyList<Type?>? argumentTypes, bool? isStatic = false, bool declaredOnly = true );

        IEnumerable<IMethod> OfCompatibleSignature( string name, int? genericParameterCount = null, IReadOnlyList<IType?>? argumentTypes = null, IReadOnlyList<RefKind?>? refKinds = null, bool? isStatic = false, bool declaredOnly = true );

        IMethod? OfExactSignature( string name, int genericParameterCount, IReadOnlyList<IType> parameterTypes, IReadOnlyList<RefKind>? refKinds = null, bool? isStatic = null, bool declaredOnly = true );

        IMethod? OfExactSignature( IMethod signatureTemplate, bool matchIsStatic = true, bool declaredOnly = true );

        // TODO: IMethod? OfBestSignature( ... )
    }
}