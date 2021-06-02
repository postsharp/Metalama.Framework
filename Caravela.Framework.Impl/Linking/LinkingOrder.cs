// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Linking
{
    internal enum LinkingOrder
    {
        /// <summary>
        /// Calls the semantic in the state it is after the current aspect has been applied.
        /// </summary>
        Default,

        /// <summary>
        /// Calls the semantic in the original order, before any transformation.
        /// </summary>
        Original
    }
}