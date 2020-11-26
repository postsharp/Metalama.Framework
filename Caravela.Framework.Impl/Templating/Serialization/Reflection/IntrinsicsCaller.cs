
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class IntrinsicsCaller
    {
        /// <summary>
        /// Returns <c>Caravela.Compiler.Intrinsics.methodName(documentationId)</c>.
        /// </summary>
        /// <param name="methodName">GetRuntimeMethodHandle, GetRuntimeFieldHandle, or GetRuntimeTypeHandle.</param>
        /// <param name="documentationId">The string to pass to the method.</param>
        /// <returns>Roslyn expression that represents the invocation of the method. The type of the expression is a metadata token.</returns>
        public static InvocationExpressionSyntax CreateLdTokenExpression(string methodName, string documentationId)
        {
            return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName( "Caravela" ),
                                SyntaxFactory.IdentifierName( "Compiler" ) ),
                            SyntaxFactory.IdentifierName( "Intrinsics" ) ),
                        SyntaxFactory.IdentifierName( methodName ) ) )
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal( documentationId ) ) ) ) ) );
        } 
    }
}