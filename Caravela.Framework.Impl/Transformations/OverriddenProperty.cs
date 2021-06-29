// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal class OverriddenProperty : OverriddenMember
    {
        public new IProperty OverriddenDeclaration => (IProperty) base.OverriddenDeclaration;

        public IProperty? TemplateProperty { get; }

        public IMethod? GetTemplateMethod { get; }

        public IMethod? SetTemplateMethod { get; }

        public OverriddenProperty(
            Advice advice,
            IProperty overriddenDeclaration,
            IProperty? templateProperty,
            IMethod? getTemplateMethod,
            IMethod? setTemplateMethod,
            AspectLinkerOptions? linkerOptions = null )
            : base( advice, overriddenDeclaration, linkerOptions )
        {
            Invariant.Assert( advice != null );
            Invariant.Assert( overriddenDeclaration != null );

            // We need either property template or (one or more) accessor templates, but never both.
            Invariant.Assert( templateProperty != null || getTemplateMethod != null || setTemplateMethod != null );
            Invariant.Assert( !(templateProperty != null && (getTemplateMethod != null || setTemplateMethod != null)) );

            this.TemplateProperty = templateProperty;
            this.GetTemplateMethod = getTemplateMethod;
            this.SetTemplateMethod = setTemplateMethod;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            using ( context.DiagnosticSink.WithDefaultScope( this.OverriddenDeclaration ) )
            {
                var propertyName = context.IntroductionNameProvider.GetOverrideName( this.Advice.AspectLayerId, this.OverriddenDeclaration );

                var getTemplateMethod =
                    this.TemplateProperty != null && !this.TemplateProperty.IsAutoPropertyOrField
                        ? this.TemplateProperty.Getter
                        : this.GetTemplateMethod;

                var setTemplateMethod =
                    this.TemplateProperty != null && !this.TemplateProperty.IsAutoPropertyOrField
                        ? this.TemplateProperty.Setter
                        : this.SetTemplateMethod;

                var setAccessorDeclarationKind = this.OverriddenDeclaration.Writeability == Writeability.InitOnly
                    ? SyntaxKind.InitAccessorDeclaration
                    : SyntaxKind.SetAccessorDeclaration;

                var getAccessorBody =
                    this.OverriddenDeclaration.Getter != null
                        ? getTemplateMethod != null
                            ? this.ExpandAccessorTemplate( context, getTemplateMethod, this.OverriddenDeclaration.Getter )
                            : this.GetIdentityAccessorBody( SyntaxKind.GetAccessorDeclaration )
                        : null;

                var setAccessorBody =
                    this.OverriddenDeclaration.Setter != null
                        ? setTemplateMethod != null
                            ? this.ExpandAccessorTemplate( context, setTemplateMethod, this.OverriddenDeclaration.Setter )
                            : this.GetIdentityAccessorBody( setAccessorDeclarationKind )
                        : null;

                var overrides = new[]
                {
                    new IntroducedMember(
                        this,
                        PropertyDeclaration(
                            List<AttributeListSyntax>(),
                            this.OverriddenDeclaration.GetSyntaxModifierList(),
                            this.OverriddenDeclaration.GetSyntaxReturnType(),
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
                                                    this.OverriddenDeclaration.Getter.AssertNotNull().GetSyntaxModifierList(),
                                                    getAccessorBody )
                                                : null,
                                            setAccessorBody != null
                                                ? AccessorDeclaration(
                                                    setAccessorDeclarationKind,
                                                    List<AttributeListSyntax>(),
                                                    this.OverriddenDeclaration.Setter.AssertNotNull().GetSyntaxModifierList(),
                                                    setAccessorBody )
                                                : null
                                        }.Where( a => a != null )
                                        .AssertNoneNull() ) ),
                            null,
                            null ),
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.Override,
                        this.LinkerOptions,
                        this.OverriddenDeclaration )
                };

                return overrides;
            }
        }

        private BlockSyntax? ExpandAccessorTemplate( in MemberIntroductionContext context, IMethod accessorTemplate, IMethod accessor )
        {
            using ( context.DiagnosticSink.WithDefaultScope( accessor ) )
            {
                var metaApi = MetaApi.ForFieldOrProperty(
                    this.OverriddenDeclaration,
                    accessor,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        accessorTemplate.GetSymbol(),
                        this.Advice.ReadOnlyTags,
                        this.Advice.AspectLayerId,
                        context.ServiceProvider.GetService<AspectPipelineDescription>() ) );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.Aspect.Aspect,
                    metaApi,
                    this.OverriddenDeclaration.Compilation,
                    new LinkerOverridePropertyProceedImpl(
                        this.Advice.AspectLayerId,
                        accessor,
                        LinkingOrder.Default,
                        context.SyntaxFactory ),
                    context.LexicalScope,
                    context.ServiceProvider.GetService<SyntaxSerializationService>(),
                    (ICompilationElementFactory) this.OverriddenDeclaration.Compilation.TypeFactory );

                var templateDriver = this.Advice.Aspect.AspectClass.GetTemplateDriver( accessorTemplate );

                if ( !templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out var newMethodBody ) )
                {
                    // Template expansion error.
                    return null;
                }

                return newMethodBody;
            }
        }

        /// <summary>
        /// Creates a trivial passthrough body for cases where we have template only for one accessor kind.
        /// </summary>
        /// <param name="accessorDeclarationKind"></param>
        /// <returns></returns>
        private BlockSyntax? GetIdentityAccessorBody( SyntaxKind accessorDeclarationKind )
        {
            switch ( accessorDeclarationKind )
            {
                case SyntaxKind.GetAccessorDeclaration:
                    return
                        Block(
                            ReturnStatement(
                                GetPropertyAccessExpression()
                                    .AddLinkerAnnotation(
                                        new LinkerAnnotation(
                                            this.Advice.AspectLayerId,
                                            LinkingOrder.Default,
                                            LinkerAnnotationTargetKind.PropertySetAccessor ) ) ) );

                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.InitAccessorDeclaration:
                    return
                        Block(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    GetPropertyAccessExpression()
                                        .AddLinkerAnnotation(
                                            new LinkerAnnotation(
                                                this.Advice.AspectLayerId,
                                                LinkingOrder.Default,
                                                LinkerAnnotationTargetKind.PropertySetAccessor ) ),
                                    IdentifierName( "value" ) ) ) );

                default:
                    throw new AssertionFailedException();
            }

            ExpressionSyntax GetPropertyAccessExpression()
            {
                if ( !this.OverriddenDeclaration.IsStatic )
                {
                    return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName( this.OverriddenDeclaration.Name ) );
                }
                else
                {
                    // TODO: Full qualification.
                    return IdentifierName( this.OverriddenDeclaration.Name );
                }
            }
        }
    }
}