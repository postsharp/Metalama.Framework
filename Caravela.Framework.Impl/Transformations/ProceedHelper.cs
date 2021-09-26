// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Transformations
{
    internal static class ProceedHelper
    {
        public static UserExpression CreateProceedDynamicExpression(
            in SyntaxGenerationContext generationContext,
            ExpressionSyntax invocationExpression,
            Template<IMethod> template,
            IMethod overriddenMethod )
        {
            var runtimeAspectHelperType =
                generationContext.SyntaxGenerator.Type( generationContext.ReflectionMapper.GetTypeSymbol( typeof(RunTimeAspectHelper) ) );

            switch ( template.SelectedKind )
            {
                case TemplateKind.Default when overriddenMethod.GetIteratorInfoImpl() is { IsIterator: true } iteratorInfo:
                    {
                        // The target method is a yield-based iterator.

                        ExpressionSyntax expression;

                        if ( !(iteratorInfo.EnumerableKind is EnumerableKind.IAsyncEnumerable or EnumerableKind.IAsyncEnumerator) )
                        {
                            // The target method is a non-async iterator.
                            // Generate:  `RuntimeAspectHelper.Buffer( BASE(ARGS) )`

                            expression =
                                SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            runtimeAspectHelperType,
                                            SyntaxFactory.IdentifierName( nameof(RunTimeAspectHelper.Buffer) ) ) )
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList( SyntaxFactory.SingletonSeparatedList( SyntaxFactory.Argument( invocationExpression ) ) ) )
                                    .WithAdditionalAnnotations( Simplifier.Annotation );
                        }
                        else
                        {
                            // The target method is an async iterator.
                            // Generate: `( await RuntimeAspectHelper.BufferAsync( BASE(ARGS) ) )` 

                            expression = GenerateAwaitBufferAsync();
                        }

                        return new UserExpression( expression, overriddenMethod.ReturnType, generationContext );
                    }

                case TemplateKind.Default when overriddenMethod.GetAsyncInfoImpl() is { IsAsync: true, IsAwaitableOrVoid: true } asyncInfo:
                    {
                        // The target method is an async method (but not an async iterator).
                        // Generate: `( await BASE(ARGS) )`.

                        var taskResultType = asyncInfo.ResultType;

                        return new UserExpression(
                            SyntaxFactory.ParenthesizedExpression( SyntaxFactory.AwaitExpression( invocationExpression ) )
                                .WithAdditionalAnnotations( Simplifier.Annotation ),
                            taskResultType,
                            generationContext );
                    }

                case TemplateKind.Async when overriddenMethod.GetIteratorInfoImpl() is
                    { EnumerableKind: EnumerableKind.IAsyncEnumerable or EnumerableKind.IAsyncEnumerator }:
                    {
                        var expression = GenerateAwaitBufferAsync();

                        return new UserExpression( expression, overriddenMethod.ReturnType, generationContext );
                    }
            }

            // This is a default method, or a non-default template.
            // Generate: `BASE(ARGS)`
            return new UserExpression(
                invocationExpression,
                overriddenMethod.ReturnType,
                generationContext );

            ExpressionSyntax GenerateAwaitBufferAsync()
            {
                var arguments = SyntaxFactory.ArgumentList( SyntaxFactory.SingletonSeparatedList( SyntaxFactory.Argument( invocationExpression ) ) );

                var cancellationTokenParameter = overriddenMethod.Parameters
                    .OfParameterType<CancellationToken>()
                    .LastOrDefault( p => p.Attributes.Any( a => a.Type.Name == "EnumeratorCancellationAttribute" ) );

                if ( cancellationTokenParameter != null )
                {
                    arguments = arguments.AddArguments( SyntaxFactory.Argument( SyntaxFactory.IdentifierName( cancellationTokenParameter.Name ) ) );
                }

                var bufferExpression =
                    SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                runtimeAspectHelperType,
                                SyntaxFactory.IdentifierName( nameof(RunTimeAspectHelper.Buffer) + "Async" ) ) )
                        .WithArgumentList( arguments )
                        .WithAdditionalAnnotations( Simplifier.Annotation );

                var expression = SyntaxFactory.ParenthesizedExpression( SyntaxFactory.AwaitExpression( bufferExpression ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation );

                return expression;
            }
        }
    }
}