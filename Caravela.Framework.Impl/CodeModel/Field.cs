using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    [Obfuscation( Exclude = true )]
    internal class Field : CodeElement, IProperty
    {
        private readonly IFieldSymbol _symbol;
        protected internal override ISymbol Symbol => this._symbol;

        private readonly NamedType _containingElement;
        public override ICodeElement? ContainingElement => this._containingElement;

        internal override SourceCompilation Compilation => this._containingElement.Compilation;

        public Field( IFieldSymbol symbol, NamedType containingElement )
        {
            this._symbol = symbol;
            this._containingElement = containingElement;
        }

        [Memo]
        public IType Type => this.SymbolMap.GetIType( this._symbol.Type);

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
        public override IReactiveCollection<IAttribute> Attributes => this._symbol.GetAttributes().Select(a => new Attribute(a, this.SymbolMap )).ToImmutableReactive();

        public override CodeElementKind Kind => CodeElementKind.Field;
    }
}
