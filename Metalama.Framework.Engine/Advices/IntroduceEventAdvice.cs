// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Advices
{
    internal class IntroduceEventAdvice : IntroduceMemberAdvice<IEvent, EventBuilder>
    {
        private readonly TemplateMember<IMethod> _addTemplate;
        private readonly TemplateMember<IMethod> _removeTemplate;

        // ReSharper disable once MemberCanBePrivate.Global

        public IEventBuilder Builder => this.MemberBuilder;

        public IntroduceEventAdvice(
            IAspectInstanceInternal aspect,
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
                targetDeclaration,
                eventTemplate.Declaration?.Name ?? explicitName.AssertNotNull(),
                eventTemplate.Declaration != null && eventTemplate.Declaration.IsEventField() );

            this.MemberBuilder.InitializerTemplate = eventTemplate.GetInitializerTemplate();
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( diagnosticAdder );

            this.MemberBuilder.Type =
                (this.Template.Declaration?.Type ?? (INamedType?) this._addTemplate.Declaration?.Parameters.FirstOrDefault().AssertNotNull().Type)
                .AssertNotNull();

            this.MemberBuilder.Accessibility = (this.Template.Declaration?.Accessibility ?? this._addTemplate.Declaration?.Accessibility).AssertNotNull();
        }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            // TODO: Override transformations.
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var existingDeclaration = targetDeclaration.FindClosestVisibleEvent( this.MemberBuilder, observableTransformations.OfType<IEvent>().ToList() );
            var hasNoOverrideSemantics = this.Template.Declaration != null && this.Template.Declaration.IsEventField();

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
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                 existingDeclaration.DeclaringType) ) );
                }

                switch ( this.OverrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Produce fail diagnostic.
                        return
                            AdviceResult.Create(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                     existingDeclaration.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceResult.Create();

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, we fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
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
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
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
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                        targetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                         existingDeclaration.DeclaringType) ) );
                        }
                        else if ( !compilation.InvariantComparer.Equals( this.Builder.Type, existingDeclaration.Type ) )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                                        targetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
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