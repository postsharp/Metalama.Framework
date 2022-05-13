// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    internal sealed class OverriddenProperty : OverriddenMember
    {
        public new IProperty OverriddenDeclaration => (IProperty) base.OverriddenDeclaration;

        public TemplateMember<IProperty> PropertyTemplate { get; }

        public BoundTemplateMethod GetTemplate { get; }

        public BoundTemplateMethod SetTemplate { get; }

        public OverriddenProperty(
            Advice advice,
            IProperty overriddenDeclaration,
            TemplateMember<IProperty> propertyTemplate,
            BoundTemplateMethod getTemplate,
            BoundTemplateMethod setTemplate,
            IObjectReader tags )
            : base( advice, overriddenDeclaration, tags )
        {
            // We need the getTemplate and setTemplate to be set by the caller even if propertyTemplate is set.
            // The caller is responsible for verifying the compatibility of the template with the target.
            Invariant.Assert( !(propertyTemplate.Declaration != null && propertyTemplate.Declaration.IsAutoPropertyOrField) );

            this.PropertyTemplate = propertyTemplate;
            this.GetTemplate = getTemplate;
            this.SetTemplate = setTemplate;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var propertyName = context.IntroductionNameProvider.GetOverrideName(
                this.OverriddenDeclaration.DeclaringType,
                this.Advice.AspectLayerId,
                this.OverriddenDeclaration );

            var getTemplate = this.GetTemplate;
            var setTemplate = this.SetTemplate;

            var setAccessorDeclarationKind = this.OverriddenDeclaration.Writeability == Writeability.InitOnly
                ? SyntaxKind.InitAccessorDeclaration
                : SyntaxKind.SetAccessorDeclaration;

            var templateExpansionError = false;
            BlockSyntax? getAccessorBody = null;

            if ( this.OverriddenDeclaration.GetMethod != null )
            {
                if ( getTemplate.IsNotNull )
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
                if ( setTemplate.IsNotNull )
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

            var overrides = new[]
            {
                new IntroducedMember(
                    this,
                    PropertyDeclaration(
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.GetSyntaxModifierList(),
                        context.SyntaxGenerator.PropertyType( this.OverriddenDeclaration ),
                        null,
                        Identifier( propertyName ),
                        AccessorList(
                            List(
                                new[]
                                    {
                                        getAccessorBody != null
                                            ? AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration,
                                                List<AttributeListSyntax>(),
                                                this.OverriddenDeclaration.GetMethod.AssertNotNull().GetSyntaxModifierList(),
                                                getAccessorBody )
                                            : null,
                                        setAccessorBody != null
                                            ? AccessorDeclaration(
                                                setAccessorDeclarationKind,
                                                List<AttributeListSyntax>(),
                                                this.OverriddenDeclaration.SetMethod.AssertNotNull().GetSyntaxModifierList(),
                                                setAccessorBody )
                                            : null
                                    }.Where( a => a != null )
                                    .AssertNoneNull() ) ),
                        null,
                        null ),
                    this.Advice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };

            return overrides;
        }

        private bool TryExpandAccessorTemplate(
            in MemberIntroductionContext context,
            BoundTemplateMethod accessorTemplate,
            IMethod accessor,
            [NotNullWhen( true )] out BlockSyntax? body )
        {
            var proceedExpression =
                accessor.MethodKind switch
                {
                    MethodKind.PropertyGet => ProceedHelper.CreateProceedDynamicExpression(
                        context.SyntaxGenerationContext,
                        this.CreateProceedGetExpression( context.SyntaxGenerationContext ),
                        this.GetTemplate,
                        this.OverriddenDeclaration.GetMethod.AssertNotNull() ),
                    MethodKind.PropertySet => new UserExpression(
                        this.CreateProceedSetExpression( context.SyntaxGenerationContext ),
                        this.OverriddenDeclaration.Compilation.GetCompilationModel().Factory.GetSpecialType( SpecialType.Void ),
                        context.SyntaxGenerationContext ),
                    _ => throw new AssertionFailedException()
                };

            var metaApi = MetaApi.ForFieldOrProperty(
                this.OverriddenDeclaration,
                accessor,
                new MetaApiProperties(
                    context.DiagnosticSink,
                    accessorTemplate.Template.Cast(),
                    this.Tags,
                    this.Advice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.Advice.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.Default ) );

            var expansionContext = new TemplateExpansionContext(
                this.Advice.Aspect.Aspect,
                metaApi,
                (CompilationModel) this.OverriddenDeclaration.Compilation,
                context.LexicalScopeProvider.GetLexicalScope( accessor ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                default,
                proceedExpression,
                this.Advice.AspectLayerId );

            var templateDriver = this.Advice.TemplateInstance.TemplateClass.GetTemplateDriver( accessorTemplate.Template.Declaration! );

            return templateDriver.TryExpandDeclaration( expansionContext, accessorTemplate.TemplateArguments, out body );
        }

        /// <summary>
        /// Creates a trivial passthrough body for cases where we have template only for one accessor kind.
        /// </summary>
        private BlockSyntax? CreateIdentityAccessorBody( SyntaxKind accessorDeclarationKind, SyntaxGenerationContext generationContext )
        {
            switch ( accessorDeclarationKind )
            {
                case SyntaxKind.GetAccessorDeclaration:
                    return Block( ReturnStatement( this.CreateProceedGetExpression( generationContext ) ) );

                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.InitAccessorDeclaration:
                    return Block( ExpressionStatement( this.CreateProceedSetExpression( generationContext ) ) );

                default:
                    throw new AssertionFailedException();
            }
        }

        private ExpressionSyntax CreateProceedGetExpression( SyntaxGenerationContext generationContext )
            => this.CreateMemberAccessExpression( AspectReferenceTargetKind.PropertyGetAccessor, generationContext );

        private ExpressionSyntax CreateProceedSetExpression( SyntaxGenerationContext generationContext )
            => AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                this.CreateMemberAccessExpression( AspectReferenceTargetKind.PropertySetAccessor, generationContext ),
                IdentifierName( "value" ) );
    }
}