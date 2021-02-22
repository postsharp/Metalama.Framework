using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Impl
{
    internal sealed record AspectPartId( string AspectType, string? PartName )
    {
        public AspectPartId( INamedType aspectType, string? partName ) : this( aspectType.FullName, partName )
        {
        }

        public bool Equals( AspectPartId? other )
        {
            if ( object.ReferenceEquals( other, null ) )
            {
                return false;
            }

            return StringComparer.Ordinal.Equals( this.AspectType, other.AspectType ) && StringComparer.Ordinal.Equals( this.PartName, other.PartName );
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode( this.AspectType ) ^ StringComparer.Ordinal.GetHashCode( this.PartName );
        }
    }
}
