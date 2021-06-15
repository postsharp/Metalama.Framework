// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Describes which version of the underlying semantic is referenced by the syntax node.
    /// </summary>
    internal readonly struct AspectReferenceSpecification
    {
        /// <summary>
        /// Gets the aspect layer which created the syntax node.
        /// </summary>
        public AspectLayerId AspectLayerId { get; }

        /// <summary>
        /// Gets a value indicating which version of the target semantic in relation to the aspect layer is referenced.
        /// </summary>
        public AspectReferenceOrder Order { get; }

        /// <summary>
        /// Gets a value indicating target kind. For example self or property get accessor.
        /// </summary>
        public AspectReferenceTargetKind TargetKind { get; }

        public AspectReferenceSpecification(
            AspectLayerId aspectLayerId,
            AspectReferenceOrder order,
            AspectReferenceTargetKind targetKind = AspectReferenceTargetKind.Self )
        {
            this.AspectLayerId = aspectLayerId;
            this.Order = order;
            this.TargetKind = targetKind;
        }

        public static AspectReferenceSpecification FromString( string str )
        {
            var parts = str.Split( '$' );

            var parseSuccess1 = Enum.TryParse<AspectReferenceOrder>( parts[1], out var order );

            Invariant.Assert( parseSuccess1 );

            var parseSuccess2 = Enum.TryParse<AspectReferenceTargetKind>( parts[2], out var targetKind );

            Invariant.Assert( parseSuccess2 );

            return new AspectReferenceSpecification( AspectLayerId.FromString( parts[0] ), order, targetKind );
        }

        public override string ToString()
        {
            return $"{this.AspectLayerId.FullName}${this.Order}${this.TargetKind}";
        }
    }
}