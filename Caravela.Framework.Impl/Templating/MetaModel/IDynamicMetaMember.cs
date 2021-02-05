using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    public interface IDynamicMetaMember
    {
        ExpressionSyntax CreateExpression();
    }

    interface IDynamicMetaMemberDifferentiated : IDynamicMetaMember
    {
        ExpressionSyntax CreateMemberAccessExpression( string member );
    }

    public static class DynamicMetaMemberExtensions
    {
        public static ExpressionSyntax CreateMemberAccessExpression(this IDynamicMetaMember metaMember, string member)
        {
            if ( metaMember is IDynamicMetaMemberDifferentiated metaMemberDifferentiated )
            {
                return metaMemberDifferentiated.CreateMemberAccessExpression( member );
            }

            return SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, metaMember.CreateExpression(), SyntaxFactory.IdentifierName( member ) );
        }
    }

    public interface IProceedImpl
    {
        TypeSyntax CreateTypeSyntax();
        StatementSyntax CreateAssignStatement(string returnValueLocalName);
        StatementSyntax CreateReturnStatement();
    }
}