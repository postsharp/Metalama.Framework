using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class UIntPtrSerializer : TypedObjectSerializer<UIntPtr>
    {
        public override ExpressionSyntax Serialize( UIntPtr o )
        {
            return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.IdentifierName( "System" ),
                        SyntaxFactory.IdentifierName( "UIntPtr" ) ) )
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal( o.ToUInt64() ) ) ) );
        }
    }
}