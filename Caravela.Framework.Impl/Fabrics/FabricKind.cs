// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Fabrics
{
    internal enum FabricKind
    {
        // The order is significant because it becomes the execution order.

        Compilation,
        Transitive,
        Namespace,
        Type
    }
}