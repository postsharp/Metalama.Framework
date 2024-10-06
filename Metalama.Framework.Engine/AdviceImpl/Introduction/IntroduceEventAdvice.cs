﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.Introductions.Helpers;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using System;
using Attribute = Metalama.Framework.Engine.CodeModel.Source.Attribute;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceEventAdvice : IntroduceMemberAdvice<IEvent, IEvent, EventBuilder>
{
    private readonly PartiallyBoundTemplateMethod? _addTemplate;
    private readonly PartiallyBoundTemplateMethod? _removeTemplate;

    public IntroduceEventAdvice(
        AdviceConstructorParameters<INamedType> parameters,
        string? explicitName,
        TemplateMember<IEvent>? eventTemplate,
        PartiallyBoundTemplateMethod? addTemplate,
        PartiallyBoundTemplateMethod? removeTemplate,
        IntroductionScope scope,
        OverrideStrategy overrideStrategy,
        Action<IEventBuilder>? buildAction,
        IObjectReader tags,
        INamedType? explicitlyImplementedInterfaceType )
        : base( parameters, explicitName, eventTemplate, scope, overrideStrategy, buildAction, tags, explicitlyImplementedInterfaceType )
    {
        this._addTemplate = addTemplate;
        this._removeTemplate = removeTemplate;
    }

    protected override EventBuilder CreateBuilder( in AdviceImplementationContext context )
    {
        return new EventBuilder(
            this,
            this.TargetDeclaration,
            this.MemberName,
            this.Template?.Declaration != null && this.Template.Declaration.IsEventField() == true,
            this.Tags ) { InitializerTemplate = this.Template.GetInitializerTemplate() };
    }

    protected override void InitializeBuilderCore(
        EventBuilder builder,
        TemplateAttributeProperties? templateAttributeProperties,
        in AdviceImplementationContext context )
    {
        base.InitializeBuilderCore( builder, templateAttributeProperties, in context );

        var serviceProvider = context.ServiceProvider;

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

                builder.Type = rewrittenNamedType;
            }
        }
        else if ( this.Template != null )
        {
            // Case for event fields.
            builder.Type = this.Template.Declaration.Type;
        }

        if ( this.Template != null )
        {
            if ( this.Template.Declaration.GetSymbol().AssertSymbolNotNull().GetBackingField() is { } backingField )
            {
                var classificationService = context.ServiceProvider.Global.GetRequiredService<AttributeClassificationService>();

                // TODO: Currently Roslyn does not expose the event field in the symbol model and therefore we cannot find it.
                foreach ( var attribute in backingField.GetAttributes() )
                {
                    if ( classificationService.MustCopyTemplateAttribute( attribute ) )
                    {
                        builder.AddFieldAttribute( new Attribute( attribute, this.SourceCompilation.GetCompilationModel(), builder ) );
                    }
                }
            }
        }

        if ( this._addTemplate != null )
        {
            AddAttributeForAccessorTemplate( this._addTemplate.TemplateMember.TemplateClassMember, this._addTemplate.Declaration, builder.AddMethod );
        }
        else if ( this.Template != null )
        {
            // Case for event fields.
            AddAttributeForAccessorTemplate(
                this.Template.TemplateClassMember,
                this.Template.AssertNotNull().Declaration.AddMethod,
                builder.AddMethod );
        }

        if ( this._removeTemplate != null )
        {
            AddAttributeForAccessorTemplate(
                this._removeTemplate.TemplateMember.TemplateClassMember,
                this._removeTemplate.Declaration,
                builder.RemoveMethod );
        }
        else if ( this.Template != null )
        {
            // Case for event fields.
            AddAttributeForAccessorTemplate(
                this.Template.TemplateClassMember,
                this.Template.AssertNotNull().Declaration.RemoveMethod,
                builder.RemoveMethod );
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

    protected override IntroductionAdviceResult<IEvent> ImplementCore( EventBuilder builder, in AdviceImplementationContext context )
    {
        builder.Freeze();
        
        // this.Tags: Override transformations.
        var targetDeclaration = this.TargetDeclaration;

        var existingDeclaration = targetDeclaration.FindClosestUniquelyNamedMember( builder.Name );
        var hasNoOverrideSemantics = this.Template?.Declaration != null && this.Template.Declaration.IsEventField() == true;

        if ( existingDeclaration == null )
        {
            // TODO: validate event type.

            // There is no existing declaration, we will introduce and override the introduced.
            context.AddTransformation( builder.ToTransformation() );

            OverrideHelper.AddTransformationsForStructField( targetDeclaration, this, context.AddTransformation );

            if ( !hasNoOverrideSemantics )
            {
                context.AddTransformation(
                    new OverrideEventTransformation(
                        this,
                        builder.ToRef(),
                        this._addTemplate?.ForIntroduction( builder.AddMethod ),
                        this._removeTemplate?.ForIntroduction( builder.RemoveMethod ),
                        this.Tags ) );
            }

            return this.CreateSuccessResult( AdviceOutcome.Default, builder );
        }
        else
        {
            if ( existingDeclaration is not IEvent existingEvent )
            {
                return this.CreateFailedResult(
                    AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration, existingDeclaration.DeclarationKind),
                        this ) );
            }
            else if ( existingEvent.IsStatic != builder.IsStatic )
            {
                return this.CreateFailedResult(
                    AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration,
                         existingEvent.DeclaringType),
                        this ) );
            }
            else if ( !builder.Type.Equals( existingEvent.Type ) )
            {
                return this.CreateFailedResult(
                    AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration,
                         existingEvent.DeclaringType, existingEvent.Type),
                        this ) );
            }

            switch ( this.OverrideStrategy )
            {
                case OverrideStrategy.Fail:
                    // Produce fail diagnostic.
                    return this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration,
                             existingEvent.DeclaringType),
                            this ) );

                case OverrideStrategy.Ignore:
                    // Do nothing.
                    return this.CreateIgnoredResult( existingEvent );

                case OverrideStrategy.New:
                    // If the existing declaration is in the current type, fail, otherwise, declare a new method and override.
                    if ( targetDeclaration.Equals( existingEvent.DeclaringType ) )
                    {
                        return this.CreateFailedResult(
                            AdviceDiagnosticDescriptors.CannotIntroduceNewMemberWhenItAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.AspectInstance.AspectClass.ShortName, builder, existingEvent.DeclaringType),
                                this ) );
                    }
                    else
                    {
                        builder.HasNewKeyword = builder.IsNew = true;
                        builder.OverriddenEvent = existingEvent;

                        if ( hasNoOverrideSemantics )
                        {
                            context.AddTransformation( builder.ToTransformation() );

                            return this.CreateSuccessResult( AdviceOutcome.New, builder );
                        }
                        else
                        {
                            var overriddenMethod = new OverrideEventTransformation(
                                this,
                                builder.ToRef(),
                                this._addTemplate?.ForIntroduction( builder.AddMethod ),
                                this._removeTemplate?.ForIntroduction( builder.RemoveMethod ),
                                this.Tags );

                            context.AddTransformation( builder.ToTransformation() );
                            context.AddTransformation( overriddenMethod );

                            return this.CreateSuccessResult( AdviceOutcome.New, builder );
                        }
                    }

                case OverrideStrategy.Override:
                    if ( targetDeclaration.Equals( existingEvent.DeclaringType ) )
                    {
                        if ( hasNoOverrideSemantics )
                        {
                            return this.CreateIgnoredResult( existingEvent );
                        }
                        else
                        {
                            var overriddenMethod = new OverrideEventTransformation(
                                this,
                                existingEvent.ToRef(),
                                this._addTemplate?.ForIntroduction( existingEvent.AddMethod ),
                                this._removeTemplate?.ForIntroduction( existingEvent.RemoveMethod ),
                                this.Tags );

                            context.AddTransformation( overriddenMethod );

                            return this.CreateSuccessResult( AdviceOutcome.Override, existingEvent );
                        }
                    }
                    else if ( existingEvent.IsSealed || !existingEvent.IsOverridable() )
                    {
                        return
                            this.CreateFailedResult(
                                AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration,
                                     existingEvent.DeclaringType),
                                    this ) );
                    }
                    else
                    {
                        builder.IsOverride = true;
                        builder.OverriddenEvent = existingEvent;

                        if ( hasNoOverrideSemantics )
                        {
                            context.AddTransformation( builder.ToTransformation() );

                            return this.CreateSuccessResult( AdviceOutcome.Override, builder );
                        }
                        else
                        {
                            var overriddenEvent = new OverrideEventTransformation(
                                this,
                                builder.ToRef(),
                                this._addTemplate?.ForIntroduction( builder.AddMethod ),
                                this._removeTemplate?.ForIntroduction( builder.RemoveMethod ),
                                this.Tags );

                            context.AddTransformation( builder.ToTransformation() );
                            context.AddTransformation( overriddenEvent );

                            return this.CreateSuccessResult( AdviceOutcome.Override, builder );
                        }
                    }

                default:
                    throw new AssertionFailedException( $"Invalid value for OverrideStrategy: {this.OverrideStrategy}." );
            }
        }
    }
}