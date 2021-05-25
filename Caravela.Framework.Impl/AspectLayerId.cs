// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl
{
    internal readonly struct AspectLayerId : IEquatable<AspectLayerId>, IEquatable<AspectLayer>
    {
        private static readonly char[] _separators = { ':' };

        public static bool operator ==( AspectLayerId left, AspectLayerId right ) => left.Equals( right );

        public static bool operator !=( AspectLayerId left, AspectLayerId right ) => !left.Equals( right );

        public AspectLayerId( INamedTypeSymbol aspectType, string? layerName = null ) : this( aspectType.MetadataName, layerName ) { }

        public AspectLayerId( AspectClass aspectClass, string? layerName = null ) : this( aspectClass.FullName, layerName ) { }

        public AspectLayerId( string aspectName, string? layerName = null )
        {
            this.AspectName = aspectName;
            this.LayerName = layerName;
        }

        public static AspectLayerId FromString( string s )
        {
            var parts = s.Split( _separators );

            if ( parts.Length == 1 )
            {
                return new AspectLayerId( parts[0] );
            }

            return new AspectLayerId( parts[0], parts.Length == 2 ? parts[1] : null );
        }

        public bool IsDefault => this.LayerName == null;

        public string AspectName { get; }

        public string? LayerName { get; }

        public string FullName => this.LayerName == null ? this.AspectName : this.AspectName + ":" + this.LayerName;

        public override string ToString() => this.FullName;

        public bool Equals( AspectLayerId other )
            => StringComparer.Ordinal.Equals( this.AspectName, other.AspectName ) && StringComparer.Ordinal.Equals( this.LayerName, other.LayerName );

        public override int GetHashCode()
            => StringComparer.Ordinal.GetHashCode( this.AspectName ) ^ (this.LayerName == null ? 0 : StringComparer.Ordinal.GetHashCode( this.LayerName ));

        public bool Equals( AspectLayer other ) => this.Equals( other.AspectLayerId );

        public override bool Equals( object obj )
            => obj switch
            {
                AspectLayerId id => this.Equals( id ),
                AspectLayer layer => this.Equals( layer ),
                _ => false
            };
    }
}