using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaMethodInfoSerializer : TypedObjectSerializer<CaravelaMethodInfo>
    {
        private readonly CaravelaTypeSerializer _typeSerializer;

        public CaravelaMethodInfoSerializer( CaravelaTypeSerializer typeSerializer )
        {
            this._typeSerializer = typeSerializer;
        }
        
        public override ExpressionSyntax Serialize( CaravelaMethodInfo o )
        {
            return CreateMethodBase( this._typeSerializer, o );
        }

        public static ExpressionSyntax CreateMethodBase( CaravelaTypeSerializer typeSerializer, ICaravelaMethodOrConstructorInfo info )
        {
            IMethodSymbol methodSymbol = (info.Symbol as IMethodSymbol)!.OriginalDefinition;
            string documentationId = DocumentationCommentId.CreateDeclarationId( methodSymbol );
            var methodToken = IntrinsicsCaller.CreateLdTokenExpression( nameof(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle), documentationId );
            if ( info.DeclaringTypeSymbol != null )
            {
                var typeHandle = CreateTypeHandleExpression(typeSerializer, info.DeclaringTypeSymbol );
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
                        Argument( methodToken ), Argument( typeHandle )
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

        private static ExpressionSyntax CreateTypeHandleExpression(CaravelaTypeSerializer typeSerializer, ITypeSymbol? type )
        {
            ExpressionSyntax typeExpression = typeSerializer.CreateTypeCreationExpressionFromSymbolRecursive(type);
            ExpressionSyntax typeHandle = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                typeExpression,
                IdentifierName("TypeHandle"));
            return typeHandle;
        }
    }
}