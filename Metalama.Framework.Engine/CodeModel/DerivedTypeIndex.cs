// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Comparers;
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
        private readonly ImmutableDictionaryOfArray<NamedType, NamedType> _relationships;
        private readonly ImmutableHashSet<NamedType> _processedTypes;

        private DerivedTypeIndex(
            CompilationContext compilationContext,
            ImmutableDictionaryOfArray<NamedType, NamedType> relationships,
            ImmutableHashSet<NamedType> processedTypes )
        {
            this._relationships = relationships;
            this._processedTypes = processedTypes;
            this._compilationContext = compilationContext;
        }

        private bool IsContainedInCurrentCompilation( INamedType type )
            => this.IsCurrentCompilation( type.DeclaringAssembly.GetSymbol() );

        private bool IsContainedInCurrentCompilation( NamedType type )
            => this.IsCurrentCompilation( type.Symbol?.ContainingAssembly ?? type.IType!.DeclaringAssembly.GetSymbol() );

        private bool IsCurrentCompilation( IAssemblySymbol assembly )
            => this._compilationContext.SymbolComparer.Equals( this._compilationContext.Compilation.Assembly, assembly );

        internal IEnumerable<INamedType> GetDerivedTypesInCurrentCompilation( INamedType baseType, DerivedTypesOptions options )
            => options switch
            {
                DerivedTypesOptions.All => this.GetAllDerivedTypesCore( baseType ),
                DerivedTypesOptions.DirectOnly => this.GetDirectlyDerivedTypesCore( baseType ),
                DerivedTypesOptions.FirstLevelWithinCompilationOnly => this.GetFirstLevelDerivedTypesCore( baseType ),
                DerivedTypesOptions.IncludingExternalTypesDangerous => this.GetDerivedTypes( baseType ),
                _ => throw new ArgumentOutOfRangeException( nameof( options ), $"Unexpected value '{options}'." )
            };

        internal IEnumerable<INamedType> GetDerivedTypes( INamedType baseType )
            => this._relationships[new( baseType )]
                .SelectManyRecursiveDistinct( t => this._relationships[t], this._processedTypes.KeyComparer )
                .SelectAsArray( nt => nt.ToIType( baseType.GetCompilationModel() ) );

        private IEnumerable<INamedType> GetAllDerivedTypesCore( INamedType baseType )
            => this.GetDerivedTypes( baseType )
                .Where( this.IsContainedInCurrentCompilation );

        private IEnumerable<INamedType> GetDirectlyDerivedTypesCore( INamedType baseType )
        {
            foreach ( var namedType in this._relationships[new( baseType )] )
            {
                if ( this.IsContainedInCurrentCompilation( namedType ) )
                {
                    yield return namedType.ToIType( baseType.GetCompilationModel() );
                }
            }
        }

        private IEnumerable<INamedType> GetFirstLevelDerivedTypesCore( INamedType baseType )
        {
            var set = new HashSet<INamedType>( StructuralDeclarationComparer.Default );
            GetDerivedTypesRecursive( new( baseType ) );

            return set;

            void GetDerivedTypesRecursive( NamedType parentType )
            {
                foreach ( var type in this._relationships[parentType] )
                {
                    if ( this.IsContainedInCurrentCompilation( type ) )
                    {
                        set.Add( type.ToIType( baseType.GetCompilationModel() ) );
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

            foreach ( var transformation in introducedInterfaces )
            {
                builder ??= new Builder( this );

                var introducedInterface = transformation.InterfaceType;

                if ( !introducedInterface.DeclaringAssembly.GetSymbol().Equals( this._compilationContext.Compilation.Assembly ) )
                {
                    // The type may not have been analyzed yet.
                    builder.AnalyzeType( introducedInterface );
                }

                builder.AddDerivedType( introducedInterface, transformation.TargetType );
            }

            return builder?.ToImmutable() ?? this;
        }

        internal DerivedTypeIndex WithIntroducedTypes( IEnumerable<IntroduceNamedTypeTransformation> introducedTypes )
        {
            Builder? builder = null;

            foreach ( var transformation in introducedTypes )
            {
                builder ??= new Builder( this );

                var introducedType = transformation.IntroducedDeclaration;

                if ( introducedType.BaseType is { } baseType )
                {
                    builder.AddDerivedType( baseType, introducedType );
                }

                foreach ( var implementedInterface in introducedType.ImplementedInterfaces )
                {
                    builder.AddDerivedType( implementedInterface, introducedType );
                }
            }

            return builder?.ToImmutable() ?? this;
        }

        public void PopulateDependencies( IDependencyCollector collector )
        {
            foreach ( var baseType in this._relationships.Keys )
            {
                if ( baseType.Symbol is { } baseTypeSymbol && !baseTypeSymbol.OriginalDefinition.DeclaringSyntaxReferences.IsDefaultOrEmpty )
                {
                    this.PopulateDependenciesCore( collector, baseTypeSymbol, baseType );
                }
            }
        }

        private void PopulateDependenciesCore( IDependencyCollector collector, INamedTypeSymbol rootType, NamedType baseType )
        {
            foreach ( var derivedType in this._relationships[baseType] )
            {
                if ( derivedType.Symbol is { } derivedTypeSymbol )
                {
                    collector.AddDependency( rootType, derivedTypeSymbol );
                    this.PopulateDependenciesCore( collector, rootType, derivedType );
                }
            }
        }

        internal DerivedTypeIndex WithAdditionalAnalyzedTypes( IEnumerable<INamedTypeSymbol> types )
        {
            Builder? builder = null;

            foreach ( var type in types )
            {
                if ( !this._processedTypes.Contains( new( type ) ) )
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

        internal DerivedTypeIndex WithAdditionalAnalyzedTypes( IEnumerable<INamedType> types )
        {
            Builder? builder = null;

            foreach ( var type in types )
            {
                if ( !this._processedTypes.Contains( new( type ) ) )
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