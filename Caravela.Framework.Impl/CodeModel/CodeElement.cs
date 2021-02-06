using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class CodeElement : ICodeElement, IToSyntax
    {
        internal abstract SourceCompilationModel Compilation { get; }

        internal SymbolMap SymbolMap => this.Compilation.SymbolMap;

        public abstract ICodeElement? ContainingElement { get; }

        public abstract IReactiveCollection<IAttribute> Attributes { get; }

        public abstract CodeElementKind ElementKind { get; }

        protected internal abstract ISymbol Symbol { get; }

        private IEnumerable<CSharpSyntaxNode> ToSyntaxNodes() => this.Symbol.DeclaringSyntaxReferences.Select( r => (CSharpSyntaxNode) r.GetSyntax() );

        // TODO: special case partial methods?
        CSharpSyntaxNode IToSyntax.GetSyntaxNode() => this.ToSyntaxNodes().Single();

        IEnumerable<CSharpSyntaxNode> IToSyntax.GetSyntaxNodes() => this.ToSyntaxNodes();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this.Symbol.ToDisplayString();
    }
}
