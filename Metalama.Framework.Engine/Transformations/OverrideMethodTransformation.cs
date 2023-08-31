// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Method override, which expands a template.
    /// </summary>
    internal sealed class OverrideMethodTransformation : OverrideMethodBaseTransformation
    {
        private BoundTemplateMethod BoundTemplate { get; }

        public OverrideMethodTransformation( Advice advice, IMethod targetMethod, BoundTemplateMethod boundTemplate, IObjectReader tags )
            : base( advice, targetMethod, tags )
        {
            this.BoundTemplate = boundTemplate;
        }

        public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
        {
            var proceedExpressionProvider = ( TemplateKind kind ) => this.CreateProceedExpression( context, kind );

            var metaApi = MetaApi.ForMethod(
                this.OverriddenDeclaration,
                new MetaApiProperties(
                    context.Compilation,
                    context.DiagnosticSink,
                    this.BoundTemplate.TemplateMember.Cast(),
                    this.Tags,
                    this.ParentAdvice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.ParentAdvice.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.Default ) );

            var expansionContext = new TemplateExpansionContext(
                context,
                this.ParentAdvice.TemplateInstance.TemplateProvider,
                metaApi,
                this.OverriddenDeclaration,
                this.BoundTemplate,
                proceedExpressionProvider,
                this.ParentAdvice.AspectLayerId );

            var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( this.BoundTemplate.TemplateMember.Declaration );

            if ( !templateDriver.TryExpandDeclaration( expansionContext, this.BoundTemplate.TemplateArguments, out var newMethodBody ) )
            {
                // Template expansion error.
                return Enumerable.Empty<InjectedMember>();
            }

            return this.GetInjectedMembersImpl( context, newMethodBody, this.BoundTemplate.TemplateMember.MustInterpretAsAsyncTemplate() );
        }
    }
}