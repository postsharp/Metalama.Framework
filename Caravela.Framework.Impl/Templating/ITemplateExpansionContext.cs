using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal interface ITemplateExpansionContext
    {
        StatementSyntax CreateReturnStatement( ExpressionSyntax? returnExpression );
    }
}
