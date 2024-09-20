﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.CodeModel;

internal partial class GenericContext : IEquatable<GenericContext?>, IGenericContext
{
    public static GenericContext Empty { get; } = new( null, null );

    public INamedTypeSymbol? NamedTypeSymbol { get; }

    public CompilationContext? CompilationContext { get; }

    public bool IsEmptyOrIdentity => this.NamedTypeSymbol is null or { IsDefinition: true };

    private GenericContext( INamedTypeSymbol? namedTypeSymbol, CompilationContext? compilationContext )
    {
        this.NamedTypeSymbol = namedTypeSymbol;
        this.CompilationContext = compilationContext;
    }

    public static GenericContext Get( ISymbol? symbol, CompilationContext compilationContext )
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

    public static GenericContext Get( INamedTypeSymbol? symbol, CompilationContext compilationContext )
    {
        if ( symbol == null || symbol.IsDefinition )
        {
            return Empty;
        }

        return new GenericContext( symbol, compilationContext );
    }

    [Memo]
    private TypeSymbolMapper TypeSymbolMapperInstance => new( this );

    [Memo]
    private TypeMapper TypeMapperInstance => new( this );

    [Memo]
    private SymbolMapper SymbolMapperInstance => new( this );

    public ITypeSymbol Map( ITypeParameterSymbol typeParameter )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return typeParameter;
        }

        if ( typeParameter.TypeParameterKind != TypeParameterKind.Type )
        {
            throw new NotImplementedException( "Method type parameters are not supported." );
        }

        // Find which type of the stack of nested types we have to consider.
        var requestedTypeDefinition = typeParameter.DeclaringType!.OriginalDefinition;

        for ( var type = this.NamedTypeSymbol; type != null; type = type.ContainingType )
        {
            if ( type.OriginalDefinition == requestedTypeDefinition )
            {
                return type.TypeArguments[typeParameter.Ordinal];
            }
        }

        // The type parameter cannot be matched. This can happen when we are trying to match a nested type A<T1>.B<T2> in the context of A<string>,
        // i.e. the top-level type is bound and the nested type is unbound.
        return typeParameter;
    }

    public IType Map( ITypeParameter typeParameter )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return typeParameter;
        }

        var mappedSymbol = this.Map( ((TypeParameter) typeParameter).TypeParameterSymbol );

        return typeParameter.GetCompilationModel().Factory.GetIType( mappedSymbol );
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
            _ when type.SpecialType != SpecialType.None || TypeVisitor.Instance.Visit( type ) => type, // Fast oath
            _ => this.TypeMapperInstance.Visit( type )
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

    public bool Equals( GenericContext? other )
    {
        if ( other == null )
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals( this.NamedTypeSymbol, other.NamedTypeSymbol );
    }

    public override bool Equals( object? obj ) => obj is GenericContext genericMap && this.Equals( genericMap );

    public override int GetHashCode()
    {
        return SymbolEqualityComparer.Default.GetHashCode( this.NamedTypeSymbol );
    }

    public override string ToString()
        => this.NamedTypeSymbol == null
            ? $"GenericContext (Empty)"
            : $"GenericContext Type={{{this.NamedTypeSymbol.ToDisplayString()}}}, IsIdentity={this.IsEmptyOrIdentity}";
}