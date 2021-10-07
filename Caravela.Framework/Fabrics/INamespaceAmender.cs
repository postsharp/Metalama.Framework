// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// Argument of <see cref="INamespaceFabric.AmendNamespace"/>. Allows reporting diagnostics and adding aspects to the target declaration of the fabric. 
    /// </summary>
    public interface INamespaceAmender : IAmender<INamespace>
    {
        /// <summary>
        /// Gets the target namespace of the current <see cref="INamespaceFabric"/>.
        /// </summary>
        INamespace Namespace { get; }
    }
}