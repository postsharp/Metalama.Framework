using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class Parameter : IParameter
    {
        private readonly IParameterSymbol symbol;
        private readonly CodeElement containingMember;

        private Cache Cache => containingMember.Cache;

        public Parameter(IParameterSymbol symbol, CodeElement containingMember)
        {
            this.symbol = symbol;
            this.containingMember = containingMember;
        }

        [LazyThreadSafeProperty]
        public IType Type => Cache.GetIType(symbol.Type);

        public string Name => symbol.Name;

        public int Index => symbol.Ordinal;

        public ICodeElement ContainingElement => containingMember;

        [LazyThreadSafeProperty]
        public IReadOnlyList<IAttribute> Attributes => symbol.GetAttributes().Select(a => new Attribute(a, Cache)).ToImmutableArray();
    }
}
