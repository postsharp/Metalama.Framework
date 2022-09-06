// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Finalizer override, which expands a template.
    /// </summary>
    internal sealed class OverrideFinalizerTransformation : OverrideMemberTransformation
    {
        public BoundTemplateMethod BoundTemplate { get; }

        private new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        public OverrideFinalizerTransformation( Advice advice, IMethod targetFinalizer, BoundTemplateMethod boundTemplate, IObjectReader tags )
            : base( advice, targetFinalizer, tags )
        {
            this.BoundTemplate = boundTemplate;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( MemberIntroductionContext context )
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
                    MetaApiStaticity.Default ) );

            var expansionContext = new TemplateExpansionContext(
                this.ParentAdvice.TemplateInstance.Instance,
                metaApi,
                context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                this.BoundTemplate.Template,
                proceedExpression,
                this.ParentAdvice.AspectLayerId );

            var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( this.BoundTemplate.Template.Declaration );

            if ( !templateDriver.TryExpandDeclaration( expansionContext, this.BoundTemplate.TemplateArguments, out var newMethodBody ) )
            {
                // Template expansion error.
                return Enumerable.Empty<IntroducedMember>();
            }

            var syntax =
                MethodDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList(),
                    PredefinedType( Token( SyntaxKind.VoidKeyword ) ),
                    null,
                    Identifier(
                        context.IntroductionNameProvider.GetOverrideName(
                            this.OverriddenDeclaration.DeclaringType,
                            this.ParentAdvice.AspectLayerId,
                            this.OverriddenDeclaration ) ),
                    null,
                    ParameterList(),
                    List<TypeParameterConstraintClauseSyntax>(),
                    newMethodBody,
                    null );

            return new[]
            {
                new IntroducedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Override, this.OverriddenDeclaration )
            };
        }

        private BuiltUserExpression CreateProceedExpression( in MemberIntroductionContext context )
        {
            return new BuiltUserExpression(
                context.AspectReferenceSyntaxProvider.GetFinalizerReference(
                    this.ParentAdvice.AspectLayerId,
                    this.OverriddenDeclaration,
                    context.SyntaxGenerator ),
                this.OverriddenDeclaration.GetCompilationModel().Factory.GetSpecialType( SpecialType.Void ) );
        }
    }
}