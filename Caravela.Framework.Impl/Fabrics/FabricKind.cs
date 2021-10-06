// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// Enumerates the kinds of fabrics.
    /// </summary>
    internal enum FabricKind
    {
        // The order is significant because it becomes the execution order.

        Compilation,
        
        // Transitive dependencies are intentionally running after compilation dependencies, so compilation dependencies have a chance
        // to configure the transitive dependencies before they run.
        Transitive,
        Namespace,
        Type
    }
}