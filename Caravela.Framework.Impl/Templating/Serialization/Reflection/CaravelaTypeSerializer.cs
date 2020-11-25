using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaTypeSerializer : TypedObjectSerializer<CaravelaType>
    {
        public override ExpressionSyntax Serialize( CaravelaType o )
        {
            if ( o.Symbol.TypeKind == TypeKind.Array )
            {
                var arraySymbol = o.Symbol as IArrayTypeSymbol;
                ExpressionSyntax innerTypeCreation = this.Serialize( new CaravelaType( arraySymbol!.ElementType ) );
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            innerTypeCreation,
                            IdentifierName( "MakeArrayType" ) ) )
                    .NormalizeWhitespace();
            }
            string documentationId = DocumentationCommentId.CreateDeclarationId( o.Symbol );
            var token = IntrinsicsCaller.CreateLdTokenExpression( nameof(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle), documentationId );
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName( "System" ),
                            IdentifierName( "Type" ) ),
                        IdentifierName( "GetTypeFromHandle" ) ) )
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList<ArgumentSyntax>(
                            Argument( token ) ) ) )
                .NormalizeWhitespace();
        }
    }
}