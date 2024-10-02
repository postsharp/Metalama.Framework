// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.Visitors;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

internal partial class GenericContext
{
    private sealed class TypeSymbolMapper : TypeSymbolRewriter
    {
        public GenericContext GenericContext { get; }

        public TypeSymbolMapper( GenericContext genericContext ) : base( genericContext.CompilationContext.AssertNotNull().Compilation )
        {
            this.GenericContext = genericContext;
        }

        internal override ITypeSymbol Visit( ITypeParameterSymbol typeSymbolParameter )
        {
            return this.GenericContext.Map( typeSymbolParameter );
        }
    }

    /// <summary>
    /// Determines if a typeSymbol signature contains a generic parameter.
    /// </summary>
    private sealed class TypeSymbolVisitor : SymbolVisitor<bool>
    {
        private TypeSymbolVisitor() { }

        public static TypeSymbolVisitor Instance { get; } = new();

        public override bool Visit( ISymbol? symbol ) => throw new NotImplementedException( $"Symbol kind not handled: {symbol?.Kind}." );

        public override bool VisitArrayType( IArrayTypeSymbol arrayTypeSymbol ) => this.Visit( arrayTypeSymbol.ElementType );

        public override bool VisitDynamicType( IDynamicTypeSymbol dynamicTypeSymbol ) => false;

        public override bool VisitNamedType( INamedTypeSymbol namedTypeSymbol ) => namedTypeSymbol.TypeArguments.Any( this.Visit );

        public override bool VisitPointerType( IPointerTypeSymbol pointerTypeSymbol ) => this.Visit( pointerTypeSymbol.PointedAtType );

        public override bool VisitFunctionPointerType( IFunctionPointerTypeSymbol functionPointerTypeSymbol ) => false;

        public override bool VisitTypeParameter( ITypeParameterSymbol typeSymbolParameter ) => true;
    }
}