using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Serialization
{
    internal class DecimalSerializer : TypedObjectSerializer<decimal>
    {
        public override ExpressionSyntax Serialize( decimal o )
        {
            return SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( o ) );
        }
    }
}