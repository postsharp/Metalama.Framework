using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class FloatSerializer : TypedObjectSerializer<float>
    {
        public override ExpressionSyntax Serialize( float o )
        {
            if ( float.IsPositiveInfinity( o ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token( SyntaxKind.FloatKeyword ) ),
                    SyntaxFactory.IdentifierName( "PositiveInfinity" ) );
            }
            else if ( float.IsNegativeInfinity( o ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token( SyntaxKind.FloatKeyword ) ),
                    SyntaxFactory.IdentifierName( "NegativeInfinity" ) );
            }
            else if ( float.IsNaN( o ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token( SyntaxKind.FloatKeyword ) ),
                    SyntaxFactory.IdentifierName( "NaN" ) );
            }

            return SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( o ) );
        }
    }
}