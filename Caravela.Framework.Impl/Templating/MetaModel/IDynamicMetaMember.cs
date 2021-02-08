using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    public interface IDynamicMetaMember
    {
        RuntimeExpression CreateExpression();
    }

    internal interface IDynamicMetaMemberDifferentiated : IDynamicMetaMember
    {
        RuntimeExpression CreateMemberAccessExpression( string member );
    }

    public static class DynamicMetaMemberExtensions
    {
        public static RuntimeExpression CreateMemberAccessExpression( this IDynamicMetaMember metaMember, string member )
        {
            if ( metaMember is IDynamicMetaMemberDifferentiated metaMemberDifferentiated )
            {
                return metaMemberDifferentiated.CreateMemberAccessExpression( member );
            }

            return new( SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, metaMember.CreateExpression().Syntax, SyntaxFactory.IdentifierName( member ) ) );
        }
    }

    public interface IProceedImpl
    {
        TypeSyntax CreateTypeSyntax();

        StatementSyntax CreateAssignStatement( string returnValueLocalName );

        StatementSyntax CreateReturnStatement();
    }
}