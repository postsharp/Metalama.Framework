// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
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

        private new IFinalizer OverriddenDeclaration => (IFinalizer) base.OverriddenDeclaration;

        public OverrideFinalizerTransformation( Advice advice, IFinalizer targetFinalizer, BoundTemplateMethod boundTemplate, IObjectReader tags )
            : base( advice, targetFinalizer, tags )
        {
            Invariant.Assert( !boundTemplate.IsNull );

            this.BoundTemplate = boundTemplate;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var proceedExpression = this.CreateProceedExpression( context, this.BoundTemplate.Template.SelectedKind );

            var metaApi = MetaApi.ForFinalizer(
                this.OverriddenDeclaration,
                new MetaApiProperties(
                    context.DiagnosticSink,
                    this.BoundTemplate.Template.Cast(),
                    this.Tags,
                    this.Advice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.Advice.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.Default ) );

            var expansionContext = new TemplateExpansionContext(
                this.Advice.TemplateInstance.Instance,
                metaApi,
                (CompilationModel) this.OverriddenDeclaration.Compilation,
                context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                this.BoundTemplate,
                proceedExpression,
                this.Advice.AspectLayerId );

            var templateDriver = this.Advice.TemplateInstance.TemplateClass.GetTemplateDriver( this.BoundTemplate.Template.Declaration! );

            if ( !templateDriver.TryExpandDeclaration( expansionContext, this.BoundTemplate.TemplateArguments, out var newMethodBody ) )
            {
                // Template expansion error.
                return Enumerable.Empty<IntroducedMember>();
            }

            var syntax =
                MethodDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList(),
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    null,
                    Identifier( context.IntroductionNameProvider.GetOverrideName( this.OverriddenDeclaration.DeclaringType, this.Advice.AspectLayerId, this.OverriddenDeclaration ) ),
                    null,
                    ParameterList(),
                    List<TypeParameterConstraintClauseSyntax>(),
                    newMethodBody,
                    null );

            return new[]
            {
                new IntroducedMember(this, syntax, this.Advice.AspectLayerId, IntroducedMemberSemantic.Override, this.OverriddenDeclaration)
            };
        }

        private BuiltUserExpression CreateProceedExpression( in MemberIntroductionContext context, TemplateKind templateKind )
        {
            return new BuiltUserExpression(
                context.AspectReferenceSyntaxProvider.GetFinalizerReference( this.Advice.AspectLayerId, this.OverriddenDeclaration, context.SyntaxGenerator ),
                this.OverriddenDeclaration.GetCompilationModel().Factory.GetSpecialType( SpecialType.Void ) );
        }
    }
}