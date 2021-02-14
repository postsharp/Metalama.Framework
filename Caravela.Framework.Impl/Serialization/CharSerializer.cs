using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{

    internal class CharSerializer : TypedObjectSerializer<char>
    {
        public override ExpressionSyntax Serialize( char o )
        {
            return LiteralExpression( SyntaxKind.CharacterLiteralExpression, Literal( o ) );
        }
    }
}