using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Event : Member, IEvent
    {
        private readonly IEventSymbol _symbol;

        protected internal override ISymbol Symbol => this._symbol;

        public Event( IEventSymbol symbol, NamedType containingElement ) : base( containingElement.Compilation )
        {
            this._symbol = symbol;
        }

        [Memo]
        public INamedType EventType => this.Compilation.GetNamedType( (INamedTypeSymbol) this._symbol.Type );

        [Memo]
        public IMethod Adder => this.Compilation.GetMethod( this._symbol.AddMethod! );

        [Memo]
        public IMethod Remover => this.Compilation.GetMethod( this._symbol.RemoveMethod! );

        // TODO: pseudo-accessor
        [Memo]
        public IMethod? Raiser => this._symbol.RaiseMethod == null ? null : this.Compilation.GetMethod( this._symbol.RaiseMethod );

        public override CodeElementKind ElementKind => CodeElementKind.Event;
    }
}
