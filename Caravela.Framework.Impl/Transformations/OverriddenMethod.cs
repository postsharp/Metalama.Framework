// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using RefKind = Caravela.Framework.Code.RefKind;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Method override, which expands a template.
    /// </summary>
    internal sealed class OverriddenMethod : OverriddenMember
    {
        public new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        public Template<IMethod> Template { get; }

        public OverriddenMethod( Advice advice, IMethod overriddenDeclaration, Template<IMethod> template )
            : base( advice, overriddenDeclaration )
        {
            Invariant.Assert( template.IsNotNull );

            this.Template = template;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            using ( context.DiagnosticSink.WithDefaultScope( this.OverriddenDeclaration ) )
            {
                var proceedExpression = ProceedHelper.CreateProceedDynamicExpression( this.CreateInvocationExpression(), this.Template, this.OverriddenDeclaration );
                
                var expandYieldProceed = this.CreateYieldProceedStatement( proceedExpression );

                var metaApi = MetaApi.ForMethod(
                    this.OverriddenDeclaration,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        this.Template.Cast(),
                        this.Advice.ReadOnlyTags,
                        this.Advice.AspectLayerId,
                        context.ServiceProvider ) );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.Aspect.Aspect,
                    metaApi,
                    this.OverriddenDeclaration.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ),
                    context.ServiceProvider.GetService<SyntaxSerializationService>(),
                    (ICompilationElementFactory) this.OverriddenDeclaration.Compilation.TypeFactory,
                    this.Template,
                    proceedExpression,
                    expandYieldProceed );

                var templateDriver = this.Advice.Aspect.AspectClass.GetTemplateDriver( this.Template.Declaration! );

                if ( !templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out var newMethodBody ) )
                {
                    // Template expansion error.
                    return Enumerable.Empty<IntroducedMember>();
                }

                TypeSyntax? returnType = null;

                var modifiers = this.OverriddenDeclaration.GetSyntaxModifierList();

                if ( !this.OverriddenDeclaration.IsAsync )
                {
                    if ( this.Template.MustInterpretAsAsync() )
                    {
                        // If the template is async but the overridden declaration is not, we have to add an async modifier.

                        modifiers = modifiers.Add( Token( SyntaxKind.AsyncKeyword ) );

                        /*
                        // The return type needs to be changed from void to ValueTask.
                        if ( this.OverriddenDeclaration.ReturnType.SpecialType == SpecialType.Void )
                            
                        var taskType = this.OverriddenDeclaration.ReturnType.SpecialType == SpecialType.Void
                            ? this.OverriddenDeclaration.GetCompilationModel().Factory.GetSpecialType( SpecialType.ValueTask )
                            : this.OverriddenDeclaration.GetCompilationModel()
                                .Factory.GetSpecialType( SpecialType.ValueTask_T )
                                .WithGenericArguments( this.OverriddenDeclaration.ReturnType );

                        returnType = LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( taskType.GetSymbol() );
                        */
                    }
                }
                else
                {
                    if ( !this.Template.MustInterpretAsAsync() )
                    {
                        // If the template is not async but the overridden declaration is, we have to remove the async modifier.
                        modifiers = TokenList( modifiers.Where( m => m.Kind() != SyntaxKind.AsyncKeyword ) );
                    }

                    // If the template is async and the target declaration is `async void`, and regardless of the async flag the template, we have to change the type to ValueTask, otherwise
                    // it is not awaitable

                    if ( TypeExtensions.Equals( this.OverriddenDeclaration.ReturnType, SpecialType.Void ) )
                    {
                        returnType = LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression(
                            this.OverriddenDeclaration.GetCompilationModel().Factory.GetSpecialType( SpecialType.ValueTask ).GetSymbol() );
                    }
                }

                returnType ??= LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( this.OverriddenDeclaration.ReturnType.GetSymbol() );

                var overrides = new[]
                {
                    // TODO: async, change type
                    new IntroducedMember(
                        this,
                        MethodDeclaration(
                            List<AttributeListSyntax>(),
                            modifiers,
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

        private Func<TemplateExpansionContext, StatementSyntax>? CreateYieldProceedStatement( IDynamicExpression proceedExpression )
        {
            switch ( this.Template.SelectedKind )
            {
                case TemplateKind.IEnumerable:
                case TemplateKind.IEnumerator:
                    // Generate: `foreach ( var value in PROCEED() ) {   yield return value; }`
                    return context =>
                    {
                        var varName = context.LexicalScope.GetUniqueIdentifier( "value" );

                        return ForEachStatement(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    SyntaxKind.VarKeyword,
                                    "var",
                                    "var",
                                    TriviaList() ) ),
                            Identifier( varName ),
                            proceedExpression.CreateExpression(),
                            Block(
                                SingletonList<StatementSyntax>(
                                    YieldStatement(
                                        SyntaxKind.YieldReturnStatement,
                                        IdentifierName( varName ) ) ) ) );
                    };

                case TemplateKind.IAsyncEnumerable:
                case TemplateKind.IAsyncEnumerator:
                    // Generate: `await foreach ( var value in PROCEED() ) {   yield return value; }`
                    return context =>
                    {
                        // TODO: Not sure how the CancellationToken is handled.

                        var varName = context.LexicalScope.GetUniqueIdentifier( "value" );

                        return ForEachStatement(
                                IdentifierName(
                                    Identifier(
                                        TriviaList(),
                                        SyntaxKind.VarKeyword,
                                        "var",
                                        "var",
                                        TriviaList() ) ),
                                Identifier( varName ),
                                proceedExpression.CreateExpression(),
                                Block(
                                    SingletonList<StatementSyntax>(
                                        YieldStatement(
                                            SyntaxKind.YieldReturnStatement,
                                            IdentifierName( varName ) ) ) ) )
                            .WithAwaitKeyword( Token( SyntaxKind.AwaitKeyword ) );
                    };

                default:
                    // No special
                    return null;
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