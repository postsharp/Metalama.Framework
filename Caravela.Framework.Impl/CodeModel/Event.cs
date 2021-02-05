using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Event : CodeElement, IEvent
    {
        private readonly IEventSymbol _symbol;
        protected internal override ISymbol Symbol => this._symbol;

        private readonly NamedType _containingElement;
        public override ICodeElement ContainingElement => this._containingElement;

        internal override SourceCompilation Compilation => this._containingElement.Compilation;

        public Event( IEventSymbol symbol, NamedType containingElement)
        {
            this._symbol = symbol;
            this._containingElement = containingElement;
        }

        [Memo]
        public INamedType EventType => this.SymbolMap.GetNamedType( (INamedTypeSymbol)this._symbol.Type );

        [Memo]
        public IMethod Adder => this.SymbolMap.GetMethod( this._symbol.AddMethod! );

        [Memo]
        public IMethod Remover => this.SymbolMap.GetMethod( this._symbol.RemoveMethod! );

        // TODO: pseudo-accessor
        [Memo]
        public IMethod? Raiser => this._symbol.RaiseMethod == null ? null : this.SymbolMap.GetMethod( this._symbol.RaiseMethod );

        public string Name => this._symbol.Name;

        public bool IsStatic => this._symbol.IsStatic;

        public bool IsVirtual => this._symbol.IsVirtual;

        public INamedType DeclaringType => this._containingElement;

        [Memo]
        public override IReactiveCollection<IAttribute> Attributes => this._symbol.GetAttributes().Select(a => new Attribute(a, this.SymbolMap )).ToImmutableReactive();

        public override CodeElementKind ElementKind => CodeElementKind.Event;
    }
}
