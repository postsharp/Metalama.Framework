// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
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

        public IProperty? TemplateProperty { get; }

        public IMethod? GetTemplateMethod { get; }

        public IMethod? SetTemplateMethod { get; }

        public OverriddenProperty(
            Advice advice,
            IProperty overriddenDeclaration,
            IProperty? templateProperty,
            IMethod? getTemplateMethod,
            IMethod? setTemplateMethod )
            : base( advice, overriddenDeclaration )
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
                var propertyName = context.IntroductionNameProvider.GetOverrideName(
                    this.OverriddenDeclaration.DeclaringType,
                    this.Advice.AspectLayerId,
                    this.OverriddenDeclaration );

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

                var templateExpansionError = false;
                BlockSyntax? getAccessorBody = null;

                if ( this.OverriddenDeclaration.Getter != null )
                {
                    if ( getTemplateMethod != null )
                    {
                        templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                            context,
                            getTemplateMethod,
                            this.OverriddenDeclaration.Getter,
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

                if ( this.OverriddenDeclaration.Setter != null )
                {
                    if ( setTemplateMethod != null )
                    {
                        templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                            context,
                            setTemplateMethod,
                            this.OverriddenDeclaration.Setter,
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
                        this.OverriddenDeclaration )
                };

                return overrides;
            }
        }

        private bool TryExpandAccessorTemplate(
            in MemberIntroductionContext context,
            IMethod accessorTemplate,
            IMethod accessor,
            [NotNullWhen( true )] out BlockSyntax? body )
        {
            using ( context.DiagnosticSink.WithDefaultScope( accessor ) )
            {
                var proceedExpression = new DynamicExpression(
                    accessor.MethodKind switch
                    {
                        MethodKind.PropertyGet => this.CreateGetExpression(),
                        MethodKind.PropertySet => this.CreateSetExpression(),
                        _ => throw new AssertionFailedException()
                    },
                    accessor.MethodKind switch
                    {
                        MethodKind.PropertyGet => this.OverriddenDeclaration.Type,
                        MethodKind.PropertySet => this.OverriddenDeclaration.Compilation.TypeFactory.GetSpecialType( SpecialType.Void ),
                        _ => throw new AssertionFailedException()
                    },
                    false );

                var metaApi = MetaApi.ForFieldOrProperty(
                    this.OverriddenDeclaration,
                    accessor,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        accessorTemplate.GetSymbol(),
                        this.Advice.ReadOnlyTags,
                        this.Advice.AspectLayerId,
                        proceedExpression,
                        context.ServiceProvider ) );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.Aspect.Aspect,
                    metaApi,
                    this.OverriddenDeclaration.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( accessor ),
                    context.ServiceProvider.GetService<SyntaxSerializationService>(),
                    (ICompilationElementFactory) this.OverriddenDeclaration.Compilation.TypeFactory );

                var templateDriver = this.Advice.Aspect.AspectClass.GetTemplateDriver( accessorTemplate );

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
                    return Block( ReturnStatement( this.CreateGetExpression() ) );

                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.InitAccessorDeclaration:
                    return Block( ExpressionStatement( this.CreateSetExpression() ) );

                default:
                    throw new AssertionFailedException();
            }
        }

        private ExpressionSyntax CreateGetExpression()
        {
            return this.CreateMemberAccessExpression( AspectReferenceTargetKind.PropertyGetAccessor );
        }

        private ExpressionSyntax CreateSetExpression()
        {
            return
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    this.CreateMemberAccessExpression( AspectReferenceTargetKind.PropertySetAccessor ),
                    IdentifierName( "value" ) );
        }
    }
}