using System.Linq;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class Parameter : IParameter
    {
        private readonly IParameterSymbol _symbol;
        private readonly CodeElement _containingMember;

        private SymbolMap SymbolMap => this._containingMember.SymbolMap;

        public Parameter(IParameterSymbol symbol, CodeElement containingMember)
        {
            this._symbol = symbol;
            this._containingMember = containingMember;
        }

        [Memo]
        public IType Type => this.SymbolMap.GetIType( this._symbol.Type);

        public string Name => this._symbol.Name;

        public int Index => this._symbol.Ordinal;

        public ICodeElement ContainingElement => this._containingMember;

        [Memo]
        public IReactiveCollection<IAttribute> Attributes => this._symbol.GetAttributes().Select(a => new Attribute(a, this.SymbolMap )).ToImmutableReactive();
    }
}
