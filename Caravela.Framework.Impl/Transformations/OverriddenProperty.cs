// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class OverriddenProperty : OverriddenMember
    {
        public new IProperty OverriddenDeclaration => (IProperty) base.OverriddenDeclaration;

        public TemplateMember<IProperty> PropertyTemplate { get; }

        public TemplateMember<IMethod> GetTemplate { get; }

        public TemplateMember<IMethod> SetTemplate { get; }

        public OverriddenProperty(
            Advice advice,
            IProperty overriddenDeclaration,
            TemplateMember<IProperty> propertyTemplate,
            TemplateMember<IMethod> getTemplate,
            TemplateMember<IMethod> setTemplate )
            : base( advice, overriddenDeclaration )
        {
            // We need the getTemplate and setTemplate to be set by the caller even if propertyTemplate is set.
            // The caller is responsible for verifying the compatibility of the template with the target.

            this.PropertyTemplate = propertyTemplate;
            this.GetTemplate = getTemplate;
            this.SetTemplate = setTemplate;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            using ( context.DiagnosticSink.WithDefaultScope( this.OverriddenDeclaration ) )
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
        }

        private bool TryExpandAccessorTemplate(
            in MemberIntroductionContext context,
            TemplateMember<IMethod> accessorTemplate,
            IMethod accessor,
            [NotNullWhen( true )] out BlockSyntax? body )
        {
            using ( context.DiagnosticSink.WithDefaultScope( accessor ) )
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
                            this.OverriddenDeclaration.Compilation.TypeFactory.GetSpecialType( SpecialType.Void ),
                            context.SyntaxGenerationContext ),
                        _ => throw new AssertionFailedException()
                    };

                var metaApi = MetaApi.ForFieldOrProperty(
                    this.OverriddenDeclaration,
                    accessor,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        accessorTemplate.Cast(),
                        this.Advice.ReadOnlyTags,
                        this.Advice.AspectLayerId,
                        context.SyntaxGenerationContext,
                        this.Advice.Aspect,
                        context.ServiceProvider ) );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.Aspect.Aspect,
                    metaApi,
                    this.OverriddenDeclaration.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( accessor ),
                    context.ServiceProvider.GetService<SyntaxSerializationService>(),
                    context.SyntaxGenerationContext,
                    default,
                    proceedExpression );

                var templateDriver = this.Advice.TemplateInstance.TemplateClass.GetTemplateDriver( accessorTemplate.Declaration! );

                return templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out body );
            }
        }

        /// <summary>
        /// Creates a trivial passthrough body for cases where we have template only for one accessor kind.
        /// </summary>
        /// <param name="accessorDeclarationKind"></param>
        /// <returns></returns>
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