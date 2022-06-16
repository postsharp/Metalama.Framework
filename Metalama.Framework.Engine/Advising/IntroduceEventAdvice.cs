﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    internal class IntroduceEventAdvice : IntroduceMemberAdvice<IEvent, EventBuilder>
    {
        private readonly TemplateMember<IMethod> _addTemplate;
        private readonly TemplateMember<IMethod> _removeTemplate;
        private readonly IObjectReader _parameters;

        // ReSharper disable once MemberCanBePrivate.Global

        public IntroduceEventAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            ICompilation sourceCompilation,
            string? explicitName,
            TemplateMember<IEvent> eventTemplate,
            TemplateMember<IMethod> addTemplate,
            TemplateMember<IMethod> removeTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            Action<IEventBuilder>? buildAction,
            string? layerName,
            IObjectReader tags,
            IObjectReader parameters )
            : base(
                aspect,
                templateInstance,
                targetDeclaration,
                sourceCompilation,
                explicitName,
                eventTemplate,
                scope,
                overrideStrategy,
                buildAction,
                layerName,
                tags )
        {
            this._addTemplate = addTemplate;
            this._removeTemplate = removeTemplate;
            this._parameters = parameters;

            this.Builder = new EventBuilder(
                this,
                targetDeclaration,
                this.MemberName,
                eventTemplate.Declaration != null && eventTemplate.Declaration.IsEventField(),
                tags );

            this.Builder.InitializerTemplate = eventTemplate.GetInitializerTemplate();
        }

        protected override void InitializeCore( IServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            base.InitializeCore( serviceProvider, diagnosticAdder );

            this.Builder.Type =
                (this.Template.Declaration?.Type ?? (INamedType?) this._addTemplate.Declaration?.Parameters.FirstOrDefault().AssertNotNull().Type)
                .AssertNotNull();

            this.Builder.Accessibility = (this.Template.Declaration?.Accessibility ?? this._addTemplate.Declaration?.Accessibility).AssertNotNull();
        }

        public override AdviceImplementationResult Implement(
            IServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // this.Tags: Override transformations.
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var existingDeclaration = targetDeclaration.FindClosestUniquelyNamedMember( this.Builder.Name );
            var hasNoOverrideSemantics = this.Template.Declaration != null && this.Template.Declaration.IsEventField();

            if ( existingDeclaration == null )
            {
                // TODO: validate event type.

                // There is no existing declaration, we will introduce and override the introduced.
                addTransformation( this.Builder );

                if ( !hasNoOverrideSemantics )
                {
                    addTransformation(
                        new OverrideEventTransformation(
                            this,
                            this.Builder,
                            this.Template,
                            this._addTemplate,
                            this._removeTemplate,
                            this.Tags,
                            this._parameters ) );
                }

                return AdviceImplementationResult.Success( this.Builder );
            }
            else
            {
                if ( existingDeclaration is not IEvent existingEvent )
                {
                    return AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration, existingDeclaration.DeclarationKind) ) );
                }
                else if ( existingDeclaration.IsStatic != this.Builder.IsStatic )
                {
                    return AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                             existingDeclaration.DeclaringType) ) );
                }
                else if ( !compilation.InvariantComparer.Equals( this.Builder.Type, existingEvent.Type ) )
                {
                    return AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                             existingDeclaration.DeclaringType, existingEvent.Type) ) );
                }

                switch ( this.OverrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Produce fail diagnostic.
                        return AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                 existingDeclaration.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceImplementationResult.Ignored;

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, we fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            if ( hasNoOverrideSemantics )
                            {
                                return AdviceImplementationResult.Ignored;
                            }
                            else
                            {
                                var overriddenMethod = new OverrideEventTransformation(
                                    this,
                                    existingEvent,
                                    this.Template,
                                    this._addTemplate,
                                    this._removeTemplate,
                                    this.Tags,
                                    this._parameters );

                                addTransformation( overriddenMethod );

                                return AdviceImplementationResult.Success( AdviceOutcome.New );
                            }
                        }
                        else
                        {
                            this.Builder.IsNew = true;

                            if ( hasNoOverrideSemantics )
                            {
                                addTransformation( this.Builder );

                                return AdviceImplementationResult.Success( AdviceOutcome.New );
                            }
                            else
                            {
                                var overriddenMethod = new OverrideEventTransformation(
                                    this,
                                    this.Builder,
                                    this.Template,
                                    this._addTemplate,
                                    this._removeTemplate,
                                    this.Tags,
                                    this._parameters );

                                addTransformation( this.Builder );
                                addTransformation( overriddenMethod );

                                return AdviceImplementationResult.Success( AdviceOutcome.New );
                            }
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            if ( hasNoOverrideSemantics )
                            {
                                return AdviceImplementationResult.Ignored;
                            }
                            else
                            {
                                var overriddenMethod = new OverrideEventTransformation(
                                    this,
                                    existingEvent,
                                    this.Template,
                                    this._addTemplate,
                                    this._removeTemplate,
                                    this.Tags,
                                    this._parameters );

                                addTransformation( overriddenMethod );

                                return AdviceImplementationResult.Success( AdviceOutcome.Override );
                            }
                        }
                        else if ( existingDeclaration.IsSealed || !existingDeclaration.IsVirtual )
                        {
                            return
                                AdviceImplementationResult.Failed(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                        targetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                         existingDeclaration.DeclaringType) ) );
                        }
                        else
                        {
                            this.Builder.IsOverride = true;
                            this.Builder.OverriddenEvent = existingEvent;

                            if ( hasNoOverrideSemantics )
                            {
                                addTransformation( this.Builder );

                                return AdviceImplementationResult.Success( AdviceOutcome.Override );
                            }
                            else
                            {
                                var overriddenEvent = new OverrideEventTransformation(
                                    this,
                                    this.Builder,
                                    this.Template,
                                    this._addTemplate,
                                    this._removeTemplate,
                                    this.Tags,
                                    this._parameters );

                                addTransformation( this.Builder );
                                addTransformation( overriddenEvent );

                                return AdviceImplementationResult.Success( AdviceOutcome.Override );
                            }
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}