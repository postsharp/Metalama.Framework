using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class ULongSerializer : TypedObjectSerializer<ulong>
    {
        public override ExpressionSyntax Serialize( ulong o )
        {
            return SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( o ) );
        }
    }
}