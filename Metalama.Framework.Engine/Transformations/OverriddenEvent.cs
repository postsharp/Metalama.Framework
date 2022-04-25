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
    internal class OverriddenEvent : OverriddenMember
    {
        public new IEvent OverriddenDeclaration => (IEvent) base.OverriddenDeclaration;

        public TemplateMember<IEvent> EventTemplate { get; }

        public TemplateMember<IMethod> AddTemplate { get; }

        public TemplateMember<IMethod> RemoveTemplate { get; }

        public OverriddenEvent(
            Advice advice,
            IEvent overriddenDeclaration,
            TemplateMember<IEvent> eventTemplate,
            TemplateMember<IMethod> addTemplate,
            TemplateMember<IMethod> removeTemplate,
            ITagReader tags )
            : base( advice, overriddenDeclaration, tags )
        {
            // We need event template xor both accessor templates.
            Invariant.Assert( eventTemplate.IsNotNull || (addTemplate.IsNotNull && removeTemplate.IsNotNull) );
            Invariant.Assert( !(eventTemplate.IsNotNull && (addTemplate.IsNotNull || removeTemplate.IsNotNull)) );

            this.EventTemplate = eventTemplate;
            this.AddTemplate = addTemplate;
            this.RemoveTemplate = removeTemplate;

            this.AddTemplate.ValidateTarget( overriddenDeclaration.AddMethod );
            this.RemoveTemplate.ValidateTarget( overriddenDeclaration.RemoveMethod );
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            if ( this.EventTemplate.Declaration?.IsEventField() == true )
            {
                throw new AssertionFailedException();
            }

            var eventName = context.IntroductionNameProvider.GetOverrideName(
                this.OverriddenDeclaration.DeclaringType,
                this.Advice.AspectLayerId,
                this.OverriddenDeclaration );

            var addTemplateMethod = this.EventTemplate.Declaration != null
                ? TemplateMember.Create( this.EventTemplate.Declaration.AddMethod, this.AddTemplate.TemplateInfo )
                : this.AddTemplate;

            var removeTemplateMethod = this.EventTemplate.Declaration != null
                ? TemplateMember.Create( this.EventTemplate.Declaration.RemoveMethod, this.RemoveTemplate.TemplateInfo )
                : this.RemoveTemplate;

            var templateExpansionError = false;
            BlockSyntax? addAccessorBody = null;

            if ( addTemplateMethod.IsNotNull )
            {
                templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                    context,
                    addTemplateMethod,
                    this.OverriddenDeclaration.AddMethod,
                    context.SyntaxGenerationContext,
                    out addAccessorBody );
            }
            else
            {
                addAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.GetAccessorDeclaration, context.SyntaxGenerationContext );
            }

            BlockSyntax? removeAccessorBody = null;

            if ( removeTemplateMethod.IsNotNull )
            {
                templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                    context,
                    removeTemplateMethod,
                    this.OverriddenDeclaration.RemoveMethod,
                    context.SyntaxGenerationContext,
                    out removeAccessorBody );
            }
            else
            {
                removeAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.SetAccessorDeclaration, context.SyntaxGenerationContext );
            }

            if ( templateExpansionError )
            {
                // Template expansion error.
                return Enumerable.Empty<IntroducedMember>();
            }

            // TODO: Do not throw exception when template expansion fails.
            var overrides = new[]
            {
                new IntroducedMember(
                    this,
                    EventDeclaration(
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.GetSyntaxModifierList(),
                        context.SyntaxGenerator.EventType( this.OverriddenDeclaration ),
                        null,
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
                    this.Advice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };

            return overrides;
        }

        private bool TryExpandAccessorTemplate(
            in MemberIntroductionContext context,
            TemplateMember<IMethod> accessorTemplate,
            IMethod accessor,
            SyntaxGenerationContext generationContext,
            [NotNullWhen( true )] out BlockSyntax? body )
        {
            var proceedExpression = new UserExpression(
                accessor.MethodKind switch
                {
                    MethodKind.EventAdd => this.CreateAddExpression( generationContext ),
                    MethodKind.EventRemove => this.CreateRemoveExpression( generationContext ),
                    _ => throw new AssertionFailedException()
                },
                this.OverriddenDeclaration.Compilation.TypeFactory.GetSpecialType( SpecialType.Void ),
                context.SyntaxGenerationContext );

            var metaApi = MetaApi.ForEvent(
                this.OverriddenDeclaration,
                accessor,
                new MetaApiProperties(
                    context.DiagnosticSink,
                    accessorTemplate.Cast(),
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

            var templateDriver = this.Advice.TemplateInstance.TemplateClass.GetTemplateDriver( accessorTemplate.Declaration! );

            return templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out body );
        }

        /// <summary>
        /// Creates a trivial passthrough body for cases where we have template only for one accessor kind.
        /// </summary>
        private BlockSyntax? CreateIdentityAccessorBody( SyntaxKind accessorDeclarationKind, SyntaxGenerationContext generationContext )
        {
            switch ( accessorDeclarationKind )
            {
                case SyntaxKind.AddAccessorDeclaration:
                    return Block( ExpressionStatement( this.CreateAddExpression( generationContext ) ) );

                case SyntaxKind.RemoveAccessorDeclaration:
                    return Block( ExpressionStatement( this.CreateRemoveExpression( generationContext ) ) );

                default:
                    throw new AssertionFailedException();
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