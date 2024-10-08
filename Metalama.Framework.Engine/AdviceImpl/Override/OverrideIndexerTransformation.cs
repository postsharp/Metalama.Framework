// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal sealed class OverrideIndexerTransformation : OverrideIndexerBaseTransformation
{
    private BoundTemplateMethod? GetTemplate { get; }

    private BoundTemplateMethod? SetTemplate { get; }

    public OverrideIndexerTransformation(
        Advice advice,
        IFullRef<IIndexer> overriddenDeclaration,
        BoundTemplateMethod? getTemplate,
        BoundTemplateMethod? setTemplate,
        IObjectReader tags )
        : base( advice, overriddenDeclaration, tags )
    {
        this.GetTemplate = getTemplate;
        this.SetTemplate = setTemplate;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var templateExpansionError = false;
        BlockSyntax? getAccessorBody = null;

        var overriddenDeclaration = this.OverriddenPropertyOrIndexer.GetTarget( context.Compilation );

        if ( overriddenDeclaration.GetMethod != null )
        {
            if ( this.GetTemplate != null )
            {
                templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                    context,
                    this.GetTemplate,
                    overriddenDeclaration.GetMethod,
                    out getAccessorBody );
            }
            else
            {
                getAccessorBody = this.CreateIdentityAccessorBody( context, SyntaxKind.GetAccessorDeclaration );
            }
        }
        else
        {
            getAccessorBody = null;
        }

        BlockSyntax? setAccessorBody = null;

        if ( overriddenDeclaration.SetMethod != null )
        {
            if ( this.SetTemplate != null )
            {
                templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                    context,
                    this.SetTemplate,
                    overriddenDeclaration.SetMethod,
                    out setAccessorBody );
            }
            else
            {
                setAccessorBody = this.CreateIdentityAccessorBody( context, SyntaxKind.SetAccessorDeclaration );
            }
        }
        else
        {
            setAccessorBody = null;
        }

        if ( templateExpansionError )
        {
            // Template expansion error.
            return [];
        }

        return this.GetInjectedMembersImpl( context, getAccessorBody, setAccessorBody );
    }

    private bool TryExpandAccessorTemplate(
        MemberInjectionContext context,
        BoundTemplateMethod accessorTemplate,
        IMethod accessor,
        [NotNullWhen( true )] out BlockSyntax? body )
    {
        var overriddenDeclaration = (IIndexer) this.OverriddenDeclaration.GetTarget( context.Compilation );

        SyntaxUserExpression ProceedExpressionProvider( TemplateKind kind )
        {
            return this.CreateProceedDynamicExpression( context, accessor, kind, overriddenDeclaration );
        }

        var metaApi = MetaApi.ForFieldOrPropertyOrIndexer(
            overriddenDeclaration,
            accessor,
            new MetaApiProperties(
                this.OriginalCompilation,
                context.DiagnosticSink,
                accessorTemplate.TemplateMember.AsMemberOrNamedType(),
                this.Tags,
                this.AspectLayerId,
                context.SyntaxGenerationContext,
                this.AspectInstance,
                context.ServiceProvider,
                MetaApiStaticity.Default ) );

        var expansionContext = new TemplateExpansionContext(
            context,
            metaApi,
            accessor,
            accessorTemplate,
            ProceedExpressionProvider,
            this.AspectLayerId );

        var templateDriver = accessorTemplate.TemplateMember.Driver;

        return templateDriver.TryExpandDeclaration( expansionContext, accessorTemplate.TemplateArguments, out body );
    }
}