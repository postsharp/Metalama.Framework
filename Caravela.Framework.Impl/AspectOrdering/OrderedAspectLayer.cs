namespace Caravela.Framework.Impl.AspectOrdering
{
    internal class OrderedAspectLayer : AspectLayer
    {

        public int Order { get; }

        public OrderedAspectLayer( int order, AspectLayer aspectLayer ) : base( aspectLayer.AspectType, aspectLayer.LayerName )
        {
            this.Order = order;
        }

        public override string ToString() => base.ToString() + " => " + this.Order;
    }
}