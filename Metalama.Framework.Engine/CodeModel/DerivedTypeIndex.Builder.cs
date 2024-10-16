﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using MetalamaTypeKind = Metalama.Framework.Code.TypeKind;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.CodeModel;

public partial class DerivedTypeIndex
{
    internal sealed class Builder
    {
        private readonly CompilationContext _compilationContext;
        private readonly ImmutableDictionaryOfArray<Ref<INamedType>, Ref<INamedType>>.Builder _relationships;
        private readonly ImmutableHashSet<Ref<INamedType>>.Builder _processedTypes;

        internal Builder( CompilationContext compilationContext )
        {
            this._compilationContext = compilationContext;

            this._relationships = new ImmutableDictionaryOfArray<Ref<INamedType>, Ref<INamedType>>.Builder( RefEqualityComparer<INamedType>.Default );
            this._processedTypes = ImmutableHashSet.CreateBuilder( RefEqualityComparer<INamedType>.Default );
        }

        internal Builder( DerivedTypeIndex immutable )
        {
            this._compilationContext = immutable._compilationContext;
            this._relationships = immutable._relationships.ToBuilder();
            this._processedTypes = immutable._processedTypes.ToBuilder();
        }

        public void AnalyzeType( in Ref<INamedType> type )
        {
            switch ( type.Target )
            {
                case INamedTypeSymbol symbol:
                    this.AnalyzeType( symbol );

                    break;

                case INamedTypeBuilder builder:
                    this.AnalyzeType( builder );

                    break;

                default:
                    throw new AssertionFailedException( $"Unsupported target: {type.Target}" );
            }
        }

        public void AnalyzeType( INamedTypeSymbol type )
        {
            if ( !this._processedTypes.Add( type.ToValueTypedRef<INamedType>( this._compilationContext ) ) )
            {
                return;
            }

            if ( type.BaseType != null && type.BaseType.Kind != SymbolKind.ErrorType )
            {
                var baseType = type.BaseType.OriginalDefinition;

                this._relationships.Add(
                    baseType.ToValueTypedRef<INamedType>( this._compilationContext ),
                    type.ToValueTypedRef<INamedType>( this._compilationContext ) );

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
                    interfaceType.ToValueTypedRef<INamedType>( this._compilationContext ),
                    type.ToValueTypedRef<INamedType>( this._compilationContext ) );

                this.AnalyzeType( interfaceType );
            }

            foreach ( var nestedType in type.GetTypeMembers() )
            {
                this.AnalyzeType( nestedType );
            }
        }

        private void AnalyzeType( INamedTypeBuilder type )
        {
            if ( type.GetSymbol() is { } symbol )
            {
                this.AnalyzeType( symbol );

                return;
            }

            if ( !this._processedTypes.Add( type.ToValueTypedRef<INamedType>() ) )
            {
                return;
            }

            if ( type.BaseType != null && type.BaseType.TypeKind != MetalamaTypeKind.Error )
            {
                var baseType = type.BaseType.Definition;
                this._relationships.Add( baseType.ToValueTypedRef(), type.ToValueTypedRef<INamedType>() );
                this.AnalyzeType( baseType.ToValueTypedRef() );
            }

            foreach ( var interfaceImpl in type.ImplementedInterfaces )
            {
                if ( interfaceImpl.TypeKind == MetalamaTypeKind.Error )
                {
                    continue;
                }

                var interfaceType = interfaceImpl.Definition;
                this._relationships.Add( interfaceType.ToValueTypedRef(), type.ToValueTypedRef<INamedType>() );
                this.AnalyzeType( interfaceType.ToValueTypedRef() );
            }

            foreach ( var nestedType in type.Types )
            {
                this.AnalyzeType( nestedType.ToValueTypedRef() );
            }
        }

        public void AddDerivedType( INamedTypeSymbol baseType, INamedTypeSymbol derivedType )
            => this._relationships.Add(
                baseType.ToValueTypedRef<INamedType>( this._compilationContext ),
                derivedType.ToValueTypedRef<INamedType>( this._compilationContext ) );

        public void AddDerivedType( INamedType baseType, INamedType derivedType )
            => this._relationships.Add( baseType.ToValueTypedRef(), derivedType.ToValueTypedRef() );

        public DerivedTypeIndex ToImmutable()
        {
            return new DerivedTypeIndex(
                this._compilationContext,
                this._relationships.ToImmutable(),
                this._processedTypes.ToImmutable() );
        }
    }
}