using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    internal interface IToSyntax
    {
        CSharpSyntaxNode GetSyntaxNode();
        IEnumerable<CSharpSyntaxNode> GetSyntaxNodes();
    }
}
