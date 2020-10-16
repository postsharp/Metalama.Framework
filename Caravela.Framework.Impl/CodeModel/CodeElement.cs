using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Caravela.Framework.Sdk;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    abstract class CodeElement : ICodeElement, IToSyntax
    {
        internal abstract Compilation Compilation { get; }
        internal SymbolMap SymbolMap => Compilation.SymbolMap;

        public abstract ICodeElement? ContainingElement { get; }
        public abstract IReactiveCollection<IAttribute> Attributes { get; }

        protected abstract ISymbol Symbol { get; }
        private IEnumerable<CSharpSyntaxNode> ToSyntaxNodes() => Symbol.DeclaringSyntaxReferences.Select(r => (CSharpSyntaxNode)r.GetSyntax());
        // TODO: special case partial methods
        CSharpSyntaxNode IToSyntax.ToSyntaxNode() => ToSyntaxNodes().Single();
        IEnumerable<CSharpSyntaxNode> IToSyntax.ToSyntaxNodes() => ToSyntaxNodes();
    }
}
