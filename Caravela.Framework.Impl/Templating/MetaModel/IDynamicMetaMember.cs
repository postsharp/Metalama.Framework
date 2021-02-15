using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
 
    public interface IDynamicMember
    {
        RuntimeExpression CreateExpression();
    }

    // TODO: Smell
    internal interface IDynamicMemberDifferentiated : IDynamicMember
    {
        RuntimeExpression CreateMemberAccessExpression( string member );
    }

    public static class DynamicMetaMemberExtensions
    {
        public static RuntimeExpression CreateMemberAccessExpression( this IDynamicMember dynamicMember, string member )
        {
            if ( dynamicMember is IDynamicMemberDifferentiated metaMemberDifferentiated )
            {
                return metaMemberDifferentiated.CreateMemberAccessExpression( member );
            }

            return new( SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, dynamicMember.CreateExpression().Syntax, SyntaxFactory.IdentifierName( member ) ), null, false );
        }
    }

    public interface IProceedImpl
    {
        TypeSyntax CreateTypeSyntax();

        StatementSyntax CreateAssignStatement( string returnValueLocalName );

        StatementSyntax CreateReturnStatement();
    }
}