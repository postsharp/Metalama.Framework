﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal abstract class CodeElement : ICodeElement, IHasLocation
    {
        protected CodeElement( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        internal CompilationModel Compilation { get; }

        ICompilation ICodeElement.Compilation => this.Compilation;

        [Memo]
        public virtual ICodeElement? ContainingElement => this.Compilation.Factory.GetCodeElement( this.Symbol.ContainingSymbol );

        [Memo]
        public IReadOnlyList<IAttribute> Attributes =>
            this.Symbol.GetAttributes()
                .Select( a => new Attribute( a, this.Compilation, this ) )
                .ToImmutableArray();

        public abstract CodeElementKind ElementKind { get; }

        protected internal abstract ISymbol Symbol { get; }

        private IEnumerable<CSharpSyntaxNode> ToSyntaxNodes() => this.Symbol.DeclaringSyntaxReferences.Select( r => (CSharpSyntaxNode) r.GetSyntax() );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this.Symbol.ToDisplayString();

        public bool Equals( ICodeElement other ) =>
            other is CodeElement codeElement &&
            SymbolEqualityComparer.Default.Equals( this.Symbol, codeElement.Symbol );

        public Location? Location => this.Symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()?.GetLocation();

        public IDiagnosticLocation? DiagnosticLocation => RoslynDiagnosticLocation.ForSymbol( this.Symbol );
    }
}
