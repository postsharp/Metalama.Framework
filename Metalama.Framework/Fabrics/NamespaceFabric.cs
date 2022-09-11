// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Fabrics
{
    /// <summary>
    /// An interface that, when implemented by a type in a given namespace, allows that type to analyze and
    /// add aspects to that namespace.
    /// </summary>
    /// <seealso href="@applying-aspects"/>
    public abstract class NamespaceFabric : Fabric
    {
        /// <summary>
        /// The user can implement this method to analyze types in the current namespace, add aspects, and report or suppress diagnostics.
        /// </summary>
        public abstract void AmendNamespace( INamespaceAmender amender );
    }
}