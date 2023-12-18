// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.Transformations
{
    internal sealed class OverrideEventTransformation : OverrideMemberTransformation
    {
        private new IEvent OverriddenDeclaration => (IEvent) base.OverriddenDeclaration;

        private BoundTemplateMethod? AddTemplate { get; }

        private BoundTemplateMethod? RemoveTemplate { get; }

        public OverrideEventTransformation(
            Advice advice,
            IEvent overriddenDeclaration,
            BoundTemplateMethod? addTemplate,
            BoundTemplateMethod? removeTemplate,
            IObjectReader tags )
            : base( advice, overriddenDeclaration, tags )
        {
            this.AddTemplate = addTemplate;
            this.RemoveTemplate = removeTemplate;
        }

        public override IEnumerable<InjectedMemberOrNamedType> GetInjectedMembers( MemberInjectionContext context )
        {
            var eventName = context.InjectionNameProvider.GetOverrideName(
                this.OverriddenDeclaration.DeclaringType,
                this.ParentAdvice.AspectLayerId,
                this.OverriddenDeclaration );

            var templateExpansionError = false;
            BlockSyntax? addAccessorBody = null;

            if ( this.AddTemplate != null )
            {
                templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                    context,
                    this.AddTemplate,
                    this.OverriddenDeclaration.AddMethod,
                    context.SyntaxGenerationContext,
                    out addAccessorBody );
            }
            else
            {
                addAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.AddAccessorDeclaration, context.SyntaxGenerationContext );
            }

            BlockSyntax? removeAccessorBody = null;

            if ( this.RemoveTemplate != null )
            {
                templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                    context,
                    this.RemoveTemplate,
                    this.OverriddenDeclaration.RemoveMethod,
                    context.SyntaxGenerationContext,
                    out removeAccessorBody );
            }
            else
            {
                removeAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.RemoveAccessorDeclaration, context.SyntaxGenerationContext );
            }

            if ( templateExpansionError )
            {
                // Template expansion error.
                return Enumerable.Empty<InjectedMemberOrNamedType>();
            }

            var modifiers = this.OverriddenDeclaration
                .GetSyntaxModifierList( ModifierCategories.Static )
                .Insert( 0, Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) );

            // TODO: Do not throw exception when template expansion fails.
            var overrides = new[]
            {
                new InjectedMemberOrNamedType(
                    this,
                    EventDeclaration(
                        List<AttributeListSyntax>(),
                        modifiers,
                        Token( SyntaxKind.EventKeyword ).WithTrailingTrivia( Space ),
                        context.SyntaxGenerator.EventType( this.OverriddenDeclaration ).WithTrailingTrivia( Space ),
                        null!,
                        Identifier( eventName ),
                        AccessorList(
                            List(
                                new[]
                                {
                                    AccessorDeclaration(
                                        SyntaxKind.AddAccessorDeclaration,
                                        List<AttributeListSyntax>(),
                                        this.OverriddenDeclaration.AddMethod.AssertNotNull().GetSyntaxModifierList(),
                                        addAccessorBody.AssertNotNull() ),
                                    AccessorDeclaration(
                                        SyntaxKind.RemoveAccessorDeclaration,
                                        List<AttributeListSyntax>(),
                                        this.OverriddenDeclaration.RemoveMethod.AssertNotNull().GetSyntaxModifierList(),
                                        removeAccessorBody.AssertNotNull() )
                                } ) ) ),
                    this.ParentAdvice.AspectLayerId,
                    InjectedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };

            return overrides;
        }

        private bool TryExpandAccessorTemplate(
            MemberInjectionContext context,
            BoundTemplateMethod accessorTemplate,
            IMethod accessor,
            SyntaxGenerationContext generationContext,
            [NotNullWhen( true )] out BlockSyntax? body )
        {
            var proceedExpression = new SyntaxUserExpression(
                accessor.MethodKind switch
                {
                    MethodKind.EventAdd => this.CreateAddExpression( generationContext ),
                    MethodKind.EventRemove => this.CreateRemoveExpression( generationContext ),
                    _ => throw new AssertionFailedException( $"Unexpected MethodKind: {accessor.MethodKind}." )
                },
                this.OverriddenDeclaration.Compilation.GetCompilationModel().Cache.SystemVoidType );

            var metaApi = MetaApi.ForEvent(
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
                _ => proceedExpression,
                this.ParentAdvice.AspectLayerId );

            var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( accessorTemplate.TemplateMember.Declaration );

            return templateDriver.TryExpandDeclaration( expansionContext, accessorTemplate.TemplateArguments, out body );
        }

        /// <summary>
        /// Creates a trivial passthrough body for cases where we have template only for one accessor kind.
        /// </summary>
        private BlockSyntax CreateIdentityAccessorBody( SyntaxKind accessorDeclarationKind, SyntaxGenerationContext generationContext )
        {
            switch ( accessorDeclarationKind )
            {
                case SyntaxKind.AddAccessorDeclaration:
                    return SyntaxFactoryEx.FormattedBlock( ExpressionStatement( this.CreateAddExpression( generationContext ) ) );

                case SyntaxKind.RemoveAccessorDeclaration:
                    return SyntaxFactoryEx.FormattedBlock( ExpressionStatement( this.CreateRemoveExpression( generationContext ) ) );

                default:
                    throw new AssertionFailedException( $"Unexpected syntax kind: {accessorDeclarationKind}." );
            }
        }

        private ExpressionSyntax CreateAddExpression( SyntaxGenerationContext generationContext )
            => AssignmentExpression(
                SyntaxKind.AddAssignmentExpression,
                this.CreateMemberAccessExpression( AspectReferenceTargetKind.EventAddAccessor, generationContext ),
                IdentifierName( "value" ) );

        private ExpressionSyntax CreateRemoveExpression( SyntaxGenerationContext generationContext )
            => AssignmentExpression(
                SyntaxKind.SubtractAssignmentExpression,
                this.CreateMemberAccessExpression( AspectReferenceTargetKind.EventRemoveAccessor, generationContext ),
                IdentifierName( "value" ) );
    }
}