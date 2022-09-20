// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodBase = System.Reflection.MethodBase;
using RefKind = Metalama.Framework.Code.RefKind;

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
            // The following is the old code that uses Intrinsics.
            /*
            var methodSymbol = method.GetOriginalDefinition().GetSymbol();
            var documentationId = DocumentationCommentId.CreateDeclarationId( methodSymbol.AssertNotNull() );
            var methodToken = IntrinsicsCaller.CreateLdTokenExpression( nameof(Intrinsics.GetRuntimeMethodHandle), documentationId );
            */

            var typeCreation = TypeSerializationHelper.SerializeTypeSymbolRecursive( method.DeclaringType.GetSymbol(), serializationContext );
            var allBindingFlags = SyntaxUtility.CreateBindingFlags( method, serializationContext );
            var reflectionHelperTypeSyntax = serializationContext.SyntaxGenerator.Type( serializationContext.GetTypeSymbol( typeof(ReflectionHelper) ) );

            ExpressionSyntax parameterTypeArray;

            if ( method.Parameters.Count > 0 )
            {
                var methodBaseParametersExpressions = method.Parameters.Select(
                    p => p.RefKind != RefKind.None
                        ? (ExpressionSyntax) InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    serializationContext.SyntaxGenerator.TypeOfExpression( p.Type.GetSymbol() ),
                                    IdentifierName( "MakeByRefType" ) ) )
                            .AddArgumentListArguments()
                        : serializationContext.SyntaxGenerator.TypeOfExpression( p.Type.GetSymbol() ) );

                parameterTypeArray = ImplicitArrayCreationExpression(
                    InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SeparatedList( methodBaseParametersExpressions ) ) );
            }
            else
            {
                parameterTypeArray = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    serializationContext.SyntaxGenerator.Type( serializationContext.GetTypeSymbol( typeof(Type) ) ),
                    IdentifierName( nameof(Type.EmptyTypes) ) );
            }

            ExpressionSyntax invokeGetMethod;

            if ( method is IConstructor constructor )
            {
                if ( ReflectionSignatureBuilder.HasTypeArgument( constructor ) )
                {
                    invokeGetMethod = InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                reflectionHelperTypeSyntax,
                                IdentifierName( nameof(ReflectionHelper.GetConstructor) ) ) )
                        .AddArgumentListArguments(
                            Argument( typeCreation ),
                            Argument( allBindingFlags ),
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal( ReflectionSignatureBuilder.GetConstructorSignature( constructor ) ) ) ) );
                }
                else
                {
                    invokeGetMethod = InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                typeCreation,
                                IdentifierName( "GetConstructor" ) ) )
                        .AddArgumentListArguments(
                            Argument( allBindingFlags ),
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NullLiteralExpression ) ),
                            Argument( parameterTypeArray ),
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NullLiteralExpression ) ) );
                }
            }
            else
            {
                if ( ReflectionSignatureBuilder.HasTypeArgument( (IMethod) method ) )
                {
                    invokeGetMethod = InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                reflectionHelperTypeSyntax,
                                IdentifierName( nameof(ReflectionHelper.GetMethod) ) ) )
                        .AddArgumentListArguments(
                            Argument( typeCreation ),
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal( method.Name ) ) ),
                            Argument( allBindingFlags ),
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal( ReflectionSignatureBuilder.GetMethodSignature( (IMethod) method ) ) ) ) );
                }
                else
                {
                    invokeGetMethod = InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                typeCreation,
                                IdentifierName( "GetMethod" ) ) )
                        .AddArgumentListArguments(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal( method.Name ) ) ),
                            Argument( allBindingFlags ),
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NullLiteralExpression ) ),
                            Argument( parameterTypeArray ),
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NullLiteralExpression ) ) );
                }
            }

            if ( serializationContext.CompilationModel.Project.PreprocessorSymbols.Contains( "NET" ) )
            {
                // In the new .NET, the API is marked for nullability, so we have to suppress the warning.
                invokeGetMethod = PostfixUnaryExpression( SyntaxKind.SuppressNullableWarningExpression, invokeGetMethod );
            }

            return invokeGetMethod.NormalizeWhitespace();
        }

        // The following is the old code that was used alongside Intrinsics.
        /*
        private static ExpressionSyntax CreateTypeHandleExpression( ITypeSymbol type, SyntaxSerializationContext serializationContext )
        {
            var typeExpression = TypeSerializationHelper.SerializeTypeSymbolRecursive( type, serializationContext );

            ExpressionSyntax typeHandle = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                typeExpression,
                IdentifierName( "TypeHandle" ) );

            return typeHandle;
        }
        */

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof(MemberInfo), typeof(MethodBase) );
    }
}