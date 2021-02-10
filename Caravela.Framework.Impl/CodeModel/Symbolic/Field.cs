using System.Collections.Immutable;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Field : Member, IProperty
    {
        private readonly IFieldSymbol _symbol;

        protected internal override ISymbol Symbol => this._symbol;

        private readonly NamedType _containingElement;


        
        public Field( IFieldSymbol symbol, NamedType containingElement ) : base( containingElement.Compilation )
        {
            this._symbol = symbol;
            this._containingElement = containingElement;
        }

        public RefKind RefKind => RefKind.None;

        public bool IsByRef => false;

        public bool IsRef => false;

        public bool IsRefReadonly => false;

        [Memo]
        public IType Type => this.Compilation.GetIType( this._symbol.Type );

        IReadOnlyList<IParameter> IProperty.Parameters => ImmutableList<IParameter>.Empty;

        // TODO: pseudo-accessors
        [Memo]
        public IMethod? Getter => null;

        [Memo]
        public IMethod? Setter => null;

        public INamedType DeclaringType => this._containingElement;

        public override CodeElementKind ElementKind => CodeElementKind.Field;
    }
}
