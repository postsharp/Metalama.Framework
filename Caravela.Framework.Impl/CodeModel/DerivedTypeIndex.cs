// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class DerivedTypeIndex
    {
        private readonly Compilation _compilation;

        public ImmutableMultiValueDictionary<INamedTypeSymbol, INamedTypeSymbol> Relationships { get; }

        public ImmutableHashSet<INamedTypeSymbol> ExternalBaseTypes { get; }

        private DerivedTypeIndex(
            Compilation compilation,
            ImmutableMultiValueDictionary<INamedTypeSymbol, INamedTypeSymbol> relationships,
            ImmutableHashSet<INamedTypeSymbol> externalBaseTypes )
        {
            this.Relationships = relationships;
            this.ExternalBaseTypes = externalBaseTypes;
            this._compilation = compilation;
        }

        public ImmutableArray<INamedTypeSymbol> GetDerivedTypes( INamedTypeSymbol baseType )
            => this.Relationships[baseType].SelectManyRecursive( t => this.Relationships[t] ).ToImmutableArray();

        public DerivedTypeIndex WithIntroducedInterfaces( IEnumerable<IIntroducedInterface> introducedInterfaces )
        {
            Builder? builder = null;

            foreach ( var introducedInterface in introducedInterfaces )
            {
                builder ??= new Builder( this._compilation, this.Relationships.ToBuilder(), this.ExternalBaseTypes.ToBuilder() );

                var introducedInterfaceSymbol = introducedInterface.InterfaceType.GetSymbol().AssertNotNull();

                if ( introducedInterfaceSymbol.ContainingAssembly != this._compilation.Assembly )
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
            private readonly ImmutableMultiValueDictionary<INamedTypeSymbol, INamedTypeSymbol>.Builder _relationships;

            private readonly ImmutableHashSet<INamedTypeSymbol>.Builder _processedTypes;

            public Builder( Compilation compilation )
            {
                this._compilation = compilation;
                this._relationships = new ImmutableMultiValueDictionary<INamedTypeSymbol, INamedTypeSymbol>.Builder( SymbolEqualityComparer.Default );
                this._processedTypes = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>( SymbolEqualityComparer.Default );
            }

            internal Builder(
                Compilation compilation,
                ImmutableMultiValueDictionary<INamedTypeSymbol, INamedTypeSymbol>.Builder relationships,
                ImmutableHashSet<INamedTypeSymbol>.Builder processedTypes )
            {
                this._compilation = compilation;
                this._relationships = relationships;
                this._processedTypes = processedTypes;
            }

            public void AnalyzeType( INamedTypeSymbol type )
            {
                if ( !this._processedTypes.Add( type ) )
                {
                    return;
                }

                if ( type.BaseType != null )
                {
                    var baseType = type.BaseType.OriginalDefinition;
                    this._relationships.Add( baseType, type );
                    this.AnalyzeType( baseType );
                }

                foreach ( var interfaceImpl in type.Interfaces )
                {
                    var interfaceType = interfaceImpl.OriginalDefinition;
                    this._relationships.Add( interfaceType, type );
                    this.AnalyzeType( interfaceType );
                }

                foreach ( var nestedType in type.GetTypeMembers() )
                {
                    this.AnalyzeType( nestedType );
                }
            }

            public void AddDerivedType( INamedTypeSymbol baseType, INamedTypeSymbol derivedType ) => this._relationships.Add( baseType, derivedType );

            public DerivedTypeIndex ToImmutable()
            {
                var externalBaseTypes = this._processedTypes.Where( t => t.ContainingAssembly != this._compilation.Assembly )
                    .ToImmutableHashSet<INamedTypeSymbol>( SymbolEqualityComparer.Default );

                return new DerivedTypeIndex(
                    this._compilation,
                    this._relationships.ToImmutable(),
                    externalBaseTypes );
            }
        }
    }
}