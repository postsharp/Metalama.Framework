// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    internal class OverrideEventTransformation : OverrideMemberTransformation
    {
        private readonly IObjectReader? _parameters;

        public new IEvent OverriddenDeclaration => (IEvent) base.OverriddenDeclaration;

        public TemplateMember<IEvent>? EventTemplate { get; }

        public BoundTemplateMethod? AddTemplate { get; }

        public BoundTemplateMethod? RemoveTemplate { get; }

        public OverrideEventTransformation(
            Advice advice,
            IEvent overriddenDeclaration,
            TemplateMember<IEvent>? eventTemplate,
            TemplateMember<IMethod>? addTemplate,
            TemplateMember<IMethod>? removeTemplate,
            IObjectReader tags,
            IObjectReader? parameters )
            : base( advice, overriddenDeclaration, tags )
        {
            this._parameters = parameters;

            // We need event template xor both accessor templates.
            Invariant.Assert( eventTemplate != null || (addTemplate != null && removeTemplate != null) );
            Invariant.Assert( !(eventTemplate != null && (addTemplate != null || removeTemplate != null)) );
            Invariant.Assert( !(eventTemplate != null && eventTemplate.Declaration.IsEventField()) );

            this.EventTemplate = eventTemplate;

            this.AddTemplate = addTemplate?.ForOverride( overriddenDeclaration.AddMethod, parameters );
            this.RemoveTemplate = removeTemplate?.ForOverride( overriddenDeclaration.RemoveMethod, parameters );
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( MemberIntroductionContext context )
        {
            if ( this.EventTemplate?.Declaration.IsEventField() == true )
            {
                throw new AssertionFailedException();
            }

            var eventName = context.IntroductionNameProvider.GetOverrideName(
                this.OverriddenDeclaration.DeclaringType,
                this.ParentAdvice.AspectLayerId,
                this.OverriddenDeclaration );

            BoundTemplateMethod? GetBoundTemplateMethod( IMethod? templateMethod, BoundTemplateMethod? sourceBoundTemplate )
            {
                if ( this.EventTemplate != null )
                {
                    // We have an event template.

                    if ( templateMethod == null )
                    {
                        // No template.
                        return default;
                    }
                    else
                    {
                        return TemplateMemberFactory.Create(
                                templateMethod,
                                this.EventTemplate.TemplateClassMember.Accessors[templateMethod.GetSymbol()!.MethodKind] )
                            .ForIntroduction( this._parameters );
                    }
                }
                else
                {
                    // We have accessor templates.
                    return sourceBoundTemplate;
                }
            }

            var addTemplateMethod = GetBoundTemplateMethod( this.EventTemplate?.Declaration.AddMethod, this.AddTemplate );
            var removeTemplateMethod = GetBoundTemplateMethod( this.EventTemplate?.Declaration.RemoveMethod, this.RemoveTemplate );

            var templateExpansionError = false;
            BlockSyntax? addAccessorBody = null;

            if ( addTemplateMethod != null )
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
                addAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.AddAccessorDeclaration, context.SyntaxGenerationContext );
            }

            BlockSyntax? removeAccessorBody = null;

            if ( removeTemplateMethod != null )
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
                removeAccessorBody = this.CreateIdentityAccessorBody( SyntaxKind.RemoveAccessorDeclaration, context.SyntaxGenerationContext );
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
                    this.ParentAdvice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };

            return overrides;
        }

        private bool TryExpandAccessorTemplate(
            in MemberIntroductionContext context,
            BoundTemplateMethod accessorTemplate,
            IMethod accessor,
            SyntaxGenerationContext generationContext,
            [NotNullWhen( true )] out BlockSyntax? body )
        {
            var proceedExpression = new BuiltUserExpression(
                accessor.MethodKind switch
                {
                    MethodKind.EventAdd => this.CreateAddExpression( generationContext ),
                    MethodKind.EventRemove => this.CreateRemoveExpression( generationContext ),
                    _ => throw new AssertionFailedException()
                },
                this.OverriddenDeclaration.Compilation.GetCompilationModel().Factory.GetSpecialType( SpecialType.Void ) );

            var metaApi = MetaApi.ForEvent(
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