using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Serialization
{
    internal class SByteSerializer : TypedObjectSerializer<sbyte>
    {
        public override ExpressionSyntax Serialize( sbyte o )
        {
            return SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( o ) );
        }
    }
}