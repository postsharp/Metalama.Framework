using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.AspectWorkbench
{
    public interface IDynamicMetaMember
    {
        ExpressionSyntax CreateExpression();
    }

    public interface IProceedImpl
    {
        TypeSyntax CreateTypeSyntax();
        StatementSyntax CreateAssignStatement(string returnValueLocalName);
        StatementSyntax CreateReturnStatement();
        
    }
}