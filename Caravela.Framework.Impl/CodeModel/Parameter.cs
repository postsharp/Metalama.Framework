﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class Parameter : IParameter
    {
        private readonly IParameterSymbol symbol;
        private readonly CodeElement containingMember;

        private SymbolMap SymbolMap => containingMember.SymbolMap;

        public Parameter(IParameterSymbol symbol, CodeElement containingMember)
        {
            this.symbol = symbol;
            this.containingMember = containingMember;
        }

        [Memo]
        public IType Type => SymbolMap.GetIType(symbol.Type);

        public string Name => symbol.Name;

        public int Index => symbol.Ordinal;

        public ICodeElement ContainingElement => containingMember;

        [Memo]
        public IReadOnlyList<IAttribute> Attributes => symbol.GetAttributes().Select(a => new Attribute(a, SymbolMap)).ToImmutableArray();
    }
}
