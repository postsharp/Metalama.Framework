﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

public partial class DerivedTypeIndex
{
    internal sealed class Builder
    {
        private readonly CompilationContext _compilationContext;
        private readonly ImmutableDictionaryOfArray<INamedTypeSymbol, INamedTypeSymbol>.Builder _relationships;

        private readonly ImmutableHashSet<INamedTypeSymbol>.Builder _processedTypes;

        internal Builder( CompilationContext compilationContext )
        {
            this._compilationContext = compilationContext;
            this._relationships = new ImmutableDictionaryOfArray<INamedTypeSymbol, INamedTypeSymbol>.Builder( compilationContext.SymbolComparer );
            this._processedTypes = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>( compilationContext.SymbolComparer );
        }

        internal Builder(
            CompilationContext compilationContext,
            ImmutableDictionaryOfArray<INamedTypeSymbol, INamedTypeSymbol>.Builder relationships,
            ImmutableHashSet<INamedTypeSymbol>.Builder processedTypes )
        {
            this._compilationContext = compilationContext;
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
            var externalBaseTypes = this._processedTypes.Where( t => !t.ContainingAssembly.Equals( this._compilationContext.Compilation.Assembly ) )
                .ToImmutableHashSet<INamedTypeSymbol>( this._compilationContext.SymbolComparer );

            return new DerivedTypeIndex(
                this._compilationContext,
                this._relationships.ToImmutable(),
                externalBaseTypes );
        }
    }
}