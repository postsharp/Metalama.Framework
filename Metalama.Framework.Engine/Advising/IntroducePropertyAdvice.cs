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

namespace Metalama.Framework.Engine.Advising;

internal sealed class IntroducePropertyAdvice : IntroduceMemberAdvice<IProperty, PropertyBuilder>
{
    private readonly PartiallyBoundTemplateMethod? _getTemplate;
    private readonly PartiallyBoundTemplateMethod? _setTemplate;
    private readonly bool _isProgrammaticAutoProperty;

    public IntroducePropertyAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance templateInstance,
        INamedType targetDeclaration,
        ICompilation sourceCompilation,
        string? explicitName,
        IType? explicitType,
        TemplateMember<IProperty>? propertyTemplate,
        PartiallyBoundTemplateMethod? getTemplate,
        PartiallyBoundTemplateMethod? setTemplate,
        IntroductionScope scope,
        OverrideStrategy overrideStrategy,
        Action<IPropertyBuilder>? buildAction,
        string? layerName,
        IObjectReader tags )
        : base(
            aspect,
            templateInstance,
            targetDeclaration,
            sourceCompilation,
            explicitName,
            propertyTemplate,
            scope,
            overrideStrategy,
            buildAction,
            layerName,
            tags )
    {
        this._getTemplate = getTemplate;
        this._setTemplate = setTemplate;
        this._isProgrammaticAutoProperty = propertyTemplate == null && getTemplate == null && setTemplate == null;

        var templatePropertyDeclaration = propertyTemplate?.Declaration;
        var name = this.MemberName;

        var hasGet = this._isProgrammaticAutoProperty
                     || (templatePropertyDeclaration != null ? templatePropertyDeclaration.GetMethod != null : getTemplate != null);

        var hasSet = this._isProgrammaticAutoProperty
                     || (templatePropertyDeclaration != null ? templatePropertyDeclaration.SetMethod != null : setTemplate != null);

        this.Builder = new PropertyBuilder(
            this,
            targetDeclaration,
            name,
            hasGet,
            hasSet,
            this._isProgrammaticAutoProperty || templatePropertyDeclaration is { IsAutoPropertyOrField: true },
            templatePropertyDeclaration is { Writeability: Writeability.InitOnly },
            false,
            templatePropertyDeclaration is { Writeability: Writeability.ConstructorOnly, IsAutoPropertyOrField: true },
            this.Tags );

        if ( explicitType != null )
        {
            this.Builder.Type = explicitType;
        }

        this.Builder.InitializerTemplate = propertyTemplate?.GetInitializerTemplate();
    }

    protected override void InitializeCore(
        ProjectServiceProvider serviceProvider,
        IDiagnosticAdder diagnosticAdder,
        TemplateAttributeProperties? templateAttributeProperties )
    {
        base.InitializeCore( serviceProvider, diagnosticAdder, templateAttributeProperties );

        this.Builder.IsRequired = templateAttributeProperties?.IsRequired ?? this.Template?.Declaration.IsRequired ?? false;

        if ( !this._isProgrammaticAutoProperty )
        {
            if ( this._getTemplate != null )
            {
                var typeRewriter = TemplateTypeRewriter.Get( this._getTemplate );

                this.Builder.Type = typeRewriter.Visit( this._getTemplate.Declaration.ReturnType );
            }
            else if ( this._setTemplate != null )
            {
                var runtimeParameters = this._setTemplate.TemplateMember.TemplateClassMember.RunTimeParameters;

                var typeRewriter = TemplateTypeRewriter.Get( this._setTemplate );

                if ( runtimeParameters.Length > 0 )
                {
                    // There may be an invalid template without runtime parameters, in which case type cannot be determined.

                    this.Builder.Type = typeRewriter.Visit( this._setTemplate.Declaration.Parameters[runtimeParameters[0].SourceIndex].Type );
                }
            }
            else if ( this.Template != null )
            {
                // Case for event fields.
                this.Builder.Type = this.Template.Declaration.Type;
            }            

            this.Builder.Accessibility =
                this.Template?.Accessibility ?? (this._getTemplate != null
                    ? this._getTemplate.TemplateMember.Accessibility
                    : this._setTemplate?.TemplateMember.Accessibility).AssertNotNull();

            if ( this.Builder.GetMethod != null )
            {
                ((AccessorBuilder) this.Builder.GetMethod).SetIsIteratorMethod(
                    this.Template?.IsIteratorMethod ?? this._getTemplate?.TemplateMember.IsIteratorMethod ?? false );
            }

            if ( this.Template != null )
            {
                if ( this.Template.Declaration.AssertNotNull().GetMethod != null )
                {
                    this.Builder.GetMethod.AssertNotNull().Accessibility = this.Template.GetAccessorAccessibility;
                }

                if ( this.Template.Declaration.AssertNotNull().SetMethod != null )
                {
                    this.Builder.SetMethod.AssertNotNull().Accessibility = this.Template.SetAccessorAccessibility;
                }

                if ( this.Template.Declaration.GetSymbol().AssertNotNull().GetBackingField() is { } backingField )
                {
                    var classificationService = serviceProvider.Global.GetRequiredService<AttributeClassificationService>();

                    foreach ( var attribute in backingField.GetAttributes() )
                    {
                        if ( classificationService.MustCopyTemplateAttribute( attribute ) )
                        {
                            this.Builder.AddFieldAttribute( new Attribute( attribute, this.SourceCompilation.GetCompilationModel(), this.Builder ) );
                        }
                    }
                }
            }
        }

        if ( this.Builder.GetMethod != null )
        {
            if ( this._getTemplate != null )
            {
                AddAttributeForAccessorTemplate( this._getTemplate.TemplateMember.TemplateClassMember, this._getTemplate.Declaration, this.Builder.GetMethod );
            }
            else if ( this.Template?.Declaration.GetMethod != null )
            {
                AddAttributeForAccessorTemplate( this.Template.TemplateClassMember, this.Template.Declaration.GetMethod, this.Builder.GetMethod );
            }
        }

        if ( this.Builder.SetMethod != null )
        {
            if ( this._setTemplate != null )
            {
                AddAttributeForAccessorTemplate( this._setTemplate.TemplateMember.TemplateClassMember, this._setTemplate.Declaration, this.Builder.SetMethod );
            }
            else if ( this.Template?.Declaration.SetMethod != null )
            {
                AddAttributeForAccessorTemplate( this.Template.TemplateClassMember, this.Template.Declaration.SetMethod, this.Builder.SetMethod );
            }
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

        // TODO: For get accessor template, we are ignoring accessibility of set accessor template because it can be easily incompatible.
    }

    public override AdviceKind AdviceKind => AdviceKind.IntroduceProperty;

    public override AdviceImplementationResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        var existingDeclaration = targetDeclaration.FindClosestUniquelyNamedMember( this.Builder.Name );

        var isAutoProperty = this.Template?.Declaration is { IsAutoPropertyOrField: true };

        // TODO: Introduce attributes that are added not present on the existing member?
        if ( existingDeclaration == null )
        {
            // There is no existing declaration.
            if ( isAutoProperty )
            {
                // Introduced auto property.
                addTransformation( this.Builder.ToTransformation() );

                OverrideHelper.AddTransformationsForStructField( targetDeclaration.ForCompilation( compilation ), this, addTransformation );

                return AdviceImplementationResult.Success( this.Builder );
            }
            else
            {
                // Introduce and override using the template.
                var overriddenProperty = new OverridePropertyTransformation(
                    this,
                    this.Builder,
                    this._getTemplate?.ForIntroduction( this.Builder.GetMethod ),
                    this._setTemplate?.ForIntroduction( this.Builder.SetMethod ),
                    this.Tags );

                addTransformation( this.Builder.ToTransformation() );
                addTransformation( overriddenProperty );

                return AdviceImplementationResult.Success( this.Builder );
            }
        }
        else
        {
            if ( existingDeclaration is not IProperty existingProperty )
            {
                return
                    AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration, existingDeclaration.DeclarationKind) ) );
            }

            if ( existingProperty.IsStatic != this.Builder.IsStatic )
            {
                return
                    AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                             existingProperty.DeclaringType) ) );
            }
            else if ( !compilation.Comparers.Default.Equals( this.Builder.Type, existingProperty.Type ) )
            {
                return
                    AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                             existingProperty.DeclaringType, existingProperty.Type) ) );
            }

            switch ( this.OverrideStrategy )
            {
                case OverrideStrategy.Fail:
                    // Produce fail diagnostic.
                    return
                        AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                 existingProperty.DeclaringType) ) );

                case OverrideStrategy.Ignore:
                    // Do nothing.
                    return AdviceImplementationResult.Ignored;

                case OverrideStrategy.New:
                    // If the existing declaration is in the current type, fail, otherwise, declare a new method and override.
                    if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingProperty.DeclaringType ) )
                    {
                        return AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceNewMemberWhenItAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, existingProperty.DeclaringType) ) );
                    }
                    else
                    {
                        this.Builder.IsNew = true;
                        this.Builder.OverriddenProperty = existingProperty;

                        var overriddenProperty = new OverridePropertyTransformation(
                            this,
                            this.Builder,
                            this._getTemplate?.ForIntroduction( this.Builder.GetMethod ),
                            this._setTemplate?.ForIntroduction( this.Builder.SetMethod ),
                            this.Tags );

                        addTransformation( this.Builder.ToTransformation() );
                        addTransformation( overriddenProperty );

                        return AdviceImplementationResult.Success( AdviceOutcome.New, this.Builder );
                    }

                case OverrideStrategy.Override:
                    if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingProperty.DeclaringType ) )
                    {
                        var overriddenMethod = new OverridePropertyTransformation(
                            this,
                            existingProperty,
                            this._getTemplate?.ForIntroduction( existingProperty.GetMethod ), 
                            this._setTemplate?.ForIntroduction( existingProperty.SetMethod ),
                            this.Tags );

                        addTransformation( overriddenMethod );

                        return AdviceImplementationResult.Success( AdviceOutcome.Override, existingProperty );
                    }
                    else if ( existingProperty.IsSealed || !existingProperty.IsOverridable() )
                    {
                        return
                            AdviceImplementationResult.Failed(
                                AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                     existingProperty.DeclaringType) ) );
                    }
                    else
                    {
                        this.Builder.IsOverride = true;
                        this.Builder.OverriddenProperty = existingProperty;

                        addTransformation( this.Builder.ToTransformation() );

                        OverrideHelper.OverrideProperty(
                            serviceProvider,
                            this,
                            this.Builder,
                            this._getTemplate?.ForIntroduction( this.Builder.GetMethod ),
                            this._setTemplate?.ForIntroduction( this.Builder.SetMethod ),
                            this.Tags,
                            addTransformation );

                        return AdviceImplementationResult.Success( AdviceOutcome.Override, this.Builder );
                    }

                default:
                    throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
            }
        }
    }
}