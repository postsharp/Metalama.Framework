using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class SourceEvent : Event, ISourceCodeElement
    {
        private readonly IEventSymbol _symbol;

        private SourceCompilationModel Compilation { get; }

        public ISymbol Symbol => this._symbol;

        public override string Name => this._symbol.Name;

        public override bool IsStatic => this._symbol.IsStatic;

        public override bool IsVirtual => this._symbol.IsVirtual;

        [Memo]
        public override NamedType EventType => this.Compilation.SymbolMap.GetNamedType( (INamedTypeSymbol) this._symbol.Type );

        [Memo]
        public override Method Adder => this.Compilation.SymbolMap.GetMethod( this._symbol.AddMethod! );

        [Memo]
        public override Method Remover => this.Compilation.SymbolMap.GetMethod( this._symbol.RemoveMethod! );

        // TODO: pseudo-accessor
        [Memo]
        public override Method? Raiser => this._symbol.RaiseMethod == null ? null : this.Compilation.SymbolMap.GetMethod( this._symbol.RaiseMethod );

        [Memo]
        public override IReadOnlyList<Attribute> Attributes => this._symbol.GetAttributes().Select( a => new SourceAttribute( this.Compilation, a ) ).ToImmutableList();

        public SourceEvent( IEventSymbol symbol, SourceNamedType declaringType ) : base(containingElement)
        {
            this._symbol = symbol;
            this.Compilation = containingElement.Compilation;
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals( ICodeElement other )
        {
            throw new System.NotImplementedException();
        }
    }
}
