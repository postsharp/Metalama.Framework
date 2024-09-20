// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Elfie.Model;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

internal partial class GenericMap
{
    private sealed class TypeSymbolMapper : TypeSymbolRewriter
    {
        private readonly GenericMap _genericMap;

        public TypeSymbolMapper( GenericMap genericMap ) : base( genericMap.CompilationContext.AssertNotNull().Compilation )
        {
            this._genericMap = genericMap;
        }

        internal override ITypeSymbol Visit( ITypeParameterSymbol typeSymbolParameter )
        {
            return this._genericMap.Map( typeSymbolParameter );
        }
    }

    /// <summary>
    /// Determines if a typeSymbol signature contains a generic parameter.
    /// </summary>
    private sealed class TypeSymbolVisitor : SymbolVisitor<bool>
    {
        private TypeSymbolVisitor() { }

        public static TypeSymbolVisitor Instance { get; } = new();

        public override bool Visit( ISymbol? symbol ) => throw new NotImplementedException( $"Symbol kind not handled: {symbol.Kind}." );

        public override bool VisitArrayType( IArrayTypeSymbol arrayTypeSymbol ) => this.Visit( arrayTypeSymbol.ElementType );

        public override bool VisitDynamicType( IDynamicTypeSymbol dynamicTypeSymbol ) => false;

        public override bool VisitNamedType( INamedTypeSymbol namedTypeSymbol ) => namedTypeSymbol.TypeArguments.Any( this.Visit );

        public override bool VisitPointerType( IPointerTypeSymbol pointerTypeSymbol ) => this.Visit( pointerTypeSymbol.PointedAtType );

        public override bool VisitFunctionPointerType( IFunctionPointerTypeSymbol functionPointerTypeSymbol ) => false;

        public override bool VisitTypeParameter( ITypeParameterSymbol typeSymbolParameter ) => true;
    }
}