// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    public sealed partial class DerivedTypeIndex
    {
        private readonly CompilationContext _compilationContext;

        // Maps a base type to direct derived types.
        private readonly ImmutableDictionaryOfArray<INamedTypeSymbol, INamedTypeSymbol> _relationships;
        private readonly ImmutableHashSet<INamedTypeSymbol> _processedTypes;

        private DerivedTypeIndex(
            CompilationContext compilationContext,
            ImmutableDictionaryOfArray<INamedTypeSymbol, INamedTypeSymbol> relationships,
            ImmutableHashSet<INamedTypeSymbol> processedTypes )
        {
            this._relationships = relationships;
            this._processedTypes = processedTypes;
            this._compilationContext = compilationContext;
        }

        private bool IsContainedInCurrentCompilation( INamedTypeSymbol type )
            => this._compilationContext.SymbolComparer.Equals( this._compilationContext.Compilation.Assembly, type.ContainingAssembly );

        internal IEnumerable<INamedTypeSymbol> GetDerivedTypesInCurrentCompilation( INamedTypeSymbol baseType, DerivedTypesOptions options )
            => options switch
            {
                DerivedTypesOptions.All => this.GetAllDerivedTypesCore( baseType ),
                DerivedTypesOptions.DirectOnly => this.GetDirectlyDerivedTypesCore( baseType ),
                DerivedTypesOptions.FirstLevelWithinCompilationOnly => this.GetFirstLevelDerivedTypesCore( baseType ),
                DerivedTypesOptions.IncludingExternalTypesDangerous => this.GetDerivedTypes( baseType ),
                _ => throw new ArgumentOutOfRangeException( nameof(options), $"Unexpected value '{options}'." )
            };

        internal IEnumerable<INamedTypeSymbol> GetDerivedTypes( INamedTypeSymbol baseType )
            => this._relationships[baseType]
                .SelectManyRecursiveDistinct( t => this._relationships[t] );

        private IEnumerable<INamedTypeSymbol> GetAllDerivedTypesCore( INamedTypeSymbol baseType )
            => this.GetDerivedTypes( baseType )
                .Where( this.IsContainedInCurrentCompilation );

        private IEnumerable<INamedTypeSymbol> GetDirectlyDerivedTypesCore( INamedTypeSymbol baseType )
        {
            foreach ( var type in this._relationships[baseType] )
            {
                if ( this.IsContainedInCurrentCompilation( type ) )
                {
                    yield return type;
                }
            }
        }

        private IEnumerable<INamedTypeSymbol> GetFirstLevelDerivedTypesCore( INamedTypeSymbol baseType )
        {
            var set = new HashSet<INamedTypeSymbol>( this._compilationContext.SymbolComparer );
            GetDerivedTypesRecursive( baseType );

            return set;

            void GetDerivedTypesRecursive( INamedTypeSymbol parentType )
            {
                foreach ( var type in this._relationships[parentType] )
                {
                    if ( this.IsContainedInCurrentCompilation( type ) )
                    {
                        set.Add( type );
                    }
                    else
                    {
                        GetDerivedTypesRecursive( type );
                    }
                }
            }
        }

        internal DerivedTypeIndex WithIntroducedInterfaces( IEnumerable<IIntroduceInterfaceTransformation> introducedInterfaces )
        {
            Builder? builder = null;

            foreach ( var introducedInterface in introducedInterfaces )
            {
                builder ??= new Builder( this );

                var introducedInterfaceSymbol = introducedInterface.InterfaceType.GetSymbol()
                    .AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedInterfaceImplementation );

                if ( !introducedInterfaceSymbol.ContainingAssembly.Equals( this._compilationContext.Compilation.Assembly ) )
                {
                    // The type may not have been analyzed yet.
                    builder.AnalyzeType( introducedInterfaceSymbol );
                }

                builder.AddDerivedType(
                    introducedInterfaceSymbol,
                    introducedInterface.TargetType.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedInterfaceImplementation ) );
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
            foreach ( var baseType in this._relationships.Keys )
            {
                if ( !baseType.OriginalDefinition.DeclaringSyntaxReferences.IsDefaultOrEmpty )
                {
                    this.PopulateDependenciesCore( collector, baseType, baseType );
                }
            }
        }

        private void PopulateDependenciesCore( IDependencyCollector collector, INamedTypeSymbol rootType, INamedTypeSymbol baseType )
        {
            foreach ( var derivedType in this._relationships[baseType] )
            {
                collector.AddDependency( rootType, derivedType );
                this.PopulateDependenciesCore( collector, rootType, derivedType );
            }
        }

        internal DerivedTypeIndex WithAdditionalAnalyzedTypes( IEnumerable<INamedTypeSymbol> types )
        {
            Builder? builder = null;

            foreach ( var type in types )
            {
                if ( !this._processedTypes.Contains( type ) )
                {
                    builder ??= new Builder( this );
                    builder.AnalyzeType( type );
                }
            }

            if ( builder == null )
            {
                return this;
            }
            else
            {
                return builder.ToImmutable();
            }
        }
    }
}