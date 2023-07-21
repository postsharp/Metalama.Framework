// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class DesignTimeReferenceValidatorCollection
{
    private readonly ReferenceValidatorCollectionProperties _ownProperties;
    private readonly ImmutableDictionaryOfArray<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance> _ownValidators;
    private readonly ImmutableDictionaryOfArray<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance> _allValidators;
    private readonly HashSet<DesignTimeReferenceValidatorCollection> _validatorCollectionsFromProjectReferences;

    public bool IsEmpty => this._allValidators.IsEmpty;

    public static DesignTimeReferenceValidatorCollection Empty { get; } =
        new(
            ReferenceValidatorCollectionProperties.Empty,
            ImmutableDictionaryOfArray<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance>.Empty,
            ImmutableArray<DesignTimeReferenceValidatorCollection>.Empty );

    public ReferenceValidatorCollectionProperties Properties { get; }

    private DesignTimeReferenceValidatorCollection(
        ReferenceValidatorCollectionProperties ownProperties,
        ImmutableDictionaryOfArray<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance> ownValidators,
        IEnumerable<DesignTimeReferenceValidatorCollection> validatorsFromProjectReferences )
    {
        this._ownProperties = ownProperties;
        this._ownValidators = ownValidators;

        this._validatorCollectionsFromProjectReferences =
            validatorsFromProjectReferences.SelectManyRecursiveDistinct( x => x._validatorCollectionsFromProjectReferences, includeRoots: true );

        this._allValidators = this._ownValidators.Merge( this._validatorCollectionsFromProjectReferences.Select( x => x._ownValidators ) );

        this.Properties = new ReferenceValidatorCollectionProperties(
            this._validatorCollectionsFromProjectReferences.Select( x => x.Properties ).Concat( this._ownProperties ) );
    }

    public DesignTimeReferenceValidatorCollection WithChildCollections( IEnumerable<DesignTimeReferenceValidatorCollection> childCollections )
        => new( this._ownProperties, this._ownValidators, childCollections );

    internal IReadOnlyCollection<DesignTimeReferenceValidatorInstance> GetValidatorsForSymbol( ISymbol symbol )
    {
        var symbolKey = SymbolDictionaryKey.CreateLookupKey( symbol );

        var validators = this._allValidators[symbolKey];

        if ( validators.IsDefault )
        {
            throw new AssertionFailedException();
        }

        return validators;
    }

    public Builder ToBuilder() => new( this._ownValidators.ToBuilder() );

    public ImmutableArray<TransitiveValidatorInstance> ToTransitiveValidatorInstances()
    {
        var builder = ImmutableArray.CreateBuilder<TransitiveValidatorInstance>();

        foreach ( var key in this._ownValidators.Keys )
        {
            foreach ( var validator in this._ownValidators[key] )
            {
                builder.Add( validator.ToTransitiveValidatorInstance() );
            }
        }

        return builder.ToImmutable();
    }

    public sealed class Builder
    {
        private readonly ImmutableDictionaryOfArray<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance>.Builder _builder;

        public Builder( ImmutableDictionaryOfArray<SymbolDictionaryKey, DesignTimeReferenceValidatorInstance>.Builder builder )
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
        {
            var ownValidators = this._builder.ToImmutable();

            return new DesignTimeReferenceValidatorCollection(
                new ReferenceValidatorCollectionProperties( ownValidators.SelectMany( x => x ) ),
                ownValidators,
                childCollections );
        }
    }
}