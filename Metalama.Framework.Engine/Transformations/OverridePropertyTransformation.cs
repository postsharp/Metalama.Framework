// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations
{
    internal sealed class OverridePropertyTransformation : OverridePropertyBaseTransformation
    {
        private BoundTemplateMethod? GetTemplate { get; }

        private BoundTemplateMethod? SetTemplate { get; }

        public OverridePropertyTransformation(
            Advice advice,
            IProperty overriddenDeclaration,
            BoundTemplateMethod? getTemplate,
            BoundTemplateMethod? setTemplate,
            IObjectReader tags )
            : base( advice, overriddenDeclaration, tags )
        {
            // We need the getTemplate and setTemplate to be set by the caller even if propertyTemplate is set.
            // The caller is responsible for verifying the compatibility of the template with the target.

            this.GetTemplate = getTemplate;
            this.SetTemplate = setTemplate;
        }

        public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
        {
            var getTemplate = this.GetTemplate;
            var setTemplate = this.SetTemplate;

            var templateExpansionError = false;
            BlockSyntax? getAccessorBody = null;

            if ( this.OverriddenDeclaration.GetMethod != null )
            {
                if ( getTemplate != null )
                {
                    templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                        context,
                        getTemplate,
                        this.OverriddenDeclaration.GetMethod,
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

            if ( this.OverriddenDeclaration.SetMethod != null )
            {
                if ( setTemplate != null )
                {
                    templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                        context,
                        setTemplate,
                        this.OverriddenDeclaration.SetMethod,
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
                return Enumerable.Empty<InjectedMember>();
            }

            return this.GetInjectedMembersImpl( context, getAccessorBody, setAccessorBody );
        }

        private bool TryExpandAccessorTemplate(
            MemberInjectionContext context,
            BoundTemplateMethod accessorTemplate,
            IMethod accessor,
            [NotNullWhen( true )] out BlockSyntax? body )
        {
            var proceedExpressionProvider = ( TemplateKind kind ) => this.CreateProceedDynamicExpression( context, accessor, kind );

            var metaApi = MetaApi.ForFieldOrPropertyOrIndexer(
                this.OverriddenDeclaration,
                accessor,
                new MetaApiProperties(
                    this.ParentAdvice.SourceCompilation,
                    context.DiagnosticSink,
                    accessorTemplate.TemplateMember.Cast(),
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
                accessor,
                accessorTemplate,
                proceedExpressionProvider,
                this.ParentAdvice.AspectLayerId );

            var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( accessorTemplate.TemplateMember.Declaration );

            return templateDriver.TryExpandDeclaration( expansionContext, accessorTemplate.TemplateArguments, out body );
        }
    }
}