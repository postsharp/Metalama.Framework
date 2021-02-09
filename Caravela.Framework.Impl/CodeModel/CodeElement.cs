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
        ICodeElement? ICodeElement.ContainingElement => this.ContainingElement;

        public abstract CodeElement? ContainingElement { get; }

        IReadOnlyList<IAttribute> ICodeElement.Attributes => this.Attributes;

        public abstract IReadOnlyList<Attribute> Attributes { get; }

        CodeElementKind ICodeElement.ElementKind => this.ElementKind;

        public abstract CodeElementKind ElementKind { get; }

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );

        public abstract bool Equals( ICodeElement other );
    }
}
