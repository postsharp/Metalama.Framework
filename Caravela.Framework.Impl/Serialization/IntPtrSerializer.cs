using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Serialization
{
    internal class IntPtrSerializer : TypedObjectSerializer<IntPtr>
    {
        public override ExpressionSyntax Serialize( IntPtr o )
        {
            return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.IdentifierName( "System" ),
                        SyntaxFactory.IdentifierName( "IntPtr" ) ) )
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal( o.ToInt64() ) ) ) );
        }
    }
}