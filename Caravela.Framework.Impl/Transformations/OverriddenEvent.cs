// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
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
            IMethod? removeTemplateMethod,
            AspectLinkerOptions? linkerOptions = null )
            : base( advice, overriddenDeclaration, linkerOptions )
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
            if (this.TemplateEvent?.IsEventField() == true)
            {
                throw new AssertionFailedException();
            }

            using ( context.DiagnosticSink.WithDefaultScope( this.OverriddenDeclaration ) )
            {
                var eventName = context.IntroductionNameProvider.GetOverrideName( this.Advice.AspectLayerId, this.OverriddenDeclaration );

                var addTemplateMethod = this.TemplateEvent != null ? this.TemplateEvent.Adder : this.AddTemplateMethod;
                var removeTemplateMethod = this.TemplateEvent != null ? this.TemplateEvent.Remover : this.RemoveTemplateMethod;

                var addAccessorBody =
                    addTemplateMethod != null
                        ? this.ExpandAccessorTemplate( context, addTemplateMethod, this.OverriddenDeclaration.Adder )
                        : this.GetIdentityAccessorBody( SyntaxKind.AddAccessorDeclaration );

                var removeAccessorBody =
                    removeTemplateMethod != null
                        ? this.ExpandAccessorTemplate( context, removeTemplateMethod, this.OverriddenDeclaration.Remover )
                        : this.GetIdentityAccessorBody( SyntaxKind.RemoveAccessorDeclaration );

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
                var metaApi = MetaApi.ForEvent(
                    this.OverriddenDeclaration,
                    accessor,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        accessorTemplate.GetSymbol(),
                        this.Advice.Options.Tags,
                        this.Advice.AspectLayerId ) );

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