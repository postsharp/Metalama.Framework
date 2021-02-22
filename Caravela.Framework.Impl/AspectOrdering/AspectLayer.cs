using System;

namespace Caravela.Framework.Impl.AspectOrdering
{
    internal class AspectLayer : IEquatable<AspectLayerId>
    {
        private readonly AspectType? _aspectType;

        public AspectLayer( AspectType aspectType, string? layerName )
        {
            this._aspectType = aspectType;
            this.AspectLayerId = new AspectLayerId( aspectType, layerName );
        }
        
        // Constructor for testing only.
        public AspectLayer( string aspectTypeName, string? layerName )
        {
            this.AspectLayerId = new AspectLayerId( aspectTypeName, layerName );
        }

        public AspectType AspectType => this._aspectType.AssertNotNull();

        public AspectLayerId AspectLayerId { get; }

        public bool IsDefault => this.AspectLayerId.IsDefault;
        public string AspectName => this.AspectLayerId.AspectName;
        public string? LayerName => this.AspectLayerId.LayerName;
        public bool Equals( AspectLayerId other ) => this.AspectLayerId == other;

        public override int GetHashCode() => this.AspectLayerId.GetHashCode();

        public override string ToString() => this.AspectLayerId.ToString();
    }
}