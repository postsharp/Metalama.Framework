// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using System;

namespace Caravela.Framework.Impl.AspectOrdering
{
    internal class AspectLayer : IEquatable<AspectLayerId>
    {
        private readonly AspectClass? _aspectClass;

        public AspectLayer( AspectClass aspectClass, string? layerName )
        {
            this._aspectClass = aspectClass;
            this.AspectLayerId = new AspectLayerId( aspectClass, layerName );
        }

        // Constructor for testing only.
        public AspectLayer( string aspectTypeName, string? layerName )
        {
            this.AspectLayerId = new AspectLayerId( aspectTypeName, layerName );
        }

        public AspectClass AspectClass => this._aspectClass.AssertNotNull();

        public AspectLayerId AspectLayerId { get; }

        public bool IsDefault => this.AspectLayerId.IsDefault;

        public string AspectName => this.AspectLayerId.AspectName;

        public string? LayerName => this.AspectLayerId.LayerName;

        public bool Equals( AspectLayerId other ) => this.AspectLayerId == other;

        public override int GetHashCode() => this.AspectLayerId.GetHashCode();

        public override string ToString() => this.AspectLayerId.ToString();
    }
}