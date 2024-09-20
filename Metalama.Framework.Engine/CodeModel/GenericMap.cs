// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel;

internal partial class GenericMap : IEquatable<GenericMap?>, IGenericContextImpl
{
    public static GenericMap Empty { get; } = new( [], true, null, null );

    // The implementation is currently hard-coded with ITypeSymbol[] for performance
    // and because we don't support non-symbol types yet.
    public IReadOnlyList<ITypeSymbol> TypeArguments { get; }

    public CompilationContext? CompilationContext { get; }

    public GenericMap? Parent { get; }

    private GenericMap( IReadOnlyList<ITypeSymbol> typeArguments, bool isIdentity, CompilationContext? compilationContext, GenericMap? parent )
    {
        this.TypeArguments = typeArguments;
        this.CompilationContext = compilationContext;
        this.Parent = parent;
        this.IsEmptyOrIdentity = isIdentity || typeArguments.Count == 0;
    }

    public static GenericMap Get( ISymbol? symbol, CompilationContext compilationContext )
    {
        var closestType = symbol?.GetClosestContainingType();

        if ( closestType is null )
        {
            return Empty;
        }
        else
        {
            return Get( closestType, compilationContext );
        }
    }

    public static GenericMap Get( INamedTypeSymbol? symbol, CompilationContext compilationContext )
    {
        if ( symbol == null || symbol.IsDefinition )
        {
            return Empty;
        }

        return new GenericMap( symbol.TypeArguments, symbol.IsDefinition, compilationContext, Get( symbol.ContainingType, compilationContext ) );
    }

    public bool IsEmptyOrIdentity { get; }

    [Memo]
    private TypeSymbolMapper TypeSymbolMapperInstance => new( this );

    [Memo]
    private TypeMapper TypeMapperInstance => new( this );

    [Memo]
    private SymbolMapper SymbolMapperInstance => new( this );

    public GenericMap Apply( GenericMap map )
    {
        if ( map.IsEmptyOrIdentity )
        {
            return Empty;
        }

        if ( this.IsEmptyOrIdentity )
        {
            return map;
        }

        Invariant.Assert( this.CompilationContext == map.CompilationContext );

        var results = new ITypeSymbol[map.TypeArguments.Count];
        TypeSymbolMapper? mapper = null;

        for ( var i = 0; i < map.TypeArguments.Count; i++ )
        {
            var type = map.TypeArguments[i];

            if ( type is ITypeParameterSymbol typeParameter )
            {
                results[i] = this.Map( typeParameter );
            }
            else
            {
                mapper ??= new TypeSymbolMapper( this );
                results[i] = mapper.Visit( type );
            }
        }

        // TODO: Nested (parent).
        return new GenericMap( typeArguments: results, false, this.CompilationContext!, null );
    }

    public ITypeSymbol Map( ITypeParameterSymbol typeParameter )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return typeParameter;
        }

        if ( typeParameter.ContainingSymbol.AssertSymbolNotNull().Kind != SymbolKind.NamedType )
        {
            throw new NotImplementedException( "Method type parameters are not supported." );
        }

        return this.TypeArguments[typeParameter.Ordinal];
    }

    public IType Map( ITypeParameter typeParameter )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return typeParameter;
        }

        if ( typeParameter.ContainingDeclaration.AssertNotNull().DeclarationKind != DeclarationKind.NamedType )
        {
            throw new NotImplementedException( "Method type parameters are not supported." );
        }

        return typeParameter.GetCompilationModel().Factory.GetIType( this.TypeArguments[typeParameter.Index] );
    }

    [return: NotNullIfNotNull( nameof(type) )]
    public IType? Map( IType? type )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return type;
        }

        return type switch
        {
            null => null,
            ITypeParameter typeParameter => this.Map( typeParameter ),
            _ => TypeVisitor.Instance.Visit( type ) ? this.TypeMapperInstance.Visit( type ) : null
        };
    }

    [return: NotNullIfNotNull( nameof(type) )]
    public ITypeSymbol? Map( ITypeSymbol? type )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return type;
        }

        return type switch
        {
            null => null,
            ITypeParameterSymbol typeParameter => this.Map( typeParameter ),
            _ => TypeSymbolVisitor.Instance.Visit( type ) ? this.TypeSymbolMapperInstance.Visit( type ) : null
        };
    }

    [return: NotNullIfNotNull( nameof(symbol) )]
    public ISymbol? Map( ISymbol? symbol, CompilationContext compilationContext )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return symbol;
        }

        if ( symbol == null )
        {
            return null;
        }

        return this.SymbolMapperInstance.Visit( symbol ).AssertSymbolNotNull();
    }

    public bool Equals( GenericMap? other )
    {
        if ( other == null )
        {
            return false;
        }

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

        hashCode.Add( this.Parent );

        return hashCode.ToHashCode();
    }

    GenericMap IGenericContextImpl.GenericMap => this;
}