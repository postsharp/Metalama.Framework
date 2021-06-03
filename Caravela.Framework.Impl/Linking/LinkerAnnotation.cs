// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Wrapper of the linker annotation on nodes.
    /// </summary>
    internal readonly struct LinkerAnnotation
    {
        /// <summary>
        /// Gets the aspect layer.
        /// </summary>
        public AspectLayerId AspectLayerId { get; }

        /// <summary>
        /// Gets a value indicating which version of the semantic must be invoked.
        /// </summary>
        public LinkingOrder Order { get; }

        /// <summary>
        /// Gets a value indicating target kind. For example self or property get accessor.
        /// </summary>
        public LinkerAnnotationTargetKind TargetKind { get; }

        public LinkerAnnotation(
            AspectLayerId aspectLayerId,
            LinkingOrder order,
            LinkerAnnotationTargetKind targetKind = LinkerAnnotationTargetKind.Self )
        {
            this.AspectLayerId = aspectLayerId;
            this.Order = order;
            this.TargetKind = targetKind;
        }

        public static LinkerAnnotation FromString( string str )
        {
            var parts = str.Split( '$' );

            var parseSuccess1 = Enum.TryParse<LinkingOrder>( parts[1], out var order );

            Invariant.Assert( parseSuccess1 );

            var parseSuccess2 = Enum.TryParse<LinkerAnnotationTargetKind>( parts[2], out var targetKind );

            Invariant.Assert( parseSuccess2 );

            return new LinkerAnnotation( AspectLayerId.FromString( parts[0] ), order, targetKind );
        }

        public override string ToString()
        {
            return $"{this.AspectLayerId.FullName}${this.Order}${this.TargetKind}";
        }
    }
}