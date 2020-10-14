using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace PostSharp.Framework.Sdk
{
    public static class CodeModelExtensions
    {
        public static CSharpSyntaxNode ToSyntaxNode(this ICodeElement codeElement) => ((IToSyntax)codeElement).ToSyntaxNode();
        public static IEnumerable<CSharpSyntaxNode> ToSyntaxNodes(this ICodeElement codeElement) => ((IToSyntax)codeElement).ToSyntaxNodes();
    }
}
