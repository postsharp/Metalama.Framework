// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroduceEventAdvice : IntroduceMemberAdvice<IEvent, EventBuilder>
    {
        private readonly TemplateMember<IMethod> _addTemplate;
        private readonly TemplateMember<IMethod> _removeTemplate;

        // ReSharper disable once MemberCanBePrivate.Global

        public IEventBuilder Builder => this.MemberBuilder;

        public IntroduceEventAdvice(
            AspectInstance aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            string? explicitName,
            TemplateMember<IEvent> eventTemplate,
            TemplateMember<IMethod> addTemplate,
            TemplateMember<IMethod> removeTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            Dictionary<string, object?>? tags )
            : base( aspect, templateInstance, targetDeclaration, eventTemplate, scope, overrideStrategy, layerName, tags )
        {
            this._addTemplate = addTemplate;
            this._removeTemplate = removeTemplate;

            this.MemberBuilder = new EventBuilder(
                this,
                this.TargetDeclaration,
                eventTemplate.Declaration?.Name ?? explicitName.AssertNotNull(),
                eventTemplate.Declaration != null && eventTemplate.Declaration.IsEventField() );
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( declarativeAdvices, diagnosticAdder );

            this.MemberBuilder.Type =
                (this.TemplateMember?.Type ?? (INamedType?) this._addTemplate.Declaration?.Parameters.FirstOrDefault().AssertNotNull().Type)
                .AssertNotNull();

            this.MemberBuilder.Accessibility = (this.TemplateMember?.Accessibility ?? this._addTemplate.Declaration?.Accessibility).AssertNotNull();
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // TODO: Override transformations.
            var existingDeclaration = this.TargetDeclaration.Events.OfExactSignature( this.MemberBuilder, false, false );
            var hasNoOverrideSemantics = this.TemplateMember != null && this.TemplateMember.IsEventField();

            if ( existingDeclaration == null )
            {
                // There is no existing declaration, we will introduce and override the introduced.
                if ( hasNoOverrideSemantics )
                {
                    return AdviceResult.Create( this.MemberBuilder );
                }
                else
                {
                    return AdviceResult.Create(
                        this.MemberBuilder,
                        new OverriddenEvent(
                            this,
                            this.MemberBuilder,
                            this.Template,
                            this._addTemplate,
                            this._removeTemplate ) );
                }
            }
            else
            {
                if ( existingDeclaration.IsStatic != this.MemberBuilder.IsStatic )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );
                }

                switch ( this.OverrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Produce fail diagnostic.
                        return
                            AdviceResult.Create(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceResult.Create();

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, we fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( this.TargetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            if ( hasNoOverrideSemantics )
                            {
                                return AdviceResult.Create();
                            }
                            else
                            {
                                var overriddenMethod = new OverriddenEvent(
                                    this,
                                    existingDeclaration,
                                    this.Template,
                                    this._addTemplate,
                                    this._removeTemplate );

                                return AdviceResult.Create( overriddenMethod );
                            }
                        }
                        else
                        {
                            this.MemberBuilder.IsNew = true;
                            this.MemberBuilder.OverriddenEvent = existingDeclaration;

                            if ( hasNoOverrideSemantics )
                            {
                                return AdviceResult.Create( this.MemberBuilder );
                            }
                            else
                            {
                                var overriddenMethod = new OverriddenEvent(
                                    this,
                                    this.MemberBuilder,
                                    this.Template,
                                    this._addTemplate,
                                    this._removeTemplate );

                                return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
                            }
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( this.TargetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            if ( hasNoOverrideSemantics )
                            {
                                return AdviceResult.Create();
                            }
                            else
                            {
                                var overriddenMethod = new OverriddenEvent(
                                    this,
                                    existingDeclaration,
                                    this.Template,
                                    this._addTemplate,
                                    this._removeTemplate );

                                return AdviceResult.Create( overriddenMethod );
                            }
                        }
                        else if ( existingDeclaration.IsSealed || !existingDeclaration.IsVirtual )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateDiagnostic(
                                        this.TargetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration,
                                         existingDeclaration.DeclaringType) ) );
                        }
                        else if ( !compilation.InvariantComparer.Equals( this.Builder.Type, existingDeclaration.Type ) )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateDiagnostic(
                                        this.TargetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration,
                                         existingDeclaration.DeclaringType, existingDeclaration.Type) ) );
                        }
                        else
                        {
                            this.MemberBuilder.IsOverride = true;
                            this.MemberBuilder.OverriddenEvent = existingDeclaration;

                            if ( hasNoOverrideSemantics )
                            {
                                return AdviceResult.Create( this.MemberBuilder );
                            }
                            else
                            {
                                var overriddenEvent = new OverriddenEvent(
                                    this,
                                    this.MemberBuilder,
                                    this.Template,
                                    this._addTemplate,
                                    this._removeTemplate );

                                return AdviceResult.Create( this.MemberBuilder, overriddenEvent );
                            }
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}