using Caravela.Framework.Code;

namespace Caravela.Framework.Impl
{
    internal record AspectPartId( string AspectType, string? PartName )
    {
        public AspectPartId( INamedType aspectType, string? partName ) : this( aspectType.FullName, partName )
        {
        }
    }
}
