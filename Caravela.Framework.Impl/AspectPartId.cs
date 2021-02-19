using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Impl
{
    internal readonly struct  AspectSliceId : IEquatable<AspectSliceId>
    {
        public AspectPartId AspectPart { get; }

        public int Depth { get; }

        public AspectSliceId( AspectPartId aspectPart, int depth )
        {
            this.AspectPart = aspectPart;
            this.Depth = depth;
        }

        public bool Equals(AspectSliceId other) => this.AspectPart.Equals(other.AspectPart) && this.Depth == other.Depth;

        public override bool Equals(object? obj) => obj is AspectSliceId other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.AspectPart.GetHashCode() * 397) ^ this.Depth;
            }
        }

        public static bool operator ==(AspectSliceId left, AspectSliceId right) => left.Equals(right);

        public static bool operator !=(AspectSliceId left, AspectSliceId right) => !left.Equals(right);
    }
    
    internal class AspectPartId : IEquatable<AspectPartId>
    {
        public AspectPartId( INamedType aspectType, string? partName ) : this( aspectType.FullName, partName )
        {
            
        }
        
        public AspectPartId( string aspectName, string? partName )
        {
            this.AspectName = aspectName;
            this.PartName = partName;
        }
        
        public bool IsDefault => this.PartName == null;
        
        public string AspectName { get; }
        public  string? PartName { get; }
        
        public string FullName => this.PartName == null ? this.AspectName : this.AspectName + ":" + this.PartName;


        public bool Equals(AspectPartId? other)
        {
            if ( ReferenceEquals(other, null) )
            {
                return false;
            }

            return StringComparer.Ordinal.Equals( this.AspectName, other.AspectName ) && StringComparer.Ordinal.Equals( this.PartName, other.PartName );
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode( this.AspectName ) ^ StringComparer.Ordinal.GetHashCode( this.PartName );
        }
    }
}
