using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Templating
{
    interface ITemplateLexicalScope : IDisposable
    {
        void DefineLocalVariable( string name );
        SyntaxNode RewriteLocalVariable( SyntaxNode identifierNode, ISymbol symbol );
    }
}
