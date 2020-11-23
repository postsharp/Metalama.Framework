using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaMethodInfoSerializer : TypedObjectSerializer<CaravelaMethodInfo>
    {
        public override ExpressionSyntax Serialize( CaravelaMethodInfo o )
        {
            string documentationId = DocumentationCommentId.CreateDeclarationId( o.Symbol );
            return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName( "System" ),
                                SyntaxFactory.IdentifierName( "Reflection" ) ),
                            SyntaxFactory.IdentifierName( "MethodBase" ) ),
                        SyntaxFactory.IdentifierName( "GetMethodFromHandle" ) ) )
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                            SyntaxFactory.Argument(
                                SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName( "Caravela" ),
                                                    SyntaxFactory.IdentifierName( "Compiler" ) ),
                                                SyntaxFactory.IdentifierName( "Intrinsics" ) ),
                                            SyntaxFactory.IdentifierName( "GetRuntimeMethodHandle" ) ) )
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        SyntaxFactory.Literal(documentationId) ) ) ) ) ) ) ) ) )
                .NormalizeWhitespace();
        }
    }
}