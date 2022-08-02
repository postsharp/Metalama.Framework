// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class DerivedTypeIndex
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

        public DerivedTypeIndex WithIntroducedInterfaces( IEnumerable<IIntroduceInterfaceTransformation> introducedInterfaces )
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

        public class Builder
        {
            private readonly Compilation _compilation;
            private readonly ImmutableDictionaryOfArray<INamedTypeSymbol, INamedTypeSymbol>.Builder _relationships;

            private readonly ImmutableHashSet<INamedTypeSymbol>.Builder _processedTypes;

            public Builder( Compilation compilation )
            {
                this._compilation = compilation;
                this._relationships = new ImmutableDictionaryOfArray<INamedTypeSymbol, INamedTypeSymbol>.Builder( SymbolEqualityComparer.Default );
                this._processedTypes = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>( SymbolEqualityComparer.Default );
            }

            internal Builder(
                Compilation compilation,
                ImmutableDictionaryOfArray<INamedTypeSymbol, INamedTypeSymbol>.Builder relationships,
                ImmutableHashSet<INamedTypeSymbol>.Builder processedTypes )
            {
                this._compilation = compilation;
                this._relationships = relationships;
                this._processedTypes = processedTypes;
            }

            public void AnalyzeType( INamedTypeSymbol type, IDependencyCollector? dependencyCollector = null )
            {
                if ( !this._processedTypes.Add( type ) )
                {
                    return;
                }

                if ( type.BaseType != null )
                {
                    var baseType = type.BaseType.OriginalDefinition;
                    this._relationships.Add( baseType, type );
                    dependencyCollector?.AddDependency( baseType, type );
                    this.AnalyzeType( baseType, dependencyCollector );
                }

                foreach ( var interfaceImpl in type.Interfaces )
                {
                    var interfaceType = interfaceImpl.OriginalDefinition;
                    this._relationships.Add( interfaceType, type );
                    dependencyCollector?.AddDependency( interfaceType, type );
                    this.AnalyzeType( interfaceType, dependencyCollector );
                }

                foreach ( var nestedType in type.GetTypeMembers() )
                {
                    this.AnalyzeType( nestedType, dependencyCollector );
                }
            }

            public void AddDerivedType( INamedTypeSymbol baseType, INamedTypeSymbol derivedType ) => this._relationships.Add( baseType, derivedType );

            public DerivedTypeIndex ToImmutable()
            {
                var externalBaseTypes = this._processedTypes.Where( t => !t.ContainingAssembly.Equals( this._compilation.Assembly ) )
                    .ToImmutableHashSet<INamedTypeSymbol>( SymbolEqualityComparer.Default );

                return new DerivedTypeIndex(
                    this._compilation,
                    this._relationships.ToImmutable(),
                    externalBaseTypes );
            }
        }
    }
}