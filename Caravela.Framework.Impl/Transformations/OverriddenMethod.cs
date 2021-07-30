// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Method override, which expands a template.
    /// </summary>
    internal sealed class OverriddenMethod : OverriddenMember
    {
        public new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        public IMethod TemplateMethod { get; }

        public OverriddenMethod( Advice advice, IMethod overriddenDeclaration, IMethod templateMethod )
            : base( advice, overriddenDeclaration )
        {
            Invariant.Assert( templateMethod != null );

            this.TemplateMethod = templateMethod;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            using ( context.DiagnosticSink.WithDefaultScope( this.OverriddenDeclaration ) )
            {
                var proceedExpression = this.CreateProceedExpression();

                var metaApi = MetaApi.ForMethod(
                    this.OverriddenDeclaration,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        this.TemplateMethod.GetSymbol().AssertNotNull( Justifications.TemplateMembersHaveSymbol ),
                        this.Advice.ReadOnlyTags,
                        this.Advice.AspectLayerId,
                        proceedExpression,
                        context.ServiceProvider ) );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.Aspect.Aspect,
                    metaApi,
                    this.OverriddenDeclaration.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ),
                    context.ServiceProvider.GetService<SyntaxSerializationService>(),
                    (ICompilationElementFactory) this.OverriddenDeclaration.Compilation.TypeFactory );

                var templateDriver = this.Advice.Aspect.AspectClass.GetTemplateDriver( this.TemplateMethod );

                if ( !templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out var newMethodBody ) )
                {
                    // Template expansion error.
                    return Enumerable.Empty<IntroducedMember>();
                }

                var returnType = AsyncHelper.GetIntermediateMethodReturnType( this.OverriddenDeclaration );

                var overrides = new[]
                {
                    new IntroducedMember(
                        this,
                        MethodDeclaration(
                            List<AttributeListSyntax>(),
                            this.OverriddenDeclaration.GetSyntaxModifierList(),
                            returnType,
                            null,
                            Identifier(
                                context.IntroductionNameProvider.GetOverrideName(
                                    this.OverriddenDeclaration.DeclaringType,
                                    this.Advice.AspectLayerId,
                                    this.OverriddenDeclaration ) ),
                            SyntaxHelpers.CreateSyntaxForTypeParameterList( this.OverriddenDeclaration ),
                            SyntaxHelpers.CreateSyntaxForParameterList( this.OverriddenDeclaration ),
                            SyntaxHelpers.CreateSyntaxForConstraintClauses( this.OverriddenDeclaration ),
                            newMethodBody,
                            null ),
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.Override,
                        this.OverriddenDeclaration )
                };

                return overrides;
            }
        }

        private DynamicExpression CreateProceedExpression()
        {
            var invocationExpression = this.CreateInvocationExpression();

            if ( this.OverriddenDeclaration.GetIteratorInfoImpl() is { IsIterator: true } iteratorInfo )
            {
                ExpressionSyntax expression;

                if ( !iteratorInfo.IsAsync )
                {
                    expression =
                        InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    this.OverriddenDeclaration.GetSyntaxFactory().GetTypeSyntax( typeof(RunTimeAspectHelper) ),
                                    IdentifierName( nameof(RunTimeAspectHelper.Buffer) ) ) )
                            .WithArgumentList( ArgumentList( SingletonSeparatedList( Argument( invocationExpression ) ) ) )
                            .WithAdditionalAnnotations( Simplifier.Annotation );
                }
                else
                {
                    var arguments = ArgumentList( SingletonSeparatedList( Argument( invocationExpression ) ) );

                    var cancellationTokenParameter = this.OverriddenDeclaration.Parameters
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
                                    this.OverriddenDeclaration.GetSyntaxFactory().GetTypeSyntax( typeof(RunTimeAspectHelper) ),
                                    IdentifierName( nameof(RunTimeAspectHelper.Buffer) + "Async" ) ) )
                            .WithArgumentList( arguments )
                            .WithAdditionalAnnotations( Simplifier.Annotation );

                    expression =
                        ParenthesizedExpression( AwaitExpression( bufferExpression ) ).WithAdditionalAnnotations( Simplifier.Annotation );
                }

                // We have an iterator method. The meta.Proceed() translates into a `new List( METHOD() )`.
                return new DynamicExpression(
                    expression,
                    this.OverriddenDeclaration.ReturnType,
                    false );
            }
            else if ( this.OverriddenDeclaration.GetAsyncInfoImpl() is { IsAsync: true, IsAwaitableOrVoid: true } asyncInfo )
            {
                // We have an async method. The meta.Proceed() translates into an '(await METHOD())' expression.
                var taskResultType = asyncInfo.ResultType;

                return new DynamicExpression(
                    ParenthesizedExpression( AwaitExpression( invocationExpression ) ).WithAdditionalAnnotations( Simplifier.Annotation ),
                    taskResultType,
                    false );
            }
            else
            {
                return new DynamicExpression(
                    invocationExpression,
                    this.OverriddenDeclaration.ReturnType,
                    false );
            }
        }

        private ExpressionSyntax CreateInvocationExpression()
        {
            return
                InvocationExpression(
                    this.CreateMemberAccessExpression( AspectReferenceTargetKind.Self ),
                    ArgumentList(
                        SeparatedList(
                            this.OverriddenDeclaration.Parameters.Select(
                                p =>
                                {
                                    var refKind = p.RefKind switch
                                    {
                                        RefKind.None => default,
                                        RefKind.In => default,
                                        RefKind.Out => Token( SyntaxKind.OutKeyword ),
                                        RefKind.Ref => Token( SyntaxKind.RefKeyword ),
                                        _ => throw new AssertionFailedException()
                                    };

                                    return Argument( null, refKind, IdentifierName( p.Name ) );
                                } ) ) ) );
        }
    }
}