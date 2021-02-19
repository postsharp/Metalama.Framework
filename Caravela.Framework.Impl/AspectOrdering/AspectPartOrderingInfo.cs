namespace Caravela.Framework.Impl.AspectOrdering
{
    internal class OrderedAspectPart : AspectPart
    {
        public int Order { get; }

        public OrderedAspectPart( int order, AspectPart aspectPart ) : base( aspectPart )
        {
            this.Order = order;
        }
    }
}