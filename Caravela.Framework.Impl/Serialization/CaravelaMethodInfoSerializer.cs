// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaMethodInfoSerializer : TypedObjectSerializer<CompileTimeMethodInfo>
    {
        private readonly CaravelaTypeSerializer _typeSerializer;

        public static ExpressionSyntax CreateMethodBase( CaravelaTypeSerializer typeSerializer, IReflectionMockMember info ) =>
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

        public CaravelaMethodInfoSerializer( CaravelaTypeSerializer typeSerializer )
        {
            this._typeSerializer = typeSerializer;
        }

        public override ExpressionSyntax Serialize( CompileTimeMethodInfo o )
        {
            return CreateMethodBase( this._typeSerializer, o );
        }
    }
}