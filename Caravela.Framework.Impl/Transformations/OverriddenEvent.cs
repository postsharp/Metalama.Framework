// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
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
    internal class OverriddenEvent : OverriddenMember
    {
        public new IEvent OverriddenDeclaration => (IEvent) base.OverriddenDeclaration;

        public IEvent? TemplateEvent { get; }

        public IMethod? AddTemplateMethod { get; }

        public IMethod? RemoveTemplateMethod { get; }

        public OverriddenEvent(
            Advice advice,
            IEvent overriddenDeclaration,
            IEvent? templateEvent,
            IMethod? addTemplateMethod,
            IMethod? removeTemplateMethod )
            : base( advice, overriddenDeclaration )
        {
            Invariant.Assert( advice != null );
            Invariant.Assert( overriddenDeclaration != null );

            // We need event template xor both accessor templates.
            Invariant.Assert( templateEvent != null || (addTemplateMethod != null && removeTemplateMethod != null) );
            Invariant.Assert( !(templateEvent != null && (addTemplateMethod != null || removeTemplateMethod != null)) );

            this.TemplateEvent = templateEvent;
            this.AddTemplateMethod = addTemplateMethod;
            this.RemoveTemplateMethod = removeTemplateMethod;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            if ( this.TemplateEvent?.IsEventField() == true )
            {
                throw new AssertionFailedException();
            }

            using ( context.DiagnosticSink.WithDefaultScope( this.OverriddenDeclaration ) )
            {
                var eventName = context.IntroductionNameProvider.GetOverrideName(
                    this.OverriddenDeclaration.DeclaringType,
                    this.Advice.AspectLayerId,
                    this.OverriddenDeclaration );

                var addTemplateMethod = this.TemplateEvent != null ? this.TemplateEvent.Adder : this.AddTemplateMethod;
                var removeTemplateMethod = this.TemplateEvent != null ? this.TemplateEvent.Remover : this.RemoveTemplateMethod;

                var templateExpansionError = false;
                BlockSyntax? addAccessorBody = null;

                if ( addTemplateMethod != null )
                {
                    templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                        context,
                        addTemplateMethod,
                        this.OverriddenDeclaration.Adder,
                        out addAccessorBody );
                }
                else
                {
                    addAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.GetAccessorDeclaration );
                }

                BlockSyntax? removeAccessorBody = null;

                if ( removeTemplateMethod != null )
                {
                    templateExpansionError = templateExpansionError || !this.TryExpandAccessorTemplate(
                        context,
                        removeTemplateMethod,
                        this.OverriddenDeclaration.Remover,
                        out removeAccessorBody );
                }
                else
                {
                    removeAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.SetAccessorDeclaration );
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
                            this.OverriddenDeclaration.GetSyntaxReturnType(),
                            null,
                            Identifier( eventName ),
                            AccessorList(
                                List(
                                    new[]
                                    {
                                        AccessorDeclaration(
                                            SyntaxKind.AddAccessorDeclaration,
                                            List<AttributeListSyntax>(),
                                            this.OverriddenDeclaration.Adder.AssertNotNull().GetSyntaxModifierList(),
                                            addAccessorBody.AssertNotNull() ),
                                        AccessorDeclaration(
                                            SyntaxKind.RemoveAccessorDeclaration,
                                            List<AttributeListSyntax>(),
                                            this.OverriddenDeclaration.Remover.AssertNotNull().GetSyntaxModifierList(),
                                            removeAccessorBody.AssertNotNull() )
                                    } ) ) ),
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
                        MethodKind.EventAdd => this.CreateAddExpression(),
                        MethodKind.EventRemove => this.CreateRemoveExpression(),
                        _ => throw new AssertionFailedException()
                    },
                    this.OverriddenDeclaration.EventType,
                    false );

                var metaApi = MetaApi.ForEvent(
                    this.OverriddenDeclaration,
                    accessor,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        accessorTemplate.GetSymbol(),
                        this.Advice.ReadOnlyTags,
                        this.Advice.AspectLayerId,
                        context.ServiceProvider.GetService<AspectPipelineDescription>(),
                        proceedExpression ) );

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
                case SyntaxKind.AddAccessorDeclaration:
                    return Block( ExpressionStatement( this.CreateAddExpression() ) );

                case SyntaxKind.RemoveAccessorDeclaration:
                    return Block( ExpressionStatement( this.CreateRemoveExpression() ) );

                default:
                    throw new AssertionFailedException();
            }
        }

        private ExpressionSyntax CreateAddExpression()
        {
            return
                AssignmentExpression(
                    SyntaxKind.AddAssignmentExpression,
                    this.CreateMemberAccessExpression( AspectReferenceTargetKind.EventAddAccessor ),
                    IdentifierName( "value" ) );
        }

        private ExpressionSyntax CreateRemoveExpression()
        {
            return
                AssignmentExpression(
                    SyntaxKind.SubtractAssignmentExpression,
                    this.CreateMemberAccessExpression( AspectReferenceTargetKind.EventRemoveAccessor ),
                    IdentifierName( "value" ) );
        }
    }
}