// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Method override, which expands a template.
    /// </summary>
    internal sealed class OverrideOperatorTransformation : OverrideMemberTransformation
    {
        private BoundTemplateMethod BoundTemplate { get; }

        private new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        public OverrideOperatorTransformation( Advice advice, IMethod targetOperator, BoundTemplateMethod boundTemplate, IObjectReader tags )
            : base( advice, targetOperator, tags )
        {
            this.BoundTemplate = boundTemplate;
        }

        public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
        {
            var proceedExpression = this.CreateProceedExpression( context );

            var metaApi = MetaApi.ForMethod(
                this.OverriddenDeclaration,
                new MetaApiProperties(
                    this.ParentAdvice.SourceCompilation,
                    context.DiagnosticSink,
                    this.BoundTemplate.Template.Cast(),
                    this.Tags,
                    this.ParentAdvice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.ParentAdvice.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.AlwaysStatic ) );

            var expansionContext = new TemplateExpansionContext(
                context.ServiceProvider,
                this.ParentAdvice.TemplateInstance.Instance,
                metaApi,
                context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                this.BoundTemplate,
                proceedExpression,
                this.ParentAdvice.AspectLayerId );

            var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( this.BoundTemplate.Template.Declaration );

            if ( !templateDriver.TryExpandDeclaration(
                    expansionContext,
                    this.BoundTemplate.GetTemplateArgumentsForMethod( this.OverriddenDeclaration ),
                    out var newMethodBody ) )
            {
                // Template expansion error.
                return Enumerable.Empty<InjectedMember>();
            }

            var syntax =
                MethodDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ), Token( SyntaxKind.StaticKeyword ).WithTrailingTrivia( Space ) ),
                    context.SyntaxGenerator.ReturnType( this.OverriddenDeclaration ).WithTrailingTrivia( Space ),
                    null,
                    Identifier(
                        context.InjectionNameProvider.GetOverrideName(
                            this.OverriddenDeclaration.DeclaringType,
                            this.ParentAdvice.AspectLayerId,
                            this.OverriddenDeclaration ) ),
                    null,
                    context.SyntaxGenerator.ParameterList( this.OverriddenDeclaration, context.Compilation, removeDefaultValues: true ),
                    List<TypeParameterConstraintClauseSyntax>(),
                    newMethodBody,
                    null );

            return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Override, this.OverriddenDeclaration ) };
        }

        private SyntaxUserExpression CreateProceedExpression( in MemberInjectionContext context )
        {
            return new SyntaxUserExpression(
                context.AspectReferenceSyntaxProvider.GetOperatorReference(
                    this.ParentAdvice.AspectLayerId,
                    this.OverriddenDeclaration,
                    context.SyntaxGenerator ),
                this.OverriddenDeclaration.ReturnType );
        }
    }
}