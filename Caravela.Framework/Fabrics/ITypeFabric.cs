// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// A type policy is a nested type of arbitrary name that implements <see cref="ITypeFabric"/> and
    /// that can add aspects and advices to the declaring type. Type policies are executed before any other aspect.
    /// They cannot have layers. (Not implemented.)
    /// </summary>
    [CompileTimeOnly]
    [Obsolete( "Not implemented." )]
    public interface ITypeFabric
    {
        void BuildFabric( ITypeFabricBuilder typeFabricBuilder );
    }
}