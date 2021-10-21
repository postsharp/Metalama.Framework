// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// Allows adding aspects or analyzing a project, namespace, or type just by adding a type implementing this interface.
    /// You must not implement this interface directly, but <see cref="IProjectFabric"/>, <see cref="INamespaceFabric"/>,
    /// or <see cref="ITypeFabric"/>. 
    /// </summary>
    [CompileTimeOnly]
    public interface IFabric { }
}