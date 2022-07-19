// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations
{
    internal sealed class OverridePropertyTransformation : OverridePropertyBaseTransformation
    {
        public BoundTemplateMethod? GetTemplate { get; }

        public BoundTemplateMethod? SetTemplate { get; }

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

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
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
                    getAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.GetAccessorDeclaration, context.SyntaxGenerationContext );
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
                    setAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.SetAccessorDeclaration, context.SyntaxGenerationContext );
                }
            }
            else
            {
                setAccessorBody = null;
            }

            if ( templateExpansionError )
            {
                // Template expansion error.
                return Enumerable.Empty<IntroducedMember>();
            }

            return this.GetIntroducedMembersImpl( context, getAccessorBody, setAccessorBody );
        }

        private bool TryExpandAccessorTemplate(
            in MemberIntroductionContext context,
            BoundTemplateMethod accessorTemplate,
            IMethod accessor,
            [NotNullWhen( true )] out BlockSyntax? body )
        {
            var proceedExpression =
                this.CreateProceedDynamicExpression( context, accessor, accessorTemplate.Template.SelectedKind );

            var metaApi = MetaApi.ForFieldOrProperty(
                this.OverriddenDeclaration,
                accessor,
                new MetaApiProperties(
                    this.ParentAdvice.SourceCompilation,
                    context.DiagnosticSink,
                    accessorTemplate.Template.Cast(),
                    this.Tags,
                    this.ParentAdvice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.ParentAdvice.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.Default ) );

            var expansionContext = new TemplateExpansionContext(
                this.ParentAdvice.TemplateInstance.Instance,
                metaApi,
                context.LexicalScopeProvider.GetLexicalScope( accessor ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                accessorTemplate.Template,
                proceedExpression,
                this.ParentAdvice.AspectLayerId );

            var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( accessorTemplate.Template.Declaration );

            return templateDriver.TryExpandDeclaration( expansionContext, accessorTemplate.TemplateArguments, out body );
        }
    }
}