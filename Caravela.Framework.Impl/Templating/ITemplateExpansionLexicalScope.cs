using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal interface ITemplateExpansionLexicalScope : IDisposable
    {
        SyntaxToken DefineIdentifier( string name );

        IdentifierNameSyntax CreateIdentifierName( string name );

        ITemplateExpansionLexicalScope OpenNestedScope();
    }
}
