// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Transformations;

internal static class ProceedHelper
{
    public static SyntaxUserExpression CreateProceedDynamicExpression(
        SyntaxGenerationContext generationContext,
        ExpressionSyntax invocationExpression,
        TemplateKind selectedTemplateKind,
        IMethod overriddenMethod )
    {
        var runtimeAspectHelperType =
            generationContext.SyntaxGenerator.Type( generationContext.ReflectionMapper.GetTypeSymbol( typeof(RunTimeAspectHelper) ) );

        switch ( selectedTemplateKind )
        {
            case TemplateKind.Default when overriddenMethod.GetIteratorInfoImpl() is { IsIteratorMethod: true } iteratorInfo:
                {
                    // The target method is a yield-based iterator.

                    ExpressionSyntax expression;

                    if ( !(iteratorInfo.EnumerableKind is EnumerableKind.IAsyncEnumerable or EnumerableKind.IAsyncEnumerator) )
                    {
                        // The target method is a non-async iterator.
                        // Generate:  `RuntimeAspectHelper.Buffer( BASE(ARGS) )`

                        expression =
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        runtimeAspectHelperType,
                                        IdentifierName( nameof(RunTimeAspectHelper.Buffer) ) ) )
                                .WithArgumentList(
                                    ArgumentList( SingletonSeparatedList( Argument( invocationExpression ) ) ) )
                                .WithAdditionalAnnotations( Simplifier.Annotation );
                    }
                    else
                    {
                        // The target method is an async iterator.
                        // Generate: `( await RuntimeAspectHelper.BufferAsync( BASE(ARGS) ) )` 

                        expression = GenerateAwaitBufferAsync();
                    }

                    return new SyntaxUserExpression( expression, overriddenMethod.ReturnType );
                }

            case TemplateKind.Default when overriddenMethod.GetAsyncInfoImpl() is { IsAsync: true, IsAwaitableOrVoid: true } asyncInfo:
                {
                    // The target method is an async method (but not an async iterator).
                    // Generate (awaitable non-void): `( await BASE(ARGS) )`.
                    //           Or (awaitable void): `await BASE(ARGS)`.
                    //           Or (void)          : `await __LinkerInjectionHelpers__.__AsyncVoidMethod(BASE)(ARGS)`

                    switch ( asyncInfo )
                    {
                        case { } when overriddenMethod.ReturnType.Is( SpecialType.Void ):
                            return WrapAsyncVoid( invocationExpression, overriddenMethod, true );

                        case { ResultType: var resultType } when resultType.Is( SpecialType.Void ):
                            return
                                new SyntaxUserExpression(
                                    AwaitExpression(
                                            Token( SyntaxKind.AwaitKeyword ).WithTrailingTrivia( Space ),
                                            invocationExpression )
                                        .WithAdditionalAnnotations( Simplifier.Annotation ),
                                    resultType );

                        default:
                            return
                                new SyntaxUserExpression(
                                    ParenthesizedExpression(
                                            AwaitExpression(
                                                Token( SyntaxKind.AwaitKeyword ).WithTrailingTrivia( Space ),
                                                invocationExpression ) )
                                        .WithAdditionalAnnotations( Simplifier.Annotation ),
                                    asyncInfo.ResultType );
                    }
                }

            case TemplateKind.Async when overriddenMethod.ReturnType.Is( SpecialType.Void ):
                return WrapAsyncVoid( invocationExpression, overriddenMethod, false );

            case TemplateKind.Async when overriddenMethod.GetIteratorInfoImpl() is
                { EnumerableKind: EnumerableKind.IAsyncEnumerable or EnumerableKind.IAsyncEnumerator }:
                {
                    var expression = GenerateAwaitBufferAsync();

                    return new SyntaxUserExpression( expression, overriddenMethod.ReturnType );
                }
        }

        // This is a default method, or a non-default template.
        // Generate: `BASE(ARGS)`
        return new SyntaxUserExpression(
            invocationExpression,
            overriddenMethod.ReturnType );

        ExpressionSyntax GenerateAwaitBufferAsync()
        {
            var arguments = ArgumentList( SingletonSeparatedList( Argument( invocationExpression ) ) );

            var cancellationTokenParameter = overriddenMethod.Parameters
                .OfParameterType<CancellationToken>()
                .LastOrDefault( p => p.Attributes.Any( a => a.Type.Name == "EnumeratorCancellationAttribute" ) );

            if ( cancellationTokenParameter != null )
            {
                arguments = arguments.AddArguments( Argument( IdentifierName( cancellationTokenParameter.Name ) ) );
            }
            
            var bufferExpression =
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            runtimeAspectHelperType,
                            IdentifierName( nameof(RunTimeAspectHelper.Buffer) + "Async" ) ) )
                    .WithArgumentList( arguments )
                    .WithAdditionalAnnotations( Simplifier.Annotation );

            var expression = ParenthesizedExpression(
                    AwaitExpression(
                        Token( SyntaxKind.AwaitKeyword ).WithTrailingTrivia( Space ),
                        bufferExpression ) )
                .WithAdditionalAnnotations( Simplifier.Annotation );

            return expression;
        }

        static SyntaxUserExpression WrapAsyncVoid( ExpressionSyntax invocationExpression, IMethod overriddenMethod, bool await )
        {
            if ( invocationExpression is not InvocationExpressionSyntax { Expression: { } invocationTarget } actualInvocationExpression )
            {
                throw new AssertionFailedException( $"Expected invocation expression, got {invocationExpression.Kind()}" );
            }

            var expression =
                actualInvocationExpression.WithExpression(
                        InvocationExpression(
                            LinkerInjectionHelperProvider.GetAsyncVoidMethodMemberExpression(),
                            ArgumentList( SingletonSeparatedList( Argument( invocationTarget ) ) ) ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation );

            if ( await )
            {
                return
                    new SyntaxUserExpression(
                        AwaitExpression(
                            Token( TriviaList(), SyntaxKind.AwaitKeyword, TriviaList( ElasticSpace ) ),
                            expression ),
                        overriddenMethod.ReturnType );
            }
            else
            {
                return
                    new SyntaxUserExpression(
                        expression,
                        overriddenMethod.ReturnType );
            }
        }
    }
}