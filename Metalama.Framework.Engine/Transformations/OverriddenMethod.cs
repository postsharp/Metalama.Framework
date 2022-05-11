// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Project;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations
{

    /// <summary>
    /// Method override, which expands a template.
    /// </summary>
    internal sealed class OverriddenMethod : OverriddenMethodBase
    {
        
        public BoundTemplateMethod BoundTemplate { get; }

        public OverriddenMethod( Advice advice, IMethod targetMethod, BoundTemplateMethod boundTemplate, IObjectReader tags )
            : base( advice, targetMethod, tags )
        {
            Invariant.Assert( !boundTemplate.IsNull );

            this.BoundTemplate = boundTemplate;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var proceedExpression = this.CreateProceedExpression( context, this.BoundTemplate.Template.SelectedKind );

            var metaApi = MetaApi.ForMethod(
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

            return this.GetIntroducedMembersImpl( context, newMethodBody, this.BoundTemplate.Template.MustInterpretAsAsyncTemplate() );
            
        }

     
    }
}