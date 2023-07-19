// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class DesignTimeReferenceValidatorCollection
{
    public static DesignTimeReferenceValidatorCollection Empty { get; } =
        new( ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance>.Empty );

    public bool IsEmpty => this._dictionary.IsEmpty;

    public DesignTimeValidatorCollectionEqualityKey EqualityKey { get; }

    private readonly ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance> _dictionary;

    private DesignTimeReferenceValidatorCollection( ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance> dictionary )
    {
        this._dictionary = dictionary;
        this.EqualityKey = ComputeValidatorHash( dictionary );
    }

    internal ImmutableHashSet<DesignTimeReferenceValidatorInstance> GetValidatorsForSymbol( ISymbol symbol )
    {
        var symbolKey = SymbolDictionaryKey.CreateLookupKey( symbol );

        return this._dictionary[symbolKey];
    }

    private static DesignTimeValidatorCollectionEqualityKey ComputeValidatorHash(
        ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance> validators )
    {
        XXH64 hasher = new();
        ulong combined = 0;

        foreach ( var group in validators )
        {
            foreach ( var validator in group )
            {
                hasher.Reset();
                var digest = validator.GetLongHashCode( hasher );

                // XOR is a poor hashing function but, to compute `combined`, we must have a _commutative_ hashing function
                // because our input is unordered. Ordering the input would increase significantly increase the computation time
                // and we would loose the benefit of a hash over structural equality comparison.
                combined ^= digest;
            }
        }

        return new DesignTimeValidatorCollectionEqualityKey( combined );
    }

    public Builder ToBuilder() => new( this._dictionary.ToBuilder() );

    public ImmutableArray<TransitiveValidatorInstance> ToTransitiveValidatorInstances()
    {
        var builder = ImmutableArray.CreateBuilder<TransitiveValidatorInstance>();

        foreach ( var key in this._dictionary.Keys )
        {
            foreach ( var validator in this._dictionary[key] )
            {
                builder.Add( validator.ToTransitiveValidatorInstance() );
            }
        }

        return builder.ToImmutable();
    }

    public sealed class Builder
    {
        private readonly ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance>.Builder _builder;

        public Builder( ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance>.Builder builder )
        {
            this._builder = builder;
        }

        public void Remove( DesignTimeReferenceValidatorInstance validator ) => this._builder.Remove( validator.ValidatedDeclaration, validator );

        public void Add( DesignTimeReferenceValidatorInstance validator ) => this._builder.Add( validator.ValidatedDeclaration, validator );

        public DesignTimeReferenceValidatorCollection ToImmutable() => new( this._builder.ToImmutable() );
    }
}