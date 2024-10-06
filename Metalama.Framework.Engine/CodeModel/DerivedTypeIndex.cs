// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

public sealed partial class DerivedTypeIndex
{
    private readonly CompilationContext _compilationContext;

    // Maps a base type to direct derived types.
    private readonly ImmutableDictionaryOfArray<IRef<INamedType>, IRef<INamedType>> _relationships;
    private readonly ImmutableHashSet<IRef<INamedType>> _processedTypes;

    private DerivedTypeIndex(
        CompilationContext compilationContext,
        ImmutableDictionaryOfArray<IRef<INamedType>, IRef<INamedType>> relationships,
        ImmutableHashSet<IRef<INamedType>> processedTypes )
    {
        Invariant.Assert( ReferenceEquals( relationships.KeyComparer, RefEqualityComparer<INamedType>.Default ) );

        this._relationships = relationships;
        this._processedTypes = processedTypes;
        this._compilationContext = compilationContext;
    }

    private bool IsContainedInCurrentCompilation( IRef<INamedType> type )
    {
        switch ( type )
        {
            case ISymbolRef { Symbol: INamedTypeSymbol symbol }:
                return this.IsCurrentCompilation( symbol.ContainingAssembly );

            case IBuiltDeclarationRef { BuilderData: INamedTypeBuilder }:
                // Builders are always "in" the current Roslyn compilation.
                return true;

            default:
                throw new InvalidOperationException( $"Unexpected type {type}." );
        }
    }

    private bool IsCurrentCompilation( IAssemblySymbol assembly )
        => this._compilationContext.SymbolComparer.Equals( this._compilationContext.Compilation.Assembly, assembly );

    internal IEnumerable<INamedType> GetDerivedTypesInCurrentCompilation( INamedType baseType, DerivedTypesOptions options )
        => options switch
        {
            DerivedTypesOptions.All => this.GetAllDerivedTypes( baseType ),
            DerivedTypesOptions.DirectOnly => this.GetDirectlyDerivedTypes( baseType ),
            DerivedTypesOptions.FirstLevelWithinCompilationOnly => this.GetFirstLevelDerivedTypes( baseType ),
            DerivedTypesOptions.IncludingExternalTypesDangerous => this.GetDerivedTypes( baseType ),
            _ => throw new ArgumentOutOfRangeException( nameof(options), $"Unexpected value '{options}'." )
        };
    
    internal IEnumerable<IRef<INamedType>> GetDerivedTypesInCurrentCompilation( IRef<INamedType> baseType, DerivedTypesOptions options )
        => options switch
        {
            DerivedTypesOptions.All => this.GetAllDerivedTypesCore( baseType ),
            DerivedTypesOptions.DirectOnly => this.GetDirectlyDerivedTypesCore( baseType ),
            DerivedTypesOptions.FirstLevelWithinCompilationOnly => this.GetFirstLevelDerivedTypesCore( baseType ),
            DerivedTypesOptions.IncludingExternalTypesDangerous => this.GetDerivedTypesCore( baseType ),
            _ => throw new ArgumentOutOfRangeException( nameof(options), $"Unexpected value '{options}'." )
        };

    internal IEnumerable<INamedType> GetDerivedTypes( INamedType baseType )
        => this.GetDerivedTypesCore( baseType.ToRef() )
            .Select( nt => nt.GetTarget( baseType.Compilation ) );

    private IEnumerable<IRef<INamedType>> GetDerivedTypesCore( IRef<INamedType> baseType )
        => this._relationships[baseType]
            .SelectManyRecursiveDistinct( t => this._relationships[t], this._processedTypes.KeyComparer );

    
    private IEnumerable<INamedType> GetAllDerivedTypes( INamedType baseType )
        => this.GetAllDerivedTypesCore( baseType.ToRef() )
            .Select( nt => nt.GetTarget( baseType.Compilation ) );

    private IEnumerable<IRef<INamedType>> GetAllDerivedTypesCore( IRef<INamedType> baseType )
        => this.GetDerivedTypesCore( baseType )
            .Where( this.IsContainedInCurrentCompilation );

    private IEnumerable<INamedType> GetDirectlyDerivedTypes( INamedType baseType )
        => this.GetDirectlyDerivedTypesCore( baseType.ToRef() )
            .Select( nt => nt.GetTarget( baseType.Compilation ) );

    private IEnumerable<IRef<INamedType>> GetDirectlyDerivedTypesCore( IRef<INamedType> baseType )
    {
        foreach ( var namedType in this._relationships[baseType] )
        {
            if ( this.IsContainedInCurrentCompilation( namedType ) )
            {
                yield return namedType;
            }
        }
    }

    private IEnumerable<INamedType> GetFirstLevelDerivedTypes( INamedType baseType )
        => this.GetFirstLevelDerivedTypesCore( baseType.ToRef() )
            .Select( nt => nt.GetTarget( baseType.Compilation ) );

    private IEnumerable<IRef<INamedType>> GetFirstLevelDerivedTypesCore( IRef<INamedType> baseType )
    {
        var set = new HashSet<IRef<INamedType>>( RefEqualityComparer<INamedType>.Default );
        GetDerivedTypesRecursive( baseType );

        return set;

        void GetDerivedTypesRecursive( IRef<INamedType> parentType )
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

        foreach ( var transformation in introducedInterfaces )
        {
            builder ??= new Builder( this );

            var introducedInterface = transformation.InterfaceType;

            if ( !introducedInterface.GetStrategy().GetAssemblySymbol(introducedInterface, this._compilationContext ).Equals( this._compilationContext.Compilation.Assembly ) )
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

            var introducedType = transformation.BuilderData;

            if ( introducedType.BaseType is { } baseType )
            {
                builder.AddDerivedType( baseType, introducedType.ToRef() );
            }

            foreach ( var implementedInterface in introducedType.ImplementedInterfaces )
            {
                builder.AddDerivedType( implementedInterface, introducedType.ToRef() );
            }
        }

        return builder?.ToImmutable() ?? this;
    }

    public void PopulateDependencies( IDependencyCollector collector )
    {
        foreach ( var baseType in this._relationships.Keys )
        {
            if ( baseType is ISymbolRef
                {
                    Symbol: INamedTypeSymbol { OriginalDefinition.DeclaringSyntaxReferences.IsDefaultOrEmpty: false } baseTypeSymbol
                } )
            {
                this.PopulateDependenciesCore( collector, baseTypeSymbol, baseType );
            }
        }
    }

    private void PopulateDependenciesCore( IDependencyCollector collector, INamedTypeSymbol rootType, IRef<INamedType> baseType )
    {
        foreach ( var derivedType in this._relationships[baseType] )
        {
            if ( derivedType is ISymbolRef { Symbol: INamedTypeSymbol derivedTypeSymbol } )
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
            if ( !this._processedTypes.Contains( type.ToRef( this._compilationContext ) ) )
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
            if ( !this._processedTypes.Contains( type.ToRef() ) )
            {
                builder ??= new Builder( this );
                builder.AnalyzeType( type.ToRef() );
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