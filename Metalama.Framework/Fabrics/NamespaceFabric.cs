// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Fabrics
{
    /// <summary>
    /// An interface that, when implemented by a type in a given namespace, allows that type to analyze and
    /// add aspects to that namespace.
    /// </summary>
    public abstract class NamespaceFabric : Fabric
    {
        /// <summary>
        /// The user can implement this method to analyze types in the current namespace, add aspects, and report or suppress diagnostics.
        /// </summary>
        public abstract void AmendNamespace( INamespaceAmender amender );
    }
}