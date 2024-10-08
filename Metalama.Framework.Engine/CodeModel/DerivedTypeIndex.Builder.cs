// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.CodeModel;

public partial class DerivedTypeIndex
{
    internal sealed class Builder
    {
        private readonly CompilationContext _compilationContext;
        private readonly ImmutableDictionaryOfArray<IFullRef<INamedType>, IFullRef<INamedType>>.Builder _relationships;
        private readonly ImmutableHashSet<IFullRef<INamedType>>.Builder _processedTypes;

        internal Builder( CompilationContext compilationContext )
        {
            this._compilationContext = compilationContext;

            this._relationships = new ImmutableDictionaryOfArray<IFullRef<INamedType>, IFullRef<INamedType>>.Builder( RefEqualityComparer<INamedType>.Default );
            this._processedTypes = ImmutableHashSet.CreateBuilder<IFullRef<INamedType>>( RefEqualityComparer<INamedType>.Default );
        }

        internal Builder( DerivedTypeIndex immutable )
        {
            this._compilationContext = immutable._compilationContext;
            this._relationships = immutable._relationships.ToBuilder();
            this._processedTypes = immutable._processedTypes.ToBuilder();
        }

        public void AnalyzeType( IRef<INamedType> type )
        {
            switch ( type )
            {
                case ISymbolRef { Symbol: INamedTypeSymbol symbol }:
                    this.AnalyzeType( symbol );

                    break;

                case IBuiltDeclarationRef { BuilderData: NamedTypeBuilderData builder }:
                    this.AnalyzeType( builder );

                    break;

                default:
                    throw new AssertionFailedException( $"Unsupported target: {type}" );
            }
        }

        public void AnalyzeType( INamedTypeSymbol type )
        {
            if ( !this._processedTypes.Add( type.ToRef( this._compilationContext ) ) )
            {
                return;
            }

            if ( type.BaseType != null && type.BaseType.Kind != SymbolKind.ErrorType )
            {
                var baseType = type.BaseType.OriginalDefinition;

                this._relationships.Add(
                    baseType.ToRef( this._compilationContext ),
                    type.ToRef( this._compilationContext ) );

                this.AnalyzeType( baseType );
            }

            foreach ( var interfaceImpl in type.Interfaces )
            {
                if ( interfaceImpl.TypeKind == RoslynTypeKind.Error )
                {
                    continue;
                }

                var interfaceType = interfaceImpl.OriginalDefinition;

                this._relationships.Add(
                    interfaceType.ToRef( this._compilationContext ),
                    type.ToRef( this._compilationContext ) );

                this.AnalyzeType( interfaceType );
            }

            foreach ( var nestedType in type.GetTypeMembers() )
            {
                this.AnalyzeType( nestedType );
            }
        }

        private void AnalyzeType( NamedTypeBuilderData type )
        {
            if ( !this._processedTypes.Add( type.ToRef() ) )
            {
                return;
            }

            if ( type.BaseType is { IsValid: true } )
            {
                var baseType = type.BaseType.Definition;
                this._relationships.Add( baseType, type.ToRef() );
                this.AnalyzeType( baseType );
            }

            foreach ( var interfaceImpl in type.ImplementedInterfaces )
            {
                if ( !interfaceImpl.IsValid )
                {
                    continue;
                }

                var interfaceType = interfaceImpl.Definition;
                this._relationships.Add( interfaceType, type.ToRef() );
                this.AnalyzeType( interfaceType );
            }
        }

        public void AddDerivedType( INamedTypeSymbol baseType, INamedTypeSymbol derivedType )
            => this._relationships.Add(
                baseType.ToRef( this._compilationContext ),
                derivedType.ToRef( this._compilationContext ) );

        public void AddDerivedType( IFullRef<INamedType> baseType, IFullRef<INamedType> derivedType ) => this._relationships.Add( baseType, derivedType );

        public void AddDerivedType( INamedType baseType, INamedType derivedType ) => this._relationships.Add( baseType.ToFullRef(), derivedType.ToFullRef() );

        public DerivedTypeIndex ToImmutable()
        {
            return new DerivedTypeIndex(
                this._compilationContext,
                this._relationships.ToImmutable(),
                this._processedTypes.ToImmutable() );
        }
    }
}