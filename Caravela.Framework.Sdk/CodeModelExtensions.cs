using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    public static class CodeModelExtensions
    {
        public static CSharpSyntaxNode GetSyntaxNode(this ICodeElement codeElement) => ((IToSyntax)codeElement).GetSyntaxNode();
        public static IEnumerable<CSharpSyntaxNode> GetSyntaxNodes(this ICodeElement codeElement) => ((IToSyntax)codeElement).GetSyntaxNodes();
    }
}
