// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl
{
    internal enum AspectReferenceOrder
    {
        /// <summary>
        /// Calls the semantic in the state it is after the current aspect layer has been applied.
        /// </summary>
        Self,

        /// <summary>
        /// Gets the final state of the semantic with all transformations. If the semantic is virtual, this results in a virtual call.
        /// Otherwise, this results in a call to the semantic with all transformations of the current class (but not of derived classes).
        /// </summary>
        Final,

        /// <summary>
        /// Gets the state of the semantic before the current aspect layer. If the semantic is  <c>override</c> or <c>new</c> and we are
        /// in the first aspect layer for the current type, this results in a call to <c>base</c>. 
        /// </summary>
        Base,

        /// <summary>
        /// Calls the semantic in the original order, before any transformation.
        /// </summary>
        Original
    }
}