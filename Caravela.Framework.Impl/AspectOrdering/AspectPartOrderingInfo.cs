using System;

namespace Caravela.Framework.Impl.AspectOrdering
{
    internal class AspectLayer : IEquatable<AspectLayerId>
    {
        public AspectLayer( AspectType aspectType, string? layerName )
        {
            this.AspectType = aspectType;
            this.Id = new AspectLayerId(aspectType, layerName);
        }

        public AspectType AspectType { get; }
        public AspectLayerId Id { get; }

        public bool IsDefault => this.Id.IsDefault;
        public string AspectName => this.Id.AspectName;
        public string? LayerName => this.Id.LayerName;
        public bool Equals( AspectLayerId other ) => this.Id == other;

        public override int GetHashCode() => this.Id.GetHashCode();

    }
    internal class OrderedAspectLayer : AspectLayer
    {
        public AspectLayerId AspectLayerId { get; }
        
        public int Order { get; }

        

        public OrderedAspectLayer( int order, AspectLayer aspectLayer ) : base( aspectLayer.AspectType, aspectLayer.LayerName )
        {
            this.AspectLayerId = aspectLayer.Id;
            this.Order = order;
        }
    }
}