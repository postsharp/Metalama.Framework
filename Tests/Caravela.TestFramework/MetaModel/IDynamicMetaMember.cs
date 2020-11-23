using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.TestFramework.MetaModel
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