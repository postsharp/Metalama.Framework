using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class IntSerializer : TypedObjectSerializer<int>
    {
        public override ExpressionSyntax Serialize( int o )
        {
            return LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( o ) );
        }
    }
}