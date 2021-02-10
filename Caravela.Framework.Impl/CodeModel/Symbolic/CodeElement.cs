using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class CodeElement : ICodeElement, IToSyntax
    {
        protected CodeElement( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        internal CompilationModel Compilation { get; }

          
        [Memo]
        public virtual ICodeElement ContainingElement => this.Symbol.ContainingSymbol switch
        {
            INamedTypeSymbol type => this.Compilation.GetNamedType( type ),
            IMethodSymbol method => this.Compilation.GetMethod( method ),
            _ => throw new NotImplementedException()
        };

        [Memo]
        public IReadOnlyList<IAttribute> Attributes =>
            this.Symbol.GetAttributes()
                .Select( a => new Attribute( a, this.Compilation, this ) )
                .ToImmutableArray();


        public abstract CodeElementKind ElementKind { get; }

        protected internal abstract ISymbol Symbol { get; }

        private IEnumerable<CSharpSyntaxNode> ToSyntaxNodes() => this.Symbol.DeclaringSyntaxReferences.Select( r => (CSharpSyntaxNode) r.GetSyntax() );

        // TODO: special case partial methods?
        CSharpSyntaxNode IToSyntax.GetSyntaxNode() => this.ToSyntaxNodes().Single();

        IEnumerable<CSharpSyntaxNode> IToSyntax.GetSyntaxNodes() => this.ToSyntaxNodes();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this.Symbol.ToDisplayString();

        public bool Equals( ICodeElement other ) =>
            other is CodeElement codeElement &&
            SymbolEqualityComparer.Default.Equals( this.Symbol, codeElement.Symbol );
    }
}
