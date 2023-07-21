// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class DesignTimeReferenceValidatorCollection
{
    private readonly ImmutableArray<DesignTimeReferenceValidatorCollection> _childCollections;
    private readonly ReferenceValidatorCollectionProperties _ownProperties;

    public static DesignTimeReferenceValidatorCollection Empty { get; } =
        new(
            ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance>.Empty,
            ImmutableArray<DesignTimeReferenceValidatorCollection>.Empty );

    public bool IsEmpty => this._dictionary.IsEmpty && this._childCollections.Length == 0;

    private readonly ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance> _dictionary;

    public ReferenceValidatorCollectionProperties Properties { get; }

    private DesignTimeReferenceValidatorCollection(
        ImmutableDictionaryOfHashSet<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance> dictionary,
        IEnumerable<DesignTimeReferenceValidatorCollection> childCollections )
    {
        this._dictionary = dictionary;
        this._childCollections = childCollections.Where( c => !c.IsEmpty ).ToImmutableArray();
        this._ownProperties = new ReferenceValidatorCollectionProperties( dictionary.SelectMany( x => x ) );
        this.Properties = new ReferenceValidatorCollectionProperties( this._childCollections.Select( x => x.Properties ).Concat( this._ownProperties ) );
    }

    private DesignTimeReferenceValidatorCollection(
        DesignTimeReferenceValidatorCollection prototype,
        IEnumerable<DesignTimeReferenceValidatorCollection> childCollections )
    {
        this._dictionary = prototype._dictionary;
        this._ownProperties = prototype._ownProperties;
        this._childCollections = childCollections.Where( c => !c.IsEmpty ).ToImmutableArray();
        this.Properties = new ReferenceValidatorCollectionProperties( this._childCollections.Select( x => x.Properties ).Concat( this._ownProperties ) );
    }

    public DesignTimeReferenceValidatorCollection WithChildCollections( IEnumerable<DesignTimeReferenceValidatorCollection> childCollections )
        => new( this, childCollections );

    internal IReadOnlyCollection<DesignTimeReferenceValidatorInstance> GetValidatorsForSymbol( ISymbol symbol )
    {
        var symbolKey = SymbolDictionaryKey.CreateLookupKey( symbol );
        var result = (IReadOnlyCollection<DesignTimeReferenceValidatorInstance>) this._dictionary[symbolKey];
        List<DesignTimeReferenceValidatorInstance>? mergedList = null;

        foreach ( var childCollection in this._childCollections )
        {
            var childResult = childCollection.GetValidatorsForSymbol( symbol );

            if ( childResult.Count > 0 )
            {
                if ( result.Count == 0 )
                {
                    result = childResult;
                }
                else
                {
                    // It should be unlikely that several projects add validation to the same declaration.
                    // When this happens, we use a lightweight strategy to merge the collections.

                    if ( mergedList == null )
                    {
                        mergedList = new List<DesignTimeReferenceValidatorInstance>();
                        mergedList.AddRange( result );
                    }

                    mergedList.AddRange( childResult );
                }
            }
        }

        return mergedList ?? result;
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

        public void Remove( DesignTimeReferenceValidatorInstance validator )
        {
            if ( !this._builder.Remove( validator.ValidatedDeclaration, validator ) )
            {
#if DEBUG
                throw new AssertionFailedException( "Cannot remove validator." );
#endif
            }
        }

        public void Add( DesignTimeReferenceValidatorInstance validator ) => this._builder.Add( validator.ValidatedDeclaration, validator );

        public DesignTimeReferenceValidatorCollection ToImmutable( IEnumerable<DesignTimeReferenceValidatorCollection> childCollections )
            => new( this._builder.ToImmutable(), childCollections );
    }
}