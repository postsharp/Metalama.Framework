using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class StringSerializer : TypedObjectSerializer<string>
    {
        public override ExpressionSyntax Serialize( string o )
        {
            return SyntaxFactory.LiteralExpression( SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal( o ) );
        }
    }
}