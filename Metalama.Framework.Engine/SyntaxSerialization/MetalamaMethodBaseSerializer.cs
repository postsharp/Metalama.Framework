// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Immutable;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodBase = System.Reflection.MethodBase;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal abstract class MetalamaMethodBaseSerializer<TInput, TOutput> : ObjectSerializer<TInput, TOutput>
        where TInput : MethodBase, TOutput
        where TOutput : MethodBase
    {
        public MetalamaMethodBaseSerializer( SyntaxSerializationService service ) : base( service ) { }

        internal static ExpressionSyntax SerializeMethodBase(
            ICompileTimeReflectionObject<IMethodBase> method,
            SyntaxSerializationContext serializationContext )
            => SerializeMethodBase(
                method.Target.GetTarget( serializationContext.CompilationModel )
                    .AssertNotNull( Justifications.SerializersNotImplementedForIntroductions ),
                serializationContext );

        internal static ExpressionSyntax SerializeMethodBase( IMethodBase method, SyntaxSerializationContext serializationContext )
        {
            return SerializeMethodBase( method, method.DeclaringType.GetSymbol(), serializationContext );
        }

        private static ExpressionSyntax SerializeMethodBase(
            IMethodBase method,
            ITypeSymbol? declaringGenericTypeSymbol,
            SyntaxSerializationContext serializationContext )
        {
            // var methodSymbol = method.GetOriginalDefinition().GetSymbol();
            // var documentationId = DocumentationCommentId.CreateDeclarationId( methodSymbol.AssertNotNull() );
            // var methodToken = IntrinsicsCaller.CreateLdTokenExpression( nameof(Intrinsics.GetRuntimeMethodHandle), documentationId );

            var typeCreation = TypeSerializationHelper.SerializeTypeSymbolRecursive( method.DeclaringType.GetSymbol(), serializationContext );

            var allBindingFlags = SyntaxUtility.CreateBindingFlags( serializationContext );

            var invokeGetMethodToken = InvocationExpression(
                    MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            typeCreation,
                            IdentifierName( "GetMethod" ) )
                        .WithAdditionalAnnotations( Simplifier.Annotation ) )
                .AddArgumentListArguments(
                    Argument(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal( method.Name ) ) ),
                    Argument( allBindingFlags ) );

            var accessMethodMemberToken = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    invokeGetMethodToken,
                    IdentifierName( "MethodHandle" ) );

            ExpressionSyntax invokeGetMethodFromHandle;

            if ( declaringGenericTypeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType )
            {
                var typeHandle = CreateTypeHandleExpression( declaringGenericTypeSymbol, serializationContext );

                invokeGetMethodFromHandle = InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            serializationContext.GetTypeSyntax( typeof(MethodBase) ),
                            IdentifierName( "GetMethodFromHandle" ) ) )
                    .AddArgumentListArguments( Argument( accessMethodMemberToken ), Argument( typeHandle ) );
            }
            else
            {
                invokeGetMethodFromHandle = InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            serializationContext.GetTypeSyntax( typeof(MethodBase) ),
                            IdentifierName( "GetMethodFromHandle" ) ) )
                    .AddArgumentListArguments( Argument( accessMethodMemberToken ) );
            }

            if ( serializationContext.CompilationModel.Project.PreprocessorSymbols.Contains( "NET" ) )
            {
                // In the new .NET, the API is marked for nullability, so we have to suppress the warning.
                invokeGetMethodFromHandle = PostfixUnaryExpression( SyntaxKind.SuppressNullableWarningExpression, invokeGetMethodFromHandle );
            }

            return invokeGetMethodFromHandle
                .NormalizeWhitespace();
        }

        private static ExpressionSyntax CreateTypeHandleExpression( ITypeSymbol type, SyntaxSerializationContext serializationContext )
        {
            var typeExpression = TypeSerializationHelper.SerializeTypeSymbolRecursive( type, serializationContext );

            ExpressionSyntax typeHandle = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                typeExpression,
                IdentifierName( "TypeHandle" ) );

            return typeHandle;
        }

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof(MemberInfo), typeof(MethodBase) );
    }
}