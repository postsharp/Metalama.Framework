using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization.Reflection
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

        public static ExpressionSyntax CreateMethodBase( CaravelaTypeSerializer typeSerializer, ICaravelaMethodOrConstructorInfo info ) =>
            CreateMethodBase( typeSerializer, (IMethodSymbol) info.Symbol, info.DeclaringTypeSymbol );

        public static ExpressionSyntax CreateMethodBase( CaravelaTypeSerializer typeSerializer, IMethodSymbol methodSymbol, ITypeSymbol? declaringGenericTypeSymbol )
        {
            methodSymbol = methodSymbol.OriginalDefinition;
            var documentationId = DocumentationCommentId.CreateDeclarationId( methodSymbol );
            var methodToken = IntrinsicsCaller.CreateLdTokenExpression( nameof( Compiler.Intrinsics.GetRuntimeMethodHandle ), documentationId );
            if ( declaringGenericTypeSymbol != null )
            {
                var typeHandle = CreateTypeHandleExpression( typeSerializer, declaringGenericTypeSymbol );
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
                        Argument( methodToken ), Argument( typeHandle ) ).NormalizeWhitespace();
            }
            else
            {
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
                    .AddArgumentListArguments( Argument( methodToken ) )
                    .NormalizeWhitespace();
            }
        }

        private static ExpressionSyntax CreateTypeHandleExpression( CaravelaTypeSerializer typeSerializer, ITypeSymbol type )
        {
            var typeExpression = typeSerializer.CreateTypeCreationExpressionFromSymbolRecursive( type );
            ExpressionSyntax typeHandle = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                typeExpression,
                IdentifierName( "TypeHandle" ) );
            return typeHandle;
        }
    }
}