﻿using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;

using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Field : Member, IProperty
    {
        private readonly IFieldSymbol _symbol;

        protected internal override ISymbol Symbol => this._symbol;

        private readonly NamedType _containingElement;

        public override CodeElement? ContainingElement => this._containingElement;

        internal override SourceCompilationModel Compilation => this._containingElement.Compilation;

        public Field( IFieldSymbol symbol, NamedType containingElement )
        {
            this._symbol = symbol;
            this._containingElement = containingElement;
        }

        public RefKind RefKind => RefKind.None;

        public bool IsByRef => false;

        public bool IsRef => false;

        public bool IsRefReadonly => false;

        [Memo]
        public IType Type => this.SymbolMap.GetIType( this._symbol.Type );

        public IImmutableList<IParameter> Parameters => ImmutableList<IParameter>.Empty;

        // TODO: pseudo-accessors
        [Memo]
        public IMethod? Getter => null;

        [Memo]
        public IMethod? Setter => null;

        public string Name => this._symbol.Name;

        public bool IsStatic => this._symbol.IsStatic;

        public bool IsVirtual => false;

        public INamedType DeclaringType => this._containingElement;

        [Memo]
        public override IImmutableList<Attribute> Attributes => this._symbol.GetAttributes().Select( a => new Attribute( a, this.SymbolMap ) ).ToImmutableReactive();

        public override CodeElementKind ElementKind => CodeElementKind.Field;
    }
}
