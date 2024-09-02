// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

/// <summary>
/// Filters symbols that should not be included in the code model.
/// </summary>
internal sealed class SymbolValidator
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly CompilationContext _owner;
#pragma warning restore IDE0052 // Remove unread private members

    private readonly ConcurrentDictionary<ISymbol, bool> _cache;
    private readonly SymbolVisitor _visitor;
    private readonly SymbolVisitor _visitorNoRecursion;
    private readonly Func<ISymbol, bool> _visitDelegate;

    public SymbolValidator( CompilationContext owner )
    {
        this._owner = owner;
        this._cache = new ConcurrentDictionary<ISymbol, bool>( SymbolEqualityComparer.Default );
        this._visitor = new SymbolVisitor( this, true );
        this._visitorNoRecursion = new SymbolVisitor( this, false );
        this._visitDelegate = this.IsValidNoCache;
    }

    public bool IsValid( ISymbol symbol )
    {
        return this._cache.GetOrAdd( symbol, this._visitDelegate );
    }

    private bool IsValidNoCache( ISymbol symbol )
    {
        var result = this._visitor.Visit( symbol );

        return result;
    }

    private bool IsValidNoRecursion( ISymbol symbol )
    {
        return this._visitorNoRecursion.Visit( symbol );
    }

    private class SymbolVisitor : SymbolVisitor<bool>
    {
        private readonly SymbolValidator _validator;
        private readonly bool _recursion;

        public SymbolVisitor( SymbolValidator validator, bool visitInterfaces )
        {
            this._validator = validator;
        }

        public override bool DefaultVisit( ISymbol symbol ) => throw new NotImplementedException();

        public override bool VisitArrayType( IArrayTypeSymbol symbol )
            => symbol.TypeKind != TypeKind.Error
               && this._validator.IsValid( symbol.ElementType );

        public override bool VisitDiscard( IDiscardSymbol symbol ) => true;

        public override bool VisitDynamicType( IDynamicTypeSymbol symbol ) => true;

        public override bool VisitEvent( IEventSymbol symbol ) => this._validator.IsValid( symbol.Type );

        public override bool VisitField( IFieldSymbol symbol ) => this._validator.IsValid( symbol.Type );

        public override bool VisitLabel( ILabelSymbol symbol ) => true;

        public override bool VisitLocal( ILocalSymbol symbol ) => true;

        public override bool VisitAssembly( IAssemblySymbol symbol ) => true;

        public override bool VisitNamespace( INamespaceSymbol symbol ) => true;

        public override bool VisitModule( IModuleSymbol symbol ) => true;

        public override bool VisitAlias( IAliasSymbol symbol ) => true;

        public override bool VisitMethod( IMethodSymbol symbol )
            => this._validator.IsValid( symbol.ReturnType )
               && symbol.Parameters.All( p => this._validator.IsValid( p.Type ) )
               && symbol.TypeParameters.All( this._validator.IsValid );

        public override bool VisitNamedType( INamedTypeSymbol symbol )
            => symbol.Kind != SymbolKind.ErrorType 
               && symbol.TypeKind != TypeKind.Error)
               && (symbol.BaseType == null || this._validator.IsValid( symbol.BaseType ))
               && (!this._recursion || symbol.AllInterfaces.All( this._validator.IsValidNoRecursion ))
               && symbol.TypeArguments.All( this._validator.IsValid );

        public override bool VisitParameter( IParameterSymbol symbol ) => this._validator.IsValid( symbol.Type );

        public override bool VisitPointerType( IPointerTypeSymbol symbol )
            => symbol.TypeKind != TypeKind.Error
               && this._validator.IsValid( symbol.PointedAtType );

        public override bool VisitFunctionPointerType( IFunctionPointerTypeSymbol symbol )
            => symbol.TypeKind != TypeKind.Error
               && this._validator.IsValid( symbol.Signature );

        public override bool VisitProperty( IPropertySymbol symbol )
            => this._validator.IsValid( symbol.Type )
               && symbol.Parameters.All( p => this._validator.IsValid( p.Type ) );

        public override bool VisitRangeVariable( IRangeVariableSymbol symbol ) => true;

        public override bool VisitTypeParameter( ITypeParameterSymbol symbol )
            => symbol.TypeKind != TypeKind.Error
               && (!this._recursion || symbol.ConstraintTypes.All( this._validator.IsValidNoRecursion ));

        public override bool Visit( ISymbol? symbol ) => symbol == null || base.Visit( symbol );
    }
}