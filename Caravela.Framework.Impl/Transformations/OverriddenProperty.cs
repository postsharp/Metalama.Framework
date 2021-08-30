// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Utilities;
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

        public Template<IProperty> PropertyTemplate { get; }

        public Template<IMethod> GetTemplate { get; }

        public Template<IMethod> SetTemplate { get; }

        public OverriddenProperty(
            Advice advice,
            IProperty overriddenDeclaration,
            Template<IProperty> propertyTemplate,
            Template<IMethod> getTemplate,
            Template<IMethod> setTemplate )
            : base( advice, overriddenDeclaration )
        {
            // We need either property template or (one or more) accessor templates, but never both.
            Invariant.Assert( propertyTemplate.IsNotNull || getTemplate.IsNotNull || setTemplate.IsNotNull );
            Invariant.Assert( !(propertyTemplate.IsNotNull && (getTemplate.IsNotNull || setTemplate.IsNotNull)) );

            if ( propertyTemplate.IsNotNull )
            {
                this.PropertyTemplate = propertyTemplate;

                if ( !propertyTemplate.Declaration!.IsAutoPropertyOrField )
                {
                    this.GetTemplate = Template.Create( this.PropertyTemplate.Declaration!.GetMethod, this.GetTemplate.TemplateInfo );
                    this.SetTemplate = Template.Create( this.PropertyTemplate.Declaration!.SetMethod, this.GetTemplate.TemplateInfo );
                }
            }
            else
            {
                this.GetTemplate = getTemplate;
                this.SetTemplate = setTemplate;
            }

            this.GetTemplate.ValidateTarget( overriddenDeclaration.GetMethod );
            this.SetTemplate.ValidateTarget( overriddenDeclaration.SetMethod );
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
                        getAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.GetAccessorDeclaration );
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
                        setAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.SetAccessorDeclaration );
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
                            SyntaxHelpers.CreateSyntaxForPropertyType( this.OverriddenDeclaration ),
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
            Template<IMethod> accessorTemplate,
            IMethod accessor,
            [NotNullWhen( true )] out BlockSyntax? body )
        {
            using ( context.DiagnosticSink.WithDefaultScope( accessor ) )
            {
                var proceedExpression =
                    accessor.MethodKind switch
                    {
                        MethodKind.PropertyGet => ProceedHelper.CreateProceedDynamicExpression(
                            this.CreateProceedGetExpression(),
                            this.GetTemplate,
                            this.OverriddenDeclaration.GetMethod.AssertNotNull() ),
                        MethodKind.PropertySet => new DynamicExpression(
                            this.CreateProceedSetExpression(),
                            this.OverriddenDeclaration.Compilation.TypeFactory.GetSpecialType( SpecialType.Void ) ),
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
                        context.ServiceProvider ) );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.Aspect.Aspect,
                    metaApi,
                    this.OverriddenDeclaration.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( accessor ),
                    context.ServiceProvider.GetService<SyntaxSerializationService>(),
                    (ICompilationElementFactory) this.OverriddenDeclaration.Compilation.TypeFactory,
                    default,
                    proceedExpression );

                var templateDriver = this.Advice.Aspect.AspectClass.GetTemplateDriver( accessorTemplate.Declaration! );

                return templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out body );
            }
        }

        /// <summary>
        /// Creates a trivial passthrough body for cases where we have template only for one accessor kind.
        /// </summary>
        /// <param name="accessorDeclarationKind"></param>
        /// <returns></returns>
        private BlockSyntax? CreateIdentityAccessorBody( SyntaxKind accessorDeclarationKind )
        {
            switch ( accessorDeclarationKind )
            {
                case SyntaxKind.GetAccessorDeclaration:
                    return Block( ReturnStatement( this.CreateProceedGetExpression() ) );

                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.InitAccessorDeclaration:
                    return Block( ExpressionStatement( this.CreateProceedSetExpression() ) );

                default:
                    throw new AssertionFailedException();
            }
        }

        private ExpressionSyntax CreateProceedGetExpression() => this.CreateMemberAccessExpression( AspectReferenceTargetKind.PropertyGetAccessor );

        private ExpressionSyntax CreateProceedSetExpression()
            => AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                this.CreateMemberAccessExpression( AspectReferenceTargetKind.PropertySetAccessor ),
                IdentifierName( "value" ) );
    }
}