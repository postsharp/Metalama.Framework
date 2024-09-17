using Metalama.Framework.Code;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel;

internal readonly struct GenericMap( IReadOnlyList<IType> typeArguments ) : IEquatable<GenericMap>
{
    public bool IsEmpty => this.TypeArguments.Count == 0;

    public IReadOnlyList<IType> TypeArguments { get; init; } = typeArguments;

    public static readonly GenericMap Empty = new GenericMap();

    public GenericMap Apply( in GenericMap map )
    {
        if ( map.IsEmpty )
        {
            return Empty;
        }
        
        var results = new IType[map.TypeArguments.Count];
        Mapper? mapper = null;


        for ( var i = 0; i < map.TypeArguments.Count; i++ )
        {
            var type = map.TypeArguments[i];
            
            if ( type is ITypeParameter typeParameter )
            {
                results[i] = this.Map( typeParameter );
            }
            else
            {
                mapper ??= new Mapper( this );
                results[i] = mapper.Visit( type );
            }
        }

        return new GenericMap(results);
    }

    public IType Map( ITypeParameter typeParameter )
    {
        if ( typeParameter.ContainingDeclaration.DeclarationKind != DeclarationKind.NamedType )
        {
            throw new NotImplementedException( "Method type parameters are not supported." );
        }
        
        return this.TypeArguments[typeParameter.Index];
    }

    public IType Map( IType type )
    {
        if ( type is ITypeParameter typeParameter )
        {
            return this.Map( typeParameter );
        }
        else
        {
            return new Mapper( this ).Visit( type );
        }
    }

    private sealed class Mapper : TypeRewriter
    {
        private readonly GenericMap _genericMap;

        public Mapper( GenericMap genericMap ) 
        {
            this._genericMap = genericMap;
        }

        internal override IType Visit( ITypeParameter typeParameter )
        {
            return this._genericMap.Map( typeParameter );
        }
    }

    public bool Equals( GenericMap other )
    {
        if ( Equals( other.TypeArguments, this.TypeArguments ) )
        {
            return true;
        }

        if ( other.TypeArguments.Count != this.TypeArguments.Count )
        {
            return false;
        }

        for ( var i = 0; i < this.TypeArguments.Count; i++ )
        {
            if ( !other.TypeArguments[i].Equals(  this.TypeArguments[i] ) )
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach ( var type in this.TypeArguments )
        {
            hashCode.Add( type.GetHashCode() );
        }

        return hashCode.ToHashCode();
    }
    

    public static bool operator ==( GenericMap left, GenericMap right ) => left.Equals( right );

    public static bool operator !=( GenericMap left, GenericMap right ) => !left.Equals( right );
}