// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.AspectOrdering
{
    internal class OrderedAspectLayer : AspectLayer
    {
        public int Order { get; }

        public OrderedAspectLayer( int order, AspectLayer aspectLayer ) : base( aspectLayer.AspectClass, aspectLayer.LayerName )
        {
            this.Order = order;
        }

        // For testing only.
        internal OrderedAspectLayer( int order, string aspectName, string? layerName ) : base( aspectName, layerName )
        {
            this.Order = order;
        }

        public override string ToString() => base.ToString() + " => " + this.Order;
    }
}