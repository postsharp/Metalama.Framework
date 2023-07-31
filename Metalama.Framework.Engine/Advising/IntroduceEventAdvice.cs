// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using System;
using System.Collections.Generic;
using Attribute = Metalama.Framework.Engine.CodeModel.Attribute;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class IntroduceEventAdvice : IntroduceMemberAdvice<IEvent, EventBuilder>
    {
        private readonly PartiallyBoundTemplateMethod? _addTemplate;
        private readonly PartiallyBoundTemplateMethod? _removeTemplate;

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

            if ( this._addTemplate != null || this._removeTemplate != null )
            {
                var primaryTemplate = (this._addTemplate ?? this._removeTemplate).AssertNotNull();
                var runtimeParameters = primaryTemplate.TemplateMember.TemplateClassMember.RunTimeParameters;

                var typeRewriter = TemplateTypeRewriter.Get( primaryTemplate );

                if ( runtimeParameters.Length > 0 )
                {
                    // There may be an invalid template without runtime parameters, in which case type cannot be determined.

                    var rewrittenType = typeRewriter.Visit( primaryTemplate.Declaration.Parameters[runtimeParameters[0].SourceIndex].Type );

                    if ( rewrittenType is not INamedType rewrittenNamedType )
                    {
                        throw new AssertionFailedException( $"'{rewrittenType}' is not allowed type of an event." );
                    }

                    this.Builder.Type = rewrittenNamedType;
                }
            }
            else if ( this.Template != null )
            {
                // Case for event fields.
                this.Builder.Type = this.Template.Declaration.Type;
            }

            if ( this.Template != null )
            {
                if ( this.Template.Declaration.GetSymbol().AssertNotNull().GetBackingField() is { } backingField )
                {
                    var classificationService = serviceProvider.Global.GetRequiredService<AttributeClassificationService>();

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
                AddAttributeForAccessorTemplate( this._addTemplate.TemplateMember.TemplateClassMember, this._addTemplate.Declaration, this.Builder.AddMethod );
            }
            else if ( this.Template != null )
            {
                // Case for event fields.
                AddAttributeForAccessorTemplate(
                    this.Template.TemplateClassMember,
                    this.Template.AssertNotNull().Declaration.AddMethod,
                    this.Builder.AddMethod );
            }

            if ( this._removeTemplate != null )
            {
                AddAttributeForAccessorTemplate(
                    this._removeTemplate.TemplateMember.TemplateClassMember,
                    this._removeTemplate.Declaration,
                    this.Builder.RemoveMethod );
            }
            else if ( this.Template != null )
            {
                // Case for event fields.
                AddAttributeForAccessorTemplate(
                    this.Template.TemplateClassMember,
                    this.Template.AssertNotNull().Declaration.RemoveMethod,
                    this.Builder.RemoveMethod );
            }

            void AddAttributeForAccessorTemplate( TemplateClassMember templateClassMember, IMethod accessorTemplate, IMethodBuilder accessorBuilder )
            {
                CopyTemplateAttributes( accessorTemplate, accessorBuilder, serviceProvider );

                if ( accessorBuilder.Parameters.Count > 0 && templateClassMember.RunTimeParameters.Length > 0 )
                {
                    // There may be an invalid template without runtime parameters, in which case attributes cannot be copied.
                    CopyTemplateAttributes(
                        accessorTemplate.Parameters[templateClassMember.RunTimeParameters[0].SourceIndex],
                        accessorBuilder.Parameters[0],
                        serviceProvider );
                }

                CopyTemplateAttributes( accessorTemplate.ReturnParameter, accessorBuilder.ReturnParameter, serviceProvider );
            }
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

                OverrideHelper.AddTransformationsForStructField( targetDeclaration.ForCompilation( compilation ), this, addTransformation );

                if ( !hasNoOverrideSemantics )
                {
                    addTransformation(
                        new OverrideEventTransformation(
                            this,
                            this.Builder,
                            this._addTemplate?.ForIntroduction( this.Builder.AddMethod ),
                            this._removeTemplate?.ForIntroduction( this.Builder.RemoveMethod ),
                            this.Tags ) );
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
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration, existingDeclaration.DeclarationKind),
                            this ) );
                }
                else if ( existingEvent.IsStatic != this.Builder.IsStatic )
                {
                    return AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                             existingEvent.DeclaringType),
                            this ) );
                }
                else if ( !compilation.Comparers.Default.Equals( this.Builder.Type, existingEvent.Type ) )
                {
                    return AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                             existingEvent.DeclaringType, existingEvent.Type),
                            this ) );
                }

                switch ( this.OverrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Produce fail diagnostic.
                        return AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                 existingEvent.DeclaringType),
                                this ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceImplementationResult.Ignored( existingEvent );

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingEvent.DeclaringType ) )
                        {
                            return AdviceImplementationResult.Failed(
                                AdviceDiagnosticDescriptors.CannotIntroduceNewMemberWhenItAlreadyExists.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, this.Builder, existingEvent.DeclaringType),
                                    this ) );
                        }
                        else
                        {
                            this.Builder.HasNewKeyword = this.Builder.IsNew = true;
                            this.Builder.OverriddenEvent = existingEvent;

                            if ( hasNoOverrideSemantics )
                            {
                                addTransformation( this.Builder.ToTransformation() );

                                return AdviceImplementationResult.Success( AdviceOutcome.New, this.Builder );
                            }
                            else
                            {
                                var overriddenMethod = new OverrideEventTransformation(
                                    this,
                                    this.Builder,
                                    this._addTemplate?.ForIntroduction( this.Builder.AddMethod ),
                                    this._removeTemplate?.ForIntroduction( this.Builder.RemoveMethod ),
                                    this.Tags );

                                addTransformation( this.Builder.ToTransformation() );
                                addTransformation( overriddenMethod );

                                return AdviceImplementationResult.Success( AdviceOutcome.New, this.Builder );
                            }
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingEvent.DeclaringType ) )
                        {
                            if ( hasNoOverrideSemantics )
                            {
                                return AdviceImplementationResult.Ignored( existingEvent );
                            }
                            else
                            {
                                var overriddenMethod = new OverrideEventTransformation(
                                    this,
                                    existingEvent,
                                    this._addTemplate?.ForIntroduction( existingEvent.AddMethod ),
                                    this._removeTemplate?.ForIntroduction( existingEvent.RemoveMethod ),
                                    this.Tags );

                                addTransformation( overriddenMethod );

                                return AdviceImplementationResult.Success( AdviceOutcome.Override, existingEvent );
                            }
                        }
                        else if ( existingEvent.IsSealed || !existingEvent.IsOverridable() )
                        {
                            return
                                AdviceImplementationResult.Failed(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                        targetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                         existingEvent.DeclaringType),
                                        this ) );
                        }
                        else
                        {
                            this.Builder.IsOverride = true;
                            this.Builder.OverriddenEvent = existingEvent;

                            if ( hasNoOverrideSemantics )
                            {
                                addTransformation( this.Builder.ToTransformation() );

                                return AdviceImplementationResult.Success( AdviceOutcome.Override, this.Builder );
                            }
                            else
                            {
                                var overriddenEvent = new OverrideEventTransformation(
                                    this,
                                    this.Builder,
                                    this._addTemplate?.ForIntroduction( this.Builder.AddMethod ),
                                    this._removeTemplate?.ForIntroduction( this.Builder.RemoveMethod ),
                                    this.Tags );

                                addTransformation( this.Builder.ToTransformation() );
                                addTransformation( overriddenEvent );

                                return AdviceImplementationResult.Success( AdviceOutcome.Override, this.Builder );
                            }
                        }

                    default:
                        throw new AssertionFailedException( $"Invalid value for OverrideStrategy: {this.OverrideStrategy}." );
                }
            }
        }
    }
}