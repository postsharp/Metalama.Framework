using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace PostSharp.Framework.Sdk
{
    internal interface IToSyntax
    {
        CSharpSyntaxNode ToSyntaxNode();
        IEnumerable<CSharpSyntaxNode> ToSyntaxNodes();
    }
}
