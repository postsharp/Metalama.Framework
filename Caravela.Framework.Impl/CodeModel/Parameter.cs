﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Impl.Reactive;
using Caravela.Reactive;
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
        public IReactiveCollection<IAttribute> Attributes => symbol.GetAttributes().Select(a => new Attribute(a, SymbolMap)).ToImmutableReactive();
    }
}
