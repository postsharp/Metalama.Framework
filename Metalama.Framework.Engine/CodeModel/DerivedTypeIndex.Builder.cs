// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Comparers;
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
        private readonly ImmutableDictionaryOfArray<NamedType, NamedType>.Builder _relationships;
        private readonly ImmutableHashSet<NamedType>.Builder _processedTypes;

        internal Builder( CompilationContext compilationContext )
        {
            this._compilationContext = compilationContext;

            var comparer = new NamedType.Comparer( compilationContext.SymbolComparer, StructuralDeclarationComparer.Default );

            this._relationships = new ImmutableDictionaryOfArray<NamedType, NamedType>.Builder( comparer );
            this._processedTypes = ImmutableHashSet.CreateBuilder( comparer );
        }

        internal Builder( DerivedTypeIndex immutable )
        {
            this._compilationContext = immutable._compilationContext;
            this._relationships = immutable._relationships.ToBuilder();
            this._processedTypes = immutable._processedTypes.ToBuilder();
        }

        public void AnalyzeType( INamedTypeSymbol type )
        {
            if ( !this._processedTypes.Add( new( type ) ) )
            {
                return;
            }

            if ( type.BaseType != null && type.BaseType.Kind != SymbolKind.ErrorType )
            {
                var baseType = type.BaseType.OriginalDefinition;
                this._relationships.Add( new( baseType ), new NamedType( type ) );
                this.AnalyzeType( baseType );
            }

            foreach ( var interfaceImpl in type.Interfaces )
            {
                if ( interfaceImpl.TypeKind == RoslynTypeKind.Error )
                {
                    continue;
                }

                var interfaceType = interfaceImpl.OriginalDefinition;
                this._relationships.Add( new( interfaceType ), new NamedType( type ) );
                this.AnalyzeType( interfaceType );
            }

            foreach ( var nestedType in type.GetTypeMembers() )
            {
                this.AnalyzeType( nestedType );
            }
        }

        public void AnalyzeType( INamedType type )
        {
            if ( type.GetSymbol() is { } symbol )
            {
                this.AnalyzeType( symbol );
                
                return;
            }

            if ( !this._processedTypes.Add( new( type ) ) )
            {
                return;
            }

            if ( type.BaseType != null && type.BaseType.TypeKind != MetalamaTypeKind.Error )
            {
                var baseType = type.BaseType.Definition;
                this._relationships.Add( new( baseType ), new NamedType( type ) );
                this.AnalyzeType( baseType );
            }

            foreach ( var interfaceImpl in type.ImplementedInterfaces )
            {
                if ( interfaceImpl.TypeKind == MetalamaTypeKind.Error )
                {
                    continue;
                }

                var interfaceType = interfaceImpl.Definition;
                this._relationships.Add( new( interfaceType ), new NamedType( type ) );
                this.AnalyzeType( interfaceType );
            }

            foreach ( var nestedType in type.NestedTypes )
            {
                this.AnalyzeType( nestedType );
            }
        }

        public void AddDerivedType( INamedTypeSymbol baseType, INamedTypeSymbol derivedType ) => this._relationships.Add( new( baseType ), new NamedType( derivedType ) );

        public void AddDerivedType( INamedType baseType, INamedType derivedType ) => this._relationships.Add( new( baseType ), new NamedType( derivedType ) );

        public DerivedTypeIndex ToImmutable()
        {
            return new DerivedTypeIndex(
                this._compilationContext,
                this._relationships.ToImmutable(),
                this._processedTypes.ToImmutable() );
        }
    }
}