using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class BoolSerializer : TypedObjectSerializer<bool>
    {
        public override ExpressionSyntax Serialize( bool o )
        {
            return LiteralExpression( o ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression );
        }
    }
}