using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Property : CodeElement, IProperty
    {
        private readonly IPropertySymbol _symbol;
        protected internal override ISymbol Symbol => this._symbol;
        

        private readonly NamedType _containingElement;
        public override ICodeElement? ContainingElement => this._containingElement;

        internal override SourceCompilation Compilation => this._containingElement.Compilation;

        public Property(IPropertySymbol symbol, NamedType containingElement)
        {
            this._symbol = symbol;
            this._containingElement = containingElement;
        }

        [Memo]
        public IType Type => this.SymbolMap.GetIType( this._symbol.Type);

        [Memo]
        public IImmutableList<IParameter> Parameters => this._symbol.Parameters.Select(p => new Parameter(p, this)).ToImmutableList<IParameter>();


        [Memo]
        public IMethod? Getter => this._symbol.GetMethod == null ? null : this.SymbolMap.GetMethod( this._symbol.GetMethod);

        [Memo]
        // TODO: get-only properties
        public IMethod? Setter => this._symbol.SetMethod == null ? null : this.SymbolMap.GetMethod( this._symbol.SetMethod);

        public string Name => this._symbol.Name;

        public bool IsStatic => this._symbol.IsStatic;

        public bool IsVirtual => this._symbol.IsVirtual;

        public INamedType DeclaringType => this._containingElement;

        [Memo]
        public override IReactiveCollection<IAttribute> Attributes => this._symbol.GetAttributes().Select(a => new Attribute(a, this.SymbolMap )).ToImmutableReactive();

        public override CodeElementKind Kind => CodeElementKind.Property;
    }
}
