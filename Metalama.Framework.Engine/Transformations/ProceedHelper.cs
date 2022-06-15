// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Transformations
{
    internal static class ProceedHelper
    {
        public static BuiltUserExpression CreateProceedDynamicExpression(
            SyntaxGenerationContext generationContext,
            ExpressionSyntax invocationExpression,
            BoundTemplateMethod template,
            IMethod overriddenMethod )
            => CreateProceedDynamicExpression( generationContext, invocationExpression, template.Template.SelectedKind, overriddenMethod );

        public static BuiltUserExpression CreateProceedDynamicExpression(
            SyntaxGenerationContext generationContext,
            ExpressionSyntax invocationExpression,
            TemplateKind selectedTemplateKind,
            IMethod overriddenMethod )
        {
            var runtimeAspectHelperType =
                generationContext.SyntaxGenerator.Type( generationContext.ReflectionMapper.GetTypeSymbol( typeof(RunTimeAspectHelper) ) );

            switch ( selectedTemplateKind )
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

                        return new BuiltUserExpression( expression, overriddenMethod.ReturnType );
                    }

                case TemplateKind.Default when overriddenMethod.GetAsyncInfoImpl() is { IsAsync: true, IsAwaitableOrVoid: true } asyncInfo:
                    {
                        // The target method is an async method (but not an async iterator).
                        // Generate: `( await BASE(ARGS) )`.

                        var taskResultType = asyncInfo.ResultType;

                        return new BuiltUserExpression(
                            SyntaxFactory.ParenthesizedExpression( SyntaxFactory.AwaitExpression( invocationExpression ) )
                                .WithAdditionalAnnotations( Simplifier.Annotation ),
                            taskResultType );
                    }

                case TemplateKind.Async when overriddenMethod.GetIteratorInfoImpl() is
                    { EnumerableKind: EnumerableKind.IAsyncEnumerable or EnumerableKind.IAsyncEnumerator }:
                    {
                        var expression = GenerateAwaitBufferAsync();

                        return new BuiltUserExpression( expression, overriddenMethod.ReturnType );
                    }
            }

            // This is a default method, or a non-default template.
            // Generate: `BASE(ARGS)`
            return new BuiltUserExpression(
                invocationExpression,
                overriddenMethod.ReturnType );

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