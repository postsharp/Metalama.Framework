using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal abstract class CodeElement : ISdkCodeElement, IHasDiagnosticLocation
    {
        protected CodeElement( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        internal CompilationModel Compilation { get; }

        ICompilation ICodeElement.Compilation => this.Compilation;

        CodeOrigin ICodeElement.Origin => CodeOrigin.Source;

        [Memo]
        public virtual ICodeElement? ContainingElement => this.Compilation.Factory.GetCodeElement( this.Symbol.ContainingSymbol );

        [Memo]
        public IReadOnlyList<IAttribute> Attributes =>
            this.Symbol.GetAttributes()
                .Select( a => new Attribute( a, this.Compilation, this ) )
                .ToImmutableArray();

        public abstract CodeElementKind ElementKind { get; }

        bool ISdkCodeElement.IsIntroduced => false;

        public abstract ISymbol Symbol { get; }

        private IEnumerable<CSharpSyntaxNode> ToSyntaxNodes() => this.Symbol.DeclaringSyntaxReferences.Select( r => (CSharpSyntaxNode) r.GetSyntax() );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this.Symbol.ToDisplayString();

        public bool Equals( ICodeElement other ) =>
            other is CodeElement codeElement &&
            SymbolEqualityComparer.Default.Equals( this.Symbol, codeElement.Symbol );

        public Location? DiagnosticLocation => DiagnosticLocationHelper.GetDiagnosticLocation( this.Symbol );

        IDiagnosticLocation? IDiagnosticTarget.DiagnosticLocation => this.DiagnosticLocation?.ToDiagnosticLocation();
    }
}
