using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface ISourceCodeElement 
    {
        ISymbol Symbol { get; }
    }

    internal static class SourceCodeElementExtensions
    {
        public static IEnumerable<CSharpSyntaxNode> ToSyntaxNodes(this ISourceCodeElement element) 
            => element.Symbol.DeclaringSyntaxReferences.Select( r => (CSharpSyntaxNode) r.GetSyntax() );
        
        // TODO: special case partial methods?
        public static CSharpSyntaxNode GetSyntaxNode(this ISourceCodeElement codeElement) => codeElement.ToSyntaxNodes().Single();

        public static IEnumerable<CSharpSyntaxNode> GetSyntaxNodes(this ISourceCodeElement codeElement) => codeElement.ToSyntaxNodes();
    }
    
    internal abstract class CodeElement : ICodeElement
    {

        internal SymbolMap SymbolMap => this.Compilation.SymbolMap;

        public abstract CodeElement? ContainingElement { get; }

        public abstract IImmutableList<Attribute> Attributes { get; }

        public abstract CodeElementKind ElementKind { get; }

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );
    }
}
