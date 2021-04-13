// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Read-only list of <see cref="IConstructor"/>.
    /// </summary>
    public interface IConstructorList : IMemberList<IConstructor>
    {
        // TODO: Document.

        IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<Type?>? argumentTypes );

        IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<IType?>? argumentTypes = null, IReadOnlyList<RefKind?>? refKinds = null );

        IConstructor? OfExactSignature( IConstructor signatureTemplate );

        IConstructor? OfExactSignature( IReadOnlyList<IType> parameterTypes, IReadOnlyList<RefKind>? refKinds = null );
    }
}