﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

internal class SymbolValidator : SymbolVisitor<bool>
{
    public static SymbolValidator Instance { get; } = new();

    private SymbolValidator() { }

    public bool VisitAttribute( AttributeData attribute )
        => attribute is { AttributeClass: { }, AttributeConstructor: { } } 
           && this.Visit( attribute.AttributeClass ) 
           && attribute.ConstructorArguments.All( a => this.VisitTypedConstant( a ) ) 
           && attribute.NamedArguments.All( a => this.VisitTypedConstant( a.Value ) );

    private bool VisitTypedConstant( in TypedConstant constant )
        => constant.Kind switch
        {
            TypedConstantKind.Array => constant.Values.IsDefaultOrEmpty || constant.Values.All( v => this.VisitTypedConstantValue( v ) ),
            TypedConstantKind.Error => false,
            TypedConstantKind.Type => constant.Value == null || this.VisitTypedConstantValue( constant.Value ),
            _ => true
        };

    private bool VisitTypedConstantValue( object value ) => !(value is ITypeSymbol type && !this.Visit( type ));

    public override bool DefaultVisit( ISymbol symbol ) => throw new NotImplementedException();

    public override bool VisitArrayType( IArrayTypeSymbol symbol ) => this.Visit( symbol.ElementType );

    public override bool VisitDiscard( IDiscardSymbol symbol ) => true;

    public override bool VisitDynamicType( IDynamicTypeSymbol symbol ) => true;

    public override bool VisitEvent( IEventSymbol symbol ) => this.Visit( symbol.Type );

    public override bool VisitField( IFieldSymbol symbol ) => this.Visit( symbol.Type );

    public override bool VisitLabel( ILabelSymbol symbol ) => true;

    public override bool VisitLocal( ILocalSymbol symbol ) => true;

    public override bool VisitMethod( IMethodSymbol symbol )
        => this.Visit( symbol.ReturnType )
           && symbol.Parameters.All( p => this.Visit( p.Type ) )
           && symbol.TypeParameters.All( this.Visit );

    public override bool VisitNamedType( INamedTypeSymbol symbol )
        => symbol.Kind != SymbolKind.ErrorType && this.Visit( symbol.BaseType )
                                               && symbol.Interfaces.All( this.Visit );

    public override bool VisitParameter( IParameterSymbol symbol ) => this.Visit( symbol.Type );

    public override bool VisitPointerType( IPointerTypeSymbol symbol ) => this.Visit( symbol.PointedAtType );

    public override bool VisitFunctionPointerType( IFunctionPointerTypeSymbol symbol ) => this.Visit( symbol.Signature );

    public override bool VisitProperty( IPropertySymbol symbol )
        => this.Visit( symbol.Type )
           && symbol.Parameters.All( p => this.Visit( p.Type ) );

    public override bool VisitRangeVariable( IRangeVariableSymbol symbol ) => true;

    public override bool VisitTypeParameter( ITypeParameterSymbol symbol ) => symbol.ConstraintTypes.All( this.Visit );

    public override bool Visit( ISymbol? symbol ) => symbol == null || base.Visit( symbol );
}