﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

internal partial class GenericContext
{
    private class SymbolMapper : SymbolVisitor<ISymbol>
    {
        private readonly TypeSymbolMapper _typeSymbolMapper;

        public SymbolMapper( GenericContext parent )
        {
            this._typeSymbolMapper = parent.TypeSymbolMapperInstance;
        }

        private T Map<T>( T symbol )
            where T : ISymbol
            => (T) this.Visit( symbol ).AssertSymbolNotNull();

        private T MapMember<T>( T symbol ) where T : ISymbol
        {
            var containingType = this.Map( symbol.ContainingType );
            var members = containingType.GetMembers( symbol.Name );

            return (T) members.Single( s => symbol.Equals( s.OriginalDefinition ) );
        }

        public override ISymbol? DefaultVisit( ISymbol symbol ) => throw new NotSupportedException();

        public override ISymbol? VisitArrayType( IArrayTypeSymbol symbol ) => this._typeSymbolMapper.Visit( symbol );

        public override ISymbol? VisitAssembly( IAssemblySymbol symbol ) => symbol;

        public override ISymbol? VisitDynamicType( IDynamicTypeSymbol symbol ) => this._typeSymbolMapper.Visit( symbol );

        public override ISymbol? VisitEvent( IEventSymbol symbol ) => this.MapMember( symbol );

        public override ISymbol? VisitField( IFieldSymbol symbol ) => this.MapMember( symbol );

        public override ISymbol? VisitMethod( IMethodSymbol symbol ) => this.MapMember( symbol );

        public override ISymbol? VisitModule( IModuleSymbol symbol ) => symbol;

        public override ISymbol? VisitNamedType( INamedTypeSymbol symbol ) => this._typeSymbolMapper.Visit( symbol );

        public override ISymbol? VisitNamespace( INamespaceSymbol symbol ) => symbol;

        public override ISymbol? VisitParameter( IParameterSymbol symbol )
        {
            var parent = this.Visit( symbol.ContainingSymbol );

            return parent switch
            {
                IMethodSymbol method => method.Parameters[symbol.Ordinal],
                IPropertySymbol property => property.Parameters[symbol.Ordinal],
                _ => throw new AssertionFailedException()
            };
        }

        public override ISymbol? VisitPointerType( IPointerTypeSymbol symbol ) => this._typeSymbolMapper.Visit( symbol );

        public override ISymbol? VisitFunctionPointerType( IFunctionPointerTypeSymbol symbol ) => this._typeSymbolMapper.Visit( symbol );

        public override ISymbol? VisitProperty( IPropertySymbol symbol ) => this.MapMember( symbol );

        public override ISymbol? VisitTypeParameter( ITypeParameterSymbol symbol ) => this._typeSymbolMapper.Visit( symbol );
    }
}