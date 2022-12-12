// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class DesignTimeValidatorCollection
{
    public static DesignTimeValidatorCollection Empty { get; } = new( ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeValidatorInstance>.Empty );

    public bool IsEmpty => this._dictionary.IsEmpty;

    public DesignTimeValidatorCollectionEqualityKey EqualityKey { get; }

    private readonly ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeValidatorInstance> _dictionary;

    private DesignTimeValidatorCollection( ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeValidatorInstance> dictionary )
    {
        this._dictionary = dictionary;
        this.EqualityKey = ComputeValidatorHash( dictionary );
    }

    internal ImmutableHashSet<DesignTimeValidatorInstance> GetValidatorsForSymbol( ISymbol symbol )
    {
        var symbolKey = SymbolDictionaryKey.CreateLookupKey( symbol );

        return this._dictionary[symbolKey];
    }

    private static DesignTimeValidatorCollectionEqualityKey ComputeValidatorHash(
        ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeValidatorInstance> validators )
    {
        XXH64 hasher = new();
        ulong combined = 0;

        foreach ( var group in validators )
        {
            foreach ( var validator in group )
            {
                hasher.Reset();
                validator.GetLongHashCode( hasher );

                // XOR is a poor hashing function but, to compute `combined`, we must have a _commutative_ hashing function
                // because our input is unordered. Ordering the input would increase significantly increase the computation time
                // and we would loose the benefit of a hash over structural equality comparison.
                combined ^= hasher.Digest();
            }
        }

        return new DesignTimeValidatorCollectionEqualityKey( combined );
    }

    public Builder ToBuilder() => new( this._dictionary.ToBuilder() );

    public sealed class Builder
    {
        private readonly ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeValidatorInstance>.Builder _builder;

        public Builder( ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeValidatorInstance>.Builder builder )
        {
            this._builder = builder;
        }

        public void Remove( DesignTimeValidatorInstance validator ) => this._builder.Remove( validator.ValidatedDeclaration, validator );

        public void Add( DesignTimeValidatorInstance validator ) => this._builder.Add( validator.ValidatedDeclaration, validator );

        public DesignTimeValidatorCollection ToImmutable() => new( this._builder.ToImmutable() );
    }
}