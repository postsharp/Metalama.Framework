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
        private readonly IObjectReader? _args;

        private readonly TemplateMember<IMethod> _getTemplate;
        private readonly TemplateMember<IMethod> _setTemplate;

        public IPropertyBuilder Builder => this.MemberBuilder;

        public IntroducePropertyAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            string? explicitName,
            TemplateMember<IProperty> propertyTemplate,
            TemplateMember<IMethod> getTemplate,
            TemplateMember<IMethod> setTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            IObjectReader tags,
            IObjectReader? args = null )
            : base( aspect, templateInstance, targetDeclaration, propertyTemplate, scope, overrideStrategy, layerName, tags )
        {
            this._args = args;
            this._getTemplate = getTemplate;
            this._setTemplate = setTemplate;

            var templatePropertyDeclaration = propertyTemplate.Declaration;
            var name = templatePropertyDeclaration?.Name ?? explicitName ?? throw new AssertionFailedException();
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
                this.Tags );

            if ( propertyTemplate.IsNotNull )
            {
                this.MemberBuilder.ApplyTemplateAttribute( propertyTemplate.TemplateInfo.Attribute );
            }

            this.MemberBuilder.InitializerTemplate = propertyTemplate.GetInitializerTemplate();
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( diagnosticAdder );

            // TODO: Indexers.

            this.MemberBuilder.Type = (this.Template.Declaration?.Type ?? this._getTemplate.Declaration?.ReturnType).AssertNotNull();

            this.MemberBuilder.Accessibility =
                (this.Template.Declaration?.Accessibility
                 ?? this._getTemplate.Declaration?.Accessibility ?? this._setTemplate.Declaration?.Accessibility).AssertNotNull();

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

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var existingDeclaration = targetDeclaration.FindClosestVisibleProperty(
                this.MemberBuilder,
                observableTransformations.OfType<IProperty>().ToList() );

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
                        this._getTemplate.ForIntroduction(this._args),
                        this._setTemplate.ForIntroduction( this._args ),
                        this.Tags );

                    return AdviceResult.Create( this.MemberBuilder, overriddenProperty );
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
                            var overriddenProperty = new OverridePropertyTransformation(
                                this,
                                existingDeclaration,
                                this._getTemplate.ForIntroduction( this._args ),
                                this._setTemplate.ForIntroduction( this._args ),
                                this.Tags );

                            return AdviceResult.Create( overriddenProperty );
                        }
                        else
                        {
                            this.MemberBuilder.IsNew = true;

                            var overriddenProperty = new OverridePropertyTransformation(
                                this,
                                this.MemberBuilder,
                                this._getTemplate.ForIntroduction( this._args ),
                                this._setTemplate.ForIntroduction( this._args ),
                                this.Tags );

                            return AdviceResult.Create( this.MemberBuilder, overriddenProperty );
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverridePropertyTransformation(
                                this,
                                existingDeclaration,
                                this._getTemplate.ForIntroduction( this._args ),
                                this._setTemplate.ForIntroduction( this._args ),
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
                            this.MemberBuilder.OverriddenProperty = existingDeclaration;

                            var overrideTransformations =
                                OverrideHelper.OverrideProperty(
                                    this,
                                    this.MemberBuilder,
                                    this._getTemplate,
                                    this._setTemplate,
                                    ( t, m ) => t.ForIntroduction( this._args ),
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