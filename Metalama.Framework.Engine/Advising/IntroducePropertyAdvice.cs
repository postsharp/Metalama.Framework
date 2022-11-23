// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising
{
    internal class IntroducePropertyAdvice : IntroduceMemberAdvice<IProperty, PropertyBuilder>
    {
        private readonly BoundTemplateMethod? _getTemplate;
        private readonly BoundTemplateMethod? _setTemplate;
        private readonly bool _isProgrammaticAutoProperty;

        public IntroducePropertyAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            ICompilation sourceCompilation,
            string? explicitName,
            IType? explicitType,
            TemplateMember<IProperty>? propertyTemplate,
            BoundTemplateMethod? getTemplate,
            BoundTemplateMethod? setTemplate,
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

        protected override void InitializeCore( ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            base.InitializeCore( serviceProvider, diagnosticAdder );

            if ( !this._isProgrammaticAutoProperty )
            {
                this.Builder.Type = (this.Template?.Declaration.Type ?? this._getTemplate?.Template.Declaration.ReturnType).AssertNotNull();

                this.Builder.Accessibility =
                    this.Template?.Accessibility ?? (this._getTemplate != null
                        ? this._getTemplate.Template.Accessibility
                        : this._setTemplate?.Template.Accessibility).AssertNotNull();

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
                }
            }

            if ( this._getTemplate != null )
            {
                CopyTemplateAttributes( this._getTemplate.Template.Declaration, this.Builder.GetMethod!, serviceProvider );
                CopyTemplateAttributes( this._getTemplate.Template.Declaration.ReturnParameter, this.Builder.GetMethod!.ReturnParameter, serviceProvider );
            }

            if ( this._setTemplate != null )
            {
                CopyTemplateAttributes( this._setTemplate.Template.Declaration, this.Builder.SetMethod!, serviceProvider );

                CopyTemplateAttributes(
                    this._setTemplate.Template.Declaration.Parameters[0],
                    (IDeclarationBuilder) this.Builder.SetMethod!.Parameters[0],
                    serviceProvider );

                CopyTemplateAttributes( this._setTemplate.Template.Declaration.ReturnParameter, this.Builder.SetMethod.ReturnParameter, serviceProvider );
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

                    OverrideHelper.AddTransformationsForStructField( targetDeclaration, this, addTransformation );

                    return AdviceImplementationResult.Success( this.Builder );
                }
                else
                {
                    // Introduce and override using the template.
                    var overriddenProperty = new OverridePropertyTransformation(
                        this,
                        this.Builder,
                        this._getTemplate,
                        this._setTemplate,
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

                if ( existingDeclaration.IsStatic != this.Builder.IsStatic )
                {
                    return
                        AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                 existingDeclaration.DeclaringType) ) );
                }
                else if ( !compilation.Comparers.Default.Equals( this.Builder.Type, existingProperty.Type ) )
                {
                    return
                        AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                 existingDeclaration.DeclaringType, existingProperty.Type) ) );
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
                                     existingDeclaration.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceImplementationResult.Ignored;

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, we fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenProperty = new OverridePropertyTransformation(
                                this,
                                existingProperty,
                                this._getTemplate,
                                this._setTemplate,
                                this.Tags );

                            addTransformation( overriddenProperty );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override );
                        }
                        else
                        {
                            this.Builder.IsNew = true;
                            this.Builder.OverriddenProperty = existingProperty;

                            var overriddenProperty = new OverridePropertyTransformation(
                                this,
                                this.Builder,
                                this._getTemplate,
                                this._setTemplate,
                                this.Tags );

                            addTransformation( this.Builder.ToTransformation() );
                            addTransformation( overriddenProperty );

                            return AdviceImplementationResult.Success( AdviceOutcome.New, this.Builder );
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverridePropertyTransformation(
                                this,
                                existingProperty,
                                this._getTemplate,
                                this._setTemplate,
                                this.Tags );

                            addTransformation( overriddenMethod );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override );
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
                            this.Builder.OverriddenProperty = existingProperty;

                            addTransformation( this.Builder.ToTransformation() );

                            OverrideHelper.OverrideProperty(
                                serviceProvider,
                                this,
                                this.Builder,
                                this._getTemplate,
                                this._setTemplate,
                                this.Tags,
                                addTransformation );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override );
                        }

                    default:
                        throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
                }
            }
        }
    }
}