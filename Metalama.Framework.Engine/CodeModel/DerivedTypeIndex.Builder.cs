// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Collections;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

public partial class DerivedTypeIndex
{
    public sealed class Builder
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
            var externalBaseTypes = this._processedTypes.Where( t => !t.ContainingAssembly.Equals( this._compilation.Assembly ) )
                .ToImmutableHashSet<INamedTypeSymbol>( SymbolEqualityComparer.Default );

            return new DerivedTypeIndex(
                this._compilation,
                this._relationships.ToImmutable(),
                externalBaseTypes );
        }
    }
}