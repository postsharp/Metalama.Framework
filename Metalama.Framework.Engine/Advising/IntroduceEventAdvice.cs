﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using Attribute = Metalama.Framework.Engine.CodeModel.Attribute;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class IntroduceEventAdvice : IntroduceMemberAdvice<IEvent, EventBuilder>
    {
        private readonly PartiallyBoundTemplateMethod? _addTemplate;
        private readonly PartiallyBoundTemplateMethod? _removeTemplate;

        private BoundTemplateMethod? _boundAddTemplate;
        private BoundTemplateMethod? _boundRemoveTemplate;

        // ReSharper disable once MemberCanBePrivate.Global

        public IntroduceEventAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            ICompilation sourceCompilation,
            string? explicitName,
            TemplateMember<IEvent>? eventTemplate,
            PartiallyBoundTemplateMethod? addTemplate,
            PartiallyBoundTemplateMethod? removeTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            Action<IEventBuilder>? buildAction,
            string? layerName,
            IObjectReader tags )
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

            this.Builder = new EventBuilder(
                this,
                targetDeclaration,
                this.MemberName,
                eventTemplate?.Declaration != null && eventTemplate.Declaration.IsEventField() == true,
                tags );

            this.Builder.InitializerTemplate = eventTemplate.GetInitializerTemplate();
        }

        protected override void InitializeCore(
            ProjectServiceProvider serviceProvider,
            IDiagnosticAdder diagnosticAdder,
            TemplateAttributeProperties? templateAttributeProperties )
        {
            base.InitializeCore( serviceProvider, diagnosticAdder, templateAttributeProperties );

            this.Builder.Type =
                (this.Template?.Declaration.Type ?? (INamedType?) this._addTemplate?.Declaration.Parameters.FirstOrDefault().AssertNotNull().Type)
                .AssertNotNull();

            if ( this.Template != null )
            {
                CopyTemplateAttributes( this.Template.Declaration.AddMethod, this.Builder.AddMethod, serviceProvider );

                CopyTemplateAttributes(
                    this.Template.Declaration.AddMethod.Parameters[0],
                    this.Builder.AddMethod.Parameters[0],
                    serviceProvider );

                CopyTemplateAttributes( this.Template.Declaration.AddMethod.ReturnParameter, this.Builder.AddMethod.ReturnParameter, serviceProvider );
                CopyTemplateAttributes( this.Template.Declaration.RemoveMethod, this.Builder.RemoveMethod, serviceProvider );

                CopyTemplateAttributes(
                    this.Template.Declaration.RemoveMethod.Parameters[0],
                    this.Builder.RemoveMethod.Parameters[0],
                    serviceProvider );

                CopyTemplateAttributes( this.Template.Declaration.RemoveMethod.ReturnParameter, this.Builder.RemoveMethod.ReturnParameter, serviceProvider );

                if ( this.Template.Declaration.GetSymbol().AssertNotNull().GetBackingField() is { } backingField )
                {
                    var classificationService = serviceProvider.GetRequiredService<AttributeClassificationService>();

                    // TODO: Currently Roslyn does not expose the event field in the symbol model and therefore we cannot find it.
                    foreach ( var attribute in backingField.GetAttributes() )
                    {
                        if ( classificationService.MustCopyTemplateAttribute( attribute ) )
                        {
                            this.Builder.AddFieldAttribute( new Attribute( attribute, this.SourceCompilation.GetCompilationModel(), this.Builder ) );
                        }
                    }
                }
            }

            if ( this._addTemplate != null )
            {
                CopyTemplateAttributes( this._addTemplate.Declaration, this.Builder.AddMethod, serviceProvider );

                CopyTemplateAttributes(
                    this._addTemplate.Declaration.Parameters[0],
                    this.Builder.AddMethod.Parameters[0],
                    serviceProvider );

                CopyTemplateAttributes( this._addTemplate.Declaration.ReturnParameter, this.Builder.AddMethod.ReturnParameter, serviceProvider );
            }

            if ( this._removeTemplate != null )
            {
                CopyTemplateAttributes( this._removeTemplate.Declaration, this.Builder.RemoveMethod, serviceProvider );

                CopyTemplateAttributes(
                    this._removeTemplate.Declaration.Parameters[0],
                    this.Builder.RemoveMethod.Parameters[0],
                    serviceProvider );

                CopyTemplateAttributes( this._removeTemplate.Declaration.ReturnParameter, this.Builder.RemoveMethod.ReturnParameter, serviceProvider );
            }

            // IMPORTANT: Template binding is the last step.
        }

        public override AdviceKind AdviceKind => AdviceKind.IntroduceEvent;

        public override AdviceImplementationResult Implement(
            ProjectServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // this.Tags: Override transformations.
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var existingDeclaration = targetDeclaration.FindClosestUniquelyNamedMember( this.Builder.Name );
            var hasNoOverrideSemantics = this.Template?.Declaration != null && this.Template.Declaration.IsEventField() == true;

            if ( existingDeclaration == null )
            {
                // TODO: validate event type.

                // There is no existing declaration, we will introduce and override the introduced.
                addTransformation( this.Builder.ToTransformation() );

                OverrideHelper.AddTransformationsForStructField( targetDeclaration, this, addTransformation );

                if ( !hasNoOverrideSemantics )
                {
                    addTransformation(
                        new OverrideEventTransformation(
                            this,
                            this.Builder,
                            this._addTemplate?.ForIntroductionFinal(this.Builder.AddMethod),
                            this._removeTemplate?.ForIntroductionFinal( this.Builder.RemoveMethod ),
                            this.Tags) );
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
                else if ( !compilation.Comparers.Default.Equals( this.Builder.Type, existingEvent.Type ) )
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
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
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
                                    this._addTemplate?.ForIntroductionFinal( this.Builder.AddMethod ),
                                    this._removeTemplate?.ForIntroductionFinal( this.Builder.RemoveMethod ),
                                    this.Tags );

                                addTransformation( overriddenMethod );

                                return AdviceImplementationResult.Success( AdviceOutcome.New );
                            }
                        }
                        else
                        {
                            this.Builder.IsNew = true;
                            this.Builder.OverriddenEvent = existingEvent;

                            if ( hasNoOverrideSemantics )
                            {
                                addTransformation( this.Builder.ToTransformation() );

                                return AdviceImplementationResult.Success( AdviceOutcome.New );
                            }
                            else
                            {
                                var overriddenMethod = new OverrideEventTransformation(
                                    this,
                                    this.Builder,
                                    this._addTemplate?.ForIntroductionFinal( this.Builder.AddMethod ),
                                    this._removeTemplate?.ForIntroductionFinal( this.Builder.RemoveMethod ),
                                    this.Tags );

                                addTransformation( this.Builder.ToTransformation() );
                                addTransformation( overriddenMethod );

                                return AdviceImplementationResult.Success( AdviceOutcome.New );
                            }
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
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
                                    this._addTemplate?.ForIntroductionFinal( this.Builder.AddMethod ),
                                    this._removeTemplate?.ForIntroductionFinal( this.Builder.RemoveMethod ),
                                    this.Tags );

                                addTransformation( overriddenMethod );

                                return AdviceImplementationResult.Success( AdviceOutcome.Override );
                            }
                        }
                        else if ( existingDeclaration.IsSealed || !existingDeclaration.IsOverridable() )
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
                                addTransformation( this.Builder.ToTransformation() );

                                return AdviceImplementationResult.Success( AdviceOutcome.Override );
                            }
                            else
                            {
                                var overriddenEvent = new OverrideEventTransformation(
                                    this,
                                    this.Builder,
                                    this._addTemplate?.ForIntroductionFinal( this.Builder.AddMethod ),
                                    this._removeTemplate?.ForIntroductionFinal( this.Builder.RemoveMethod ),
                                    this.Tags );

                                addTransformation( this.Builder.ToTransformation() );
                                addTransformation( overriddenEvent );

                                return AdviceImplementationResult.Success( AdviceOutcome.Override );
                            }
                        }

                    default:
                        throw new AssertionFailedException( $"Invalid value for OverrideStrategy: {this.OverrideStrategy}." );
                }
            }
        }
    }
}