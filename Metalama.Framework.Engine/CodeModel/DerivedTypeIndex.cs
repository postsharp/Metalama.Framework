// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Pipeline.DesignTime;
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
        private readonly Compilation _compilation;

        // Maps a base type to direct derived types.
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

        private bool IsContainedInCurrentCompilation( INamedTypeSymbol type )
            => SymbolEqualityComparer.Default.Equals( this._compilation.Assembly, type.ContainingAssembly );

        public IEnumerable<INamedTypeSymbol> GetDerivedTypesInCurrentCompilation( INamedTypeSymbol baseType, DerivedTypesOptions options )
        {
            return options switch
            {
                DerivedTypesOptions.All => this.GetAllDerivedTypesCore( baseType ),
                DerivedTypesOptions.DirectOnly => this.GetDirectlyDerivedTypesCore( baseType ),
                DerivedTypesOptions.FirstLevelWithinCompilationOnly => this.GetFirstLevelDerivedTypesCore( baseType ),
                _ => throw new ArgumentOutOfRangeException( nameof(options), $"Unexpected value '{options}'." )
            };
        }

        private IEnumerable<INamedTypeSymbol> GetAllDerivedTypesCore( INamedTypeSymbol baseType )
            => this._relationships[baseType]
                .SelectManyRecursive( t => this._relationships[t], throwOnDuplicate: false )
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
            var set = new HashSet<INamedTypeSymbol>( SymbolEqualityComparer.Default );
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
    }
}