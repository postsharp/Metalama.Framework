﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using OurSpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.CodeModel.GenericContexts;

public abstract partial class GenericContext : IEquatable<GenericContext?>, IGenericContext
{
    public static GenericContext Empty { get; } = new NullGenericContext();

    public bool IsEmptyOrIdentity => this.Kind == GenericContextKind.Null;

    internal abstract GenericContextKind Kind { get; }

    [Memo]
    private TypeMapper TypeMapperInstance => new( this );

    internal abstract ImmutableArray<IFullRef<IType>> TypeArguments { get; }

    internal abstract IType Map( ITypeParameter typeParameter );

    internal abstract IType Map( ITypeParameterSymbol typeParameterSymbol, CompilationModel compilation );

    internal IFullRef<IType> Map( ITypeSymbol typeSymbol, RefFactory refFactory )
    {
        if ( !ReferencesTypeParameter( typeSymbol ) )
        {
            return refFactory.FromSymbol<IType>( typeSymbol );
        }

        var mapper = new SymbolToTypeMapper( this, refFactory.CanonicalCompilation );

        return mapper.Visit( typeSymbol ).AssertNotNull().ToFullRef();
    }

    [return: NotNullIfNotNull( nameof(type) )]
    internal IType? Map( IType? type )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return type;
        }

        return type switch
        {
            null => null,
            ITypeParameter typeParameter => this.Map( typeParameter ),
            _ when type.SpecialType != OurSpecialType.None || !TypeParameterDetector.Instance.Visit( type ) => type, // Fast path
            _ => this.TypeMapperInstance.Visit( type )
        };
    }

    internal abstract GenericContext Map( GenericContext genericContext, RefFactory refFactory );

    public abstract bool Equals( GenericContext? other );

    protected abstract int GetHashCodeCore();

    public override int GetHashCode() => this.GetHashCodeCore();

    public sealed override bool Equals( object? obj ) => obj is GenericContext genericMap && this.Equals( genericMap );

    private protected static bool ReferencesTypeParameter( ITypeSymbol typeSymbol ) => TypeParameterSymbolDetector.Instance.Visit( typeSymbol );
}