using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaMethodInfoSerializer : TypedObjectSerializer<CaravelaMethodInfo>
    {
        public override ExpressionSyntax Serialize( CaravelaMethodInfo o )
        {
            return CreateMethodBase( o );
        }

        public static ExpressionSyntax CreateMethodBase( ICaravelaMethodOrConstructorInfo info )
        {
            string documentationId = DocumentationCommentId.CreateDeclarationId( info.Symbol );
            var methodToken = IntrinsicsCaller.CreateLdTokenExpression( nameof(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle), documentationId );
            if ( info.DeclaringTypeSymbol != null )
            {
                string typeDocumentationId = DocumentationCommentId.CreateDeclarationId( info.DeclaringTypeSymbol );
                var typeToken = IntrinsicsCaller.CreateLdTokenExpression( nameof(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle), typeDocumentationId );
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName( "System" ),
                                    IdentifierName( "Reflection" ) ),
                                IdentifierName( "MethodBase" ) ),
                            IdentifierName( "GetMethodFromHandle" ) ) )
                    .AddArgumentListArguments(
                        Argument( methodToken ), Argument( typeToken )
                    ).NormalizeWhitespace();
            }
            else
            {
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
                    .AddArgumentListArguments( SyntaxFactory.Argument( methodToken ) ) 
                    .NormalizeWhitespace();
            }
        }
    }
}