// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

/// <summary>
/// Method override, which expands a template.
/// </summary>
internal sealed class OverrideMethodTransformation : OverrideMethodBaseTransformation
{
    private BoundTemplateMethod BoundTemplate { get; }

    public OverrideMethodTransformation( Advice advice, IRef<IMethod> targetMethod, BoundTemplateMethod boundTemplate, IObjectReader tags )
        : base( advice, targetMethod, tags )
    {
        this.BoundTemplate = boundTemplate;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        SyntaxUserExpression ProceedExpressionProvider( TemplateKind kind )
        {
            return this.CreateProceedExpression( context, kind );
        }

        var overriddenDeclaration = this.OverriddenDeclaration.GetTarget(context.Compilation);

        var metaApi = MetaApi.ForMethod(
            overriddenDeclaration,
            new MetaApiProperties(
                this.OriginalCompilation,
                context.DiagnosticSink,
                this.BoundTemplate.TemplateMember.Cast(),
                this.Tags,
                this.AspectLayerId,
                context.SyntaxGenerationContext,
                this.AspectInstance,
                context.ServiceProvider,
                MetaApiStaticity.Default ) );

        var expansionContext = new TemplateExpansionContext(
            context,
            metaApi,
            overriddenDeclaration,
            this.BoundTemplate,
            ProceedExpressionProvider,
            this.AspectLayerId );

        var templateDriver = this.BoundTemplate.TemplateMember.Driver;

        if ( !templateDriver.TryExpandDeclaration( expansionContext, this.BoundTemplate.TemplateArguments, out var newMethodBody ) )
        {
            // Template expansion error.
            return [];
        }

        return this.GetInjectedMembersImpl( context, newMethodBody, this.BoundTemplate.TemplateMember.MustInterpretAsAsyncTemplate() );
    }
}