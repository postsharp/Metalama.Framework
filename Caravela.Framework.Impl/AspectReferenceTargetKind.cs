// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Provides kinds aspect reference targets.
    /// </summary>
    internal enum AspectReferenceTargetKind
    {
        /// <summary>
        /// Target the annotated declaration.
        /// </summary>
        Self,

        /// <summary>
        /// Target the get accessor of the referenced property.
        /// </summary>
        PropertyGetAccessor,

        /// <summary>
        /// Target the set accessor of the referenced property.
        /// </summary>
        PropertySetAccessor,

        /// <summary>
        /// Target the add accessor of the referenced event.
        /// </summary>
        EventAddAccessor,

        /// <summary>
        /// Target the remove accessor of the referenced event.
        /// </summary>
        EventRemoveAccessor
    }
}