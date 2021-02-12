using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Property : Member, IProperty
    {
        private readonly IPropertySymbol _symbol;

        protected internal override ISymbol Symbol => this._symbol;

        public Property( IPropertySymbol symbol, NamedType declaringType ) : base( declaringType.Compilation )
        {
            this._symbol = symbol;
        }

        public RefKind RefKind => ReturnParameter.MapRefKind( this._symbol.RefKind );

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsRefReadonly => this.RefKind == RefKind.RefReadonly;

        [Memo]
        public IType Type => this.Compilation.GetIType( this._symbol.Type );

        [Memo]
        public IReadOnlyList<IParameter> Parameters => this._symbol.Parameters.Select( p => new Parameter( p, this ) ).ToImmutableArray<IParameter>();

        [Memo]
        public IMethod? Getter => this._symbol.GetMethod == null ? null : this.Compilation.GetMethod( this._symbol.GetMethod );

        [Memo]

        // TODO: get-only properties
        public IMethod? Setter => this._symbol.SetMethod == null ? null : this.Compilation.GetMethod( this._symbol.SetMethod );

        public override CodeElementKind ElementKind => CodeElementKind.Property;
    }
}
