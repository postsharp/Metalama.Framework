// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel;

internal readonly struct GenericMap : IEquatable<GenericMap>, IGenericContextImpl
{
    private GenericMap( IReadOnlyList<IType> typeArguments, bool isIdentity )
    {
        this.TypeArguments = typeArguments;
        this.IsEmptyOrIdentity = isIdentity || typeArguments.Count == 0;
    }

    public static GenericMap Create( IReadOnlyList<IType> typeArguments, bool isIdentity ) => new( typeArguments, isIdentity );

    public bool IsEmptyOrIdentity { get; }

    public IReadOnlyList<IType> TypeArguments { get; }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    public static readonly GenericMap Empty;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    public GenericMap Apply( in GenericMap map )
    {
        if ( map.IsEmptyOrIdentity )
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

        return new GenericMap( typeArguments: results, false );
    }

    public IType Map( ITypeParameter typeParameter )
    {
        if ( typeParameter.ContainingDeclaration.AssertNotNull().DeclarationKind != DeclarationKind.NamedType )
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
            if ( !other.TypeArguments[i].Equals( this.TypeArguments[i] ) )
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals( object? obj ) => obj is GenericMap genericMap && this.Equals( genericMap );

    public override int GetHashCode()
    {
        var hashCode = default(HashCode);

        foreach ( var type in this.TypeArguments )
        {
            hashCode.Add( type.GetHashCode() );
        }

        return hashCode.ToHashCode();
    }

    public static bool operator ==( GenericMap left, GenericMap right ) => left.Equals( right );

    public static bool operator !=( GenericMap left, GenericMap right ) => !left.Equals( right );

    GenericMap IGenericContextImpl.GenericMap => this;
}