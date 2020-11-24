using System.Collections.Generic;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    public static class CodeModelExtensions
    {
        public static CSharpSyntaxNode? GetSyntaxNode(this ICodeElement codeElement) => (codeElement as IToSyntax)?.GetSyntaxNode();
        public static IEnumerable<CSharpSyntaxNode>? GetSyntaxNodes(this ICodeElement codeElement) => (codeElement as IToSyntax)?.GetSyntaxNodes();
    }
}
