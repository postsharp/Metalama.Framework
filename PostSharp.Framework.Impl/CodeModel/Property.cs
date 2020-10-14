using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PostSharp.Framework.Impl
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

        [LazyThreadSafeProperty]
        public IType Type => Cache.GetIType(symbol.Type);

        [LazyThreadSafeProperty]
        public IReadOnlyList<IParameter> Parameters => symbol.Parameters.Select(p => new Parameter(p, this)).ToImmutableArray();


        [LazyThreadSafeProperty]
        public IMethod? Getter => symbol.GetMethod == null ? null : Cache.GetMethod(symbol.GetMethod);

        [LazyThreadSafeProperty]
        // TODO: get-only properties
        public IMethod? Setter => symbol.SetMethod == null ? null : Cache.GetMethod(symbol.SetMethod);

        public string Name => symbol.Name;

        public bool IsStatic => symbol.IsStatic;

        [LazyThreadSafeProperty]
        public override IReadOnlyList<IAttribute> Attributes => symbol.GetAttributes().Select(a => new Attribute(a, Cache)).ToImmutableArray();
    }
}
