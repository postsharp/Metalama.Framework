using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class Property : CodeElement, IProperty
    {
        private readonly IPropertySymbol symbol;
        protected override ISymbol Symbol => symbol;

        private readonly TypeInfo containingElement;
        public override ICodeElement? ContainingElement => containingElement;

        internal override Compilation Compilation => containingElement.Compilation;

        public Property(IPropertySymbol symbol, TypeInfo containingElement)
        {
            this.symbol = symbol;
            this.containingElement = containingElement;
        }

        [Memo]
        public IType Type => SymbolMap.GetIType(symbol.Type);

        [Memo]
        public IReadOnlyList<IParameter> Parameters => symbol.Parameters.Select(p => new Parameter(p, this)).ToImmutableArray();


        [Memo]
        public IMethod? Getter => symbol.GetMethod == null ? null : SymbolMap.GetMethod(symbol.GetMethod);

        [Memo]
        // TODO: get-only properties
        public IMethod? Setter => symbol.SetMethod == null ? null : SymbolMap.GetMethod(symbol.SetMethod);

        public string Name => symbol.Name;

        public bool IsStatic => symbol.IsStatic;

        [Memo]
        public override IReactiveCollection<IAttribute> Attributes => symbol.GetAttributes().Select(a => new Attribute(a, SymbolMap)).ToImmutableReactive();
    }
}
