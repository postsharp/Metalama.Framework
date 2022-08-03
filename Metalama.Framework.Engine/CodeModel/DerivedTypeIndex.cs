// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel
{
    public sealed partial class DerivedTypeIndex
    {
        private readonly Compilation _compilation;
        private readonly ImmutableDictionaryOfArray<INamedTypeSymbol, INamedTypeSymbol> _relationships;
        private readonly ImmutableHashSet<INamedTypeSymbol> _externalBaseTypes;

        private DerivedTypeIndex(
            Compilation compilation,
            ImmutableDictionaryOfArray<INamedTypeSymbol, INamedTypeSymbol> relationships,
            ImmutableHashSet<INamedTypeSymbol> externalBaseTypes )
        {
            this._relationships = relationships;
            this._externalBaseTypes = externalBaseTypes;
            this._compilation = compilation;
        }

        public ImmutableArray<INamedTypeSymbol> GetDerivedTypes( INamedTypeSymbol baseType, bool deep )
            => deep
                ? this._relationships[baseType].SelectManyRecursive( t => this._relationships[t] ).ToImmutableArray()
                : this._relationships[baseType];

        internal DerivedTypeIndex WithIntroducedInterfaces( IEnumerable<IIntroduceInterfaceTransformation> introducedInterfaces )
        {
            Builder? builder = null;

            foreach ( var introducedInterface in introducedInterfaces )
            {
                builder ??= new Builder( this._compilation, this._relationships.ToBuilder(), this._externalBaseTypes.ToBuilder() );

                var introducedInterfaceSymbol = introducedInterface.InterfaceType.GetSymbol().AssertNotNull();

                if ( !introducedInterfaceSymbol.ContainingAssembly.Equals( this._compilation.Assembly ) )
                {
                    // The type may not have been analyzed yet.
                    builder.AnalyzeType( introducedInterfaceSymbol );
                }

                builder.AddDerivedType( introducedInterfaceSymbol, introducedInterface.TargetType.GetSymbol().AssertNotNull() );
            }

            if ( builder != null )
            {
                return builder.ToImmutable();
            }
            else
            {
                return this;
            }
        }

        public void PopulateDependencies( IDependencyCollector collector )
        {
            foreach ( var baseType in this._relationships )
            {
                foreach ( var derivedType in baseType )
                {
                    collector.AddDependency( baseType.Key, derivedType );
                }
            }
        }
    }
}