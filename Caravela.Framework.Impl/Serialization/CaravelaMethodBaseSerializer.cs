// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MethodBase = System.Reflection.MethodBase;

namespace Caravela.Framework.Impl.Serialization
{
    internal abstract class CaravelaMethodBaseSerializer : ObjectSerializer
    {
        public CaravelaMethodBaseSerializer( SyntaxSerializationService service ) : base( service ) { }

        internal ExpressionSyntax SerializeMethodBase( IReflectionMockMember method, ISyntaxFactory syntaxFactory )
            => this.SerializeMethodBase( (IMethodSymbol) method.Symbol, method.DeclaringTypeSymbol, syntaxFactory );

        internal ExpressionSyntax SerializeMethodBase(
            IMethodSymbol methodSymbol,
            ITypeSymbol? declaringGenericTypeSymbol,
            ISyntaxFactory syntaxFactory )
        {
            methodSymbol = methodSymbol.OriginalDefinition;
            var documentationId = DocumentationCommentId.CreateDeclarationId( methodSymbol );
            var methodToken = IntrinsicsCaller.CreateLdTokenExpression( nameof(Intrinsics.GetRuntimeMethodHandle), documentationId );

            if ( declaringGenericTypeSymbol != null )
            {
                var typeHandle = this.CreateTypeHandleExpression( declaringGenericTypeSymbol, syntaxFactory );

                return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            syntaxFactory.GetTypeSyntax( typeof(MethodBase) ),
                            SyntaxFactory.IdentifierName( "GetMethodFromHandle" ) ) )
                    .AddArgumentListArguments( SyntaxFactory.Argument( methodToken ), SyntaxFactory.Argument( typeHandle ) )
                    .NormalizeWhitespace();
            }

            return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        syntaxFactory.GetTypeSyntax( typeof(MethodBase) ),
                        SyntaxFactory.IdentifierName( "GetMethodFromHandle" ) ) )
                .AddArgumentListArguments( SyntaxFactory.Argument( methodToken ) )
                .NormalizeWhitespace();
        }

        private ExpressionSyntax CreateTypeHandleExpression( ITypeSymbol type, ISyntaxFactory syntaxFactory )
        {
            var typeExpression = this.Service.TypeSerializer.SerializeTypeSymbolRecursive( type, syntaxFactory );

            ExpressionSyntax typeHandle = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                typeExpression,
                SyntaxFactory.IdentifierName( "TypeHandle" ) );

            return typeHandle;
        }
    }
}