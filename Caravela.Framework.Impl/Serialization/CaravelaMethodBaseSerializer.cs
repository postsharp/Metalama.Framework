// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Caravela.Framework.Impl.Serialization
{
    internal abstract class CaravelaMethodBaseSerializer<TInput, TOutput> : ObjectSerializer<TInput, TOutput>
        where TInput : MethodBase, TOutput
        where TOutput : MethodBase
    {
        public CaravelaMethodBaseSerializer( SyntaxSerializationService service ) : base( service ) { }

        internal static ExpressionSyntax SerializeMethodBase(
            ICompileTimeReflectionObject<IMethodBase> method,
            SyntaxSerializationContext serializationContext )
            => SerializeMethodBase(
                (IMethodSymbol) method.Target.GetSymbol( serializationContext.Compilation )
                    .AssertNotNull( Justifications.SerializersNotImplementedForIntroductions ),
                serializationContext );

        internal static ExpressionSyntax SerializeMethodBase( IMethodSymbol methodSymbol, SyntaxSerializationContext serializationContext )
        {
            return SerializeMethodBase( methodSymbol, methodSymbol.ContainingType, serializationContext );
        }

        private static ExpressionSyntax SerializeMethodBase(
            IMethodSymbol methodSymbol,
            ITypeSymbol? declaringGenericTypeSymbol,
            SyntaxSerializationContext serializationContext )
        {
            methodSymbol = methodSymbol.OriginalDefinition;
            var documentationId = DocumentationCommentId.CreateDeclarationId( methodSymbol );
            var methodToken = IntrinsicsCaller.CreateLdTokenExpression( nameof(Intrinsics.GetRuntimeMethodHandle), documentationId );

            ExpressionSyntax invokeGetMethodFromHandle;

            if ( declaringGenericTypeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType )
            {
                var typeHandle = CreateTypeHandleExpression( declaringGenericTypeSymbol, serializationContext );

                invokeGetMethodFromHandle = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            serializationContext.GetTypeSyntax( typeof(MethodBase) ),
                            SyntaxFactory.IdentifierName( "GetMethodFromHandle" ) ) )
                    .AddArgumentListArguments( SyntaxFactory.Argument( methodToken ), SyntaxFactory.Argument( typeHandle ) );

             
            }
            else
            {

                invokeGetMethodFromHandle = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            serializationContext.GetTypeSyntax( typeof(MethodBase) ),
                            SyntaxFactory.IdentifierName( "GetMethodFromHandle" ) ) )
                    .AddArgumentListArguments( SyntaxFactory.Argument( methodToken ) );
            }

            if ( serializationContext.CompilationModel.Project.PreprocessorSymbols.Contains( "NET" ) )
            {
                // In the new .NET, the API is marked for nullability, so we have to suppress the warning.
                invokeGetMethodFromHandle = SyntaxFactory.PostfixUnaryExpression( SyntaxKind.SuppressNullableWarningExpression, invokeGetMethodFromHandle );
            }

            return invokeGetMethodFromHandle
                .NormalizeWhitespace();
        }

        private static ExpressionSyntax CreateTypeHandleExpression( ITypeSymbol type, SyntaxSerializationContext serializationContext )
        {
            var typeExpression = TypeSerializer.SerializeTypeSymbolRecursive( type, serializationContext );

            ExpressionSyntax typeHandle = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                typeExpression,
                SyntaxFactory.IdentifierName( "TypeHandle" ) );

            return typeHandle;
        }

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof(MemberInfo), typeof(MethodBase) );
    }
}