﻿using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    abstract class CodeElement : ICodeElement, IToSyntax
    {
        internal abstract SourceCompilation Compilation { get; }
        internal SymbolMap SymbolMap => this.Compilation.SymbolMap;

        public abstract ICodeElement? ContainingElement { get; }

        public abstract IReactiveCollection<IAttribute> Attributes { get; }

        public abstract CodeElementKind Kind { get; }

        protected internal abstract ISymbol Symbol { get; }

        private IEnumerable<CSharpSyntaxNode> ToSyntaxNodes() => this.Symbol.DeclaringSyntaxReferences.Select(r => (CSharpSyntaxNode)r.GetSyntax());
        // TODO: special case partial methods?
        CSharpSyntaxNode IToSyntax.GetSyntaxNode() => this.ToSyntaxNodes().Single();
        IEnumerable<CSharpSyntaxNode> IToSyntax.GetSyntaxNodes() => this.ToSyntaxNodes();
    }
}
