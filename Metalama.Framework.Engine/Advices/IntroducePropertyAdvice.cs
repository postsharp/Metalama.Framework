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
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Advices
{
    internal class IntroducePropertyAdvice : IntroduceMemberAdvice<IProperty, PropertyBuilder>
    {
        private readonly BoundTemplateMethod _getTemplate;
        private readonly BoundTemplateMethod _setTemplate;

        public IPropertyBuilder Builder => this.MemberBuilder;

        public IntroducePropertyAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            string? explicitName,
            TemplateMember<IProperty> propertyTemplate,
            BoundTemplateMethod getTemplate,
            BoundTemplateMethod setTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            IObjectReader tags )
            : base( aspect, templateInstance, targetDeclaration,  explicitName, propertyTemplate, scope, overrideStrategy, layerName, tags )
        {
            this._getTemplate = getTemplate;
            this._setTemplate = setTemplate;

            var templatePropertyDeclaration = propertyTemplate.Declaration;
            var name = this.MemberName;
            var hasGet = templatePropertyDeclaration != null ? templatePropertyDeclaration.GetMethod != null : getTemplate.IsNotNull;
            var hasSet = templatePropertyDeclaration != null ? templatePropertyDeclaration.SetMethod != null : setTemplate.IsNotNull;

            this.MemberBuilder = new PropertyBuilder(
                this,
                targetDeclaration,
                name,
                hasGet,
                hasSet,
                this.Template.Declaration is { IsAutoPropertyOrField: true },
                this.Template.Declaration is { Writeability: Writeability.InitOnly },
                false,
                this.Template.Declaration is { Writeability: Writeability.ConstructorOnly } && this.Template.Declaration.IsAutoPropertyOrField,
                this.Tags );

            this.MemberBuilder.InitializerTemplate = propertyTemplate.GetInitializerTemplate();
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( diagnosticAdder );

            // TODO: Indexers.

            this.MemberBuilder.Type = (this.Template.Declaration?.Type ?? this._getTemplate.Template.Declaration?.ReturnType).AssertNotNull();

            this.MemberBuilder.Accessibility =
                (this.Template.Declaration?.Accessibility
                 ?? this._getTemplate.Template.Declaration?.Accessibility ?? this._setTemplate.Template.Declaration?.Accessibility).AssertNotNull();

            if ( this.Template.IsNotNull )
            {
                if ( this.Template.Declaration.AssertNotNull().GetMethod != null )
                {
                    this.MemberBuilder.GetMethod.AssertNotNull().Accessibility = this.Template.Declaration!.GetMethod!.Accessibility;
                }

                if ( this.Template.Declaration.AssertNotNull().SetMethod != null )
                {
                    this.MemberBuilder.SetMethod.AssertNotNull().Accessibility = this.Template.Declaration!.SetMethod!.Accessibility;
                }
            }

            // TODO: For get accessor template, we are ignoring accessibility of set accessor template because it can be easily incompatible.
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var existingDeclaration = targetDeclaration.FindClosestUniquelyNamedMember( this.MemberBuilder.Name );

            var hasNoOverrideSemantics = this.Template.Declaration != null && this.Template.Declaration.IsAutoPropertyOrField;

            // TODO: Introduce attributes that are added not present on the existing member?
            if ( existingDeclaration == null )
            {
                // There is no existing declaration.
                if ( hasNoOverrideSemantics )
                {
                    // Introduced auto property.
                    return AdviceResult.Create( this.MemberBuilder );
                }
                else
                {
                    // Introduce and override using the template.
                    var overriddenProperty = new OverridePropertyTransformation(
                        this,
                        this.MemberBuilder,
                        this._getTemplate,
                        this._setTemplate,
                        this.Tags );

                    return AdviceResult.Create( this.MemberBuilder, overriddenProperty );
                }
            }
            else
            {
                if ( existingDeclaration is not IProperty existingProperty )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration, existingDeclaration.DeclarationKind) ) );
                }

                if ( existingDeclaration.IsStatic != this.MemberBuilder.IsStatic )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                 existingDeclaration.DeclaringType) ) );
                }
                else if ( !compilation.InvariantComparer.Equals( this.Builder.Type, existingProperty.Type ) )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                 existingDeclaration.DeclaringType, existingProperty.Type) ) );
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
                            var overriddenProperty = new OverridePropertyTransformation(
                                this,
                                existingProperty,
                                this._getTemplate,
                                this._setTemplate,
                                this.Tags );

                            return AdviceResult.Create( overriddenProperty );
                        }
                        else
                        {
                            this.MemberBuilder.IsNew = true;

                            var overriddenProperty = new OverridePropertyTransformation(
                                this,
                                this.MemberBuilder,
                                this._getTemplate,
                                this._setTemplate,
                                this.Tags );

                            return AdviceResult.Create( this.MemberBuilder, overriddenProperty );
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverridePropertyTransformation(
                                this,
                                existingProperty,
                                this._getTemplate,
                                this._setTemplate,
                                this.Tags );

                            return AdviceResult.Create( overriddenMethod );
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
                        else
                        {
                            this.MemberBuilder.IsOverride = true;
                            this.MemberBuilder.OverriddenProperty = existingProperty;

                            var overrideTransformations =
                                OverrideHelper.OverrideProperty(
                                    this,
                                    this.MemberBuilder,
                                    this._getTemplate,
                                    this._setTemplate,
                                    this.Tags );

                            return AdviceResult.Create( new ITransformation[] { this.MemberBuilder }.Concat( overrideTransformations ) );
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}