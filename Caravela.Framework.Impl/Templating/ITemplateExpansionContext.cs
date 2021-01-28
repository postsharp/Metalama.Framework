using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Templating
{
    internal interface ITemplateExpansionContext
    {
        StatementSyntax CreateReturnStatement( ExpressionSyntax? returnExpression );
        SyntaxNode RewriteIdentifier( SyntaxNode identifierNode, ISymbol symbol );
        ITemplateLexicalScope OpenLexicalScope();
    }
}
