using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class DoubleSerializer : TypedObjectSerializer<double>
    {
        public override ExpressionSyntax Serialize( double o )
        {
            if ( double.IsPositiveInfinity( o ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token( SyntaxKind.DoubleKeyword ) ),
                    SyntaxFactory.IdentifierName( "PositiveInfinity" ) );
            }
            else if ( double.IsNegativeInfinity( o ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token( SyntaxKind.DoubleKeyword ) ),
                    SyntaxFactory.IdentifierName( "NegativeInfinity" ) );
            }
            else if ( double.IsNaN( o ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token( SyntaxKind.DoubleKeyword ) ),
                    SyntaxFactory.IdentifierName( "NaN" ) );
            }

            return SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( o ) );
        }
    }
}