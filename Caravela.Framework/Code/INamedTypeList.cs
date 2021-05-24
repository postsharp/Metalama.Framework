// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Read-only list of <see cref="INamedType"/>.
    /// </summary>
    public interface INamedTypeList : IMemberList<INamedType>
    {
        IReadOnlyList<INamedType> DerivedFrom( Type type );
        IReadOnlyList<INamedType> DerivedFrom( INamedType type );
        
    }
}