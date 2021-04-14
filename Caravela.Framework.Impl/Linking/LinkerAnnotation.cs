// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.Linking
{
    internal enum LinkerAnnotationOrder
    {
        /// <summary>
        /// Calls the semantic in the state it is after the current aspect has been applied.
        /// </summary>
        Default,

        /// <summary>
        /// Calls the semantic in the original order, before any transformation.
        /// </summary>
        Original,
    }

    /// <summary>
    /// Wrapper of the linker annotation on nodes.
    /// </summary>
    internal struct LinkerAnnotation
    {
        /// <summary>
        /// Gets the aspect layer.
        /// </summary>
        public AspectLayerId AspectLayer { get; }

        /// <summary>
        /// Gets a value indicating which version of the semantic must be invoked.
        /// </summary>
        public LinkerAnnotationOrder Order { get; }

        public LinkerAnnotation( AspectLayerId aspectLayer, LinkerAnnotationOrder order )
        {
            this.AspectLayer = aspectLayer;
            this.Order = order;
        }

        public static LinkerAnnotation FromString( string str )
        {
            var parts = str.Split( '$' );

            var parseSuccess = Enum.TryParse<LinkerAnnotationOrder>( parts[1], out var order );

            Invariant.Assert( parseSuccess );

            return new LinkerAnnotation( AspectLayerId.FromString( parts[0] ), order );
        }

        public override string ToString()
        {
            return $"{this.AspectLayer.FullName}${this.Order}";
        }
    }
}
