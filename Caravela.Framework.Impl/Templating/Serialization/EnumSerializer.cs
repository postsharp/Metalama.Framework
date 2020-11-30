using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using Microsoft.CSharp;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class EnumSerializer
    {
        private static readonly Type[] _unsignedTypes = new[] {typeof(ushort), typeof(uint), typeof(ulong), typeof(byte)};

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
                Type underlyingType = Enum.GetUnderlyingType( o.GetType() );
                var literal = (_unsignedTypes.Contains( underlyingType )) ? Literal( Convert.ToUInt64( o ) ) : Literal( Convert.ToInt64( o ) );
                return CastExpression(
                    SyntaxFactory.ParseTypeName( typeName ),
                    ParenthesizedExpression(
                        LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            literal ) ) );
            }
        }
    }
}