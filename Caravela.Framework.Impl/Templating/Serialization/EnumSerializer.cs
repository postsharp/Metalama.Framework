using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using Microsoft.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class EnumSerializer
    {
        public ExpressionSyntax Serialize( Enum o )
        {
            Type enumType = o.GetType();
            string typeName = TypeNameUtility.ToCSharpQualifiedName( enumType );
            string? name = Enum.GetName( enumType, o );
            if ( name != null )
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ParseExpression( typeName ),
                    IdentifierName( name ) );
            }
            else
            {
                return CastExpression(
                    SyntaxFactory.ParseTypeName( typeName ),
                    LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        Literal( Convert.ToUInt64( o ) ) ) );
            }
        }
    }
}