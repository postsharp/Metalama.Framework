// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Method override, which expands a template.
    /// </summary>
    internal sealed class OverriddenMethod : OverriddenMember
    {
        public new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        public TemplateMember<IMethod> Template { get; }

        public OverriddenMethod( Advice advice, IMethod overriddenDeclaration, TemplateMember<IMethod> template )
            : base( advice, overriddenDeclaration )
        {
            Invariant.Assert( template.IsNotNull );

            this.Template = template;
            this.Template.ValidateTarget( overriddenDeclaration );
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            using ( context.DiagnosticSink.WithDefaultScope( this.OverriddenDeclaration ) )
            {
                var proceedExpression = ProceedHelper.CreateProceedDynamicExpression(
                    context.SyntaxGenerationContext,
                    this.CreateInvocationExpression( context.SyntaxGenerationContext ),
                    this.Template,
                    this.OverriddenDeclaration );

                var metaApi = MetaApi.ForMethod(
                    this.OverriddenDeclaration,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        this.Template.Cast(),
                        this.Advice.ReadOnlyTags,
                        this.Advice.AspectLayerId,
                        context.SyntaxGenerationContext,
                        this.Advice.Aspect,
                        context.ServiceProvider ) );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.TemplateInstance.Instance,
                    metaApi,
                    this.OverriddenDeclaration.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ),
                    context.ServiceProvider.GetService<SyntaxSerializationService>(),
                    context.SyntaxGenerationContext,
                    this.Template,
                    proceedExpression );

                var templateDriver = this.Advice.TemplateInstance.TemplateClass.GetTemplateDriver( this.Template.Declaration! );

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
                        returnType = context.SyntaxGenerator.Type(
                            this.OverriddenDeclaration.GetCompilationModel().Factory.GetSpecialType( SpecialType.ValueTask ).GetSymbol() );
                    }
                }

                returnType ??= context.SyntaxGenerator.Type( this.OverriddenDeclaration.ReturnType.GetSymbol() );

                var introducedMethod = MethodDeclaration(
                    List<AttributeListSyntax>(),
                    modifiers,
                    returnType,
                    null,
                    Identifier(
                        context.IntroductionNameProvider.GetOverrideName(
                            this.OverriddenDeclaration.DeclaringType,
                            this.Advice.AspectLayerId,
                            this.OverriddenDeclaration ) ),
                    context.SyntaxGenerator.TypeParameterList( this.OverriddenDeclaration ),
                    context.SyntaxGenerator.ParameterList( this.OverriddenDeclaration ),
                    context.SyntaxGenerator.ConstraintClauses( this.OverriddenDeclaration ),
                    newMethodBody,
                    null );

                var overrides = new[]
                {
                    new IntroducedMember(
                        this,
                        introducedMethod,
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.Override,
                        this.OverriddenDeclaration )
                };

                return overrides;
            }
        }

        private ExpressionSyntax CreateInvocationExpression( SyntaxGenerationContext generationContext )
            => InvocationExpression(
                this.CreateMemberAccessExpression( AspectReferenceTargetKind.Self, generationContext ),
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