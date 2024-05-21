// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

        public void AnalyzeType( Ref<INamedType> type )
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
            if ( !this._processedTypes.Add( type.ToTypedRef<INamedType>( this._compilationContext ) ) )
            {
                return;
            }

            if ( type.BaseType != null && type.BaseType.Kind != SymbolKind.ErrorType )
            {
                var baseType = type.BaseType.OriginalDefinition;
                this._relationships.Add( baseType.ToTypedRef<INamedType>( this._compilationContext ), type.ToTypedRef<INamedType>( this._compilationContext ) );
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
                    interfaceType.ToTypedRef<INamedType>( this._compilationContext ),
                    type.ToTypedRef<INamedType>( this._compilationContext ) );

                this.AnalyzeType( interfaceType );
            }

            foreach ( var nestedType in type.GetTypeMembers() )
            {
                this.AnalyzeType( nestedType );
            }
        }

        public void AnalyzeType( INamedTypeBuilder type )
        {
            if ( type.GetSymbol() is { } symbol )
            {
                this.AnalyzeType( symbol );

                return;
            }

            if ( !this._processedTypes.Add( type.ToTypedRef<INamedType>() ) )
            {
                return;
            }

            if ( type.BaseType != null && type.BaseType.TypeKind != MetalamaTypeKind.Error )
            {
                var baseType = type.BaseType.Definition;
                this._relationships.Add( baseType.ToTypedRef(), type.ToTypedRef<INamedType>() );
                this.AnalyzeType( baseType.ToTypedRef() );
            }

            foreach ( var interfaceImpl in type.ImplementedInterfaces )
            {
                if ( interfaceImpl.TypeKind == MetalamaTypeKind.Error )
                {
                    continue;
                }

                var interfaceType = interfaceImpl.Definition;
                this._relationships.Add( interfaceType.ToTypedRef(), type.ToTypedRef<INamedType>() );
                this.AnalyzeType( interfaceType.ToTypedRef() );
            }

            foreach ( var nestedType in type.Types )
            {
                this.AnalyzeType( nestedType.ToTypedRef() );
            }
        }

        public void AddDerivedType( INamedTypeSymbol baseType, INamedTypeSymbol derivedType )
            => this._relationships.Add(
                baseType.ToTypedRef<INamedType>( this._compilationContext ),
                derivedType.ToTypedRef<INamedType>( this._compilationContext ) );

        public void AddDerivedType( INamedType baseType, INamedType derivedType ) => this._relationships.Add( baseType.ToTypedRef(), derivedType.ToTypedRef() );

        public DerivedTypeIndex ToImmutable()
        {
            return new DerivedTypeIndex(
                this._compilationContext,
                this._relationships.ToImmutable(),
                this._processedTypes.ToImmutable() );
        }
    }
}