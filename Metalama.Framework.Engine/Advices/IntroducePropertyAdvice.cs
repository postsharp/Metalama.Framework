// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advices
{
    internal class IntroducePropertyAdvice : IntroduceMemberAdvice<IProperty, PropertyBuilder>
    {
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
            Dictionary<string, object?>? tags )
            : base( aspect, templateInstance, targetDeclaration, propertyTemplate, scope, overrideStrategy, layerName, tags )
        {
            this._getTemplate = getTemplate;
            this._setTemplate = setTemplate;

            var templatePropertyDeclaration = propertyTemplate.Declaration;
            var name = templatePropertyDeclaration?.Name ?? explicitName ?? throw new AssertionFailedException();
            var hasGet = templatePropertyDeclaration != null ? templatePropertyDeclaration.GetMethod != null : getTemplate.IsNotNull;
            var hasSet = templatePropertyDeclaration != null ? templatePropertyDeclaration.SetMethod != null : setTemplate.IsNotNull;

            this.MemberBuilder = new PropertyBuilder(
                this,
                this.TargetDeclaration,
                name,
                hasGet,
                hasSet,
                this.Template.Declaration is { IsAutoPropertyOrField: true },
                this.Template.Declaration is { Writeability: Writeability.InitOnly } );

            if ( propertyTemplate.IsNotNull )
            {
                this.MemberBuilder.ApplyTemplateAttribute( propertyTemplate.TemplateInfo.Attribute );
            }
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( declarativeAdvices, diagnosticAdder );

            // TODO: Indexers.

            this.MemberBuilder.Type = (this.Template.Declaration?.Type ?? this._getTemplate.Declaration?.ReturnType).AssertNotNull();
            this.MemberBuilder.Accessibility = (this.Template.Declaration?.Accessibility ?? this._getTemplate.Declaration?.Accessibility).AssertNotNull();

            if ( this.Template.Declaration != null )
            {
                var declarator = (PropertyDeclarationSyntax)this.Template.Declaration.GetPrimaryDeclaration().AssertNotNull();

                if ( declarator.Initializer != null )
                {
                    this.MemberBuilder.InitializerTemplate = TemplateMember.Create( this.Template.Declaration, TemplateInfo.None, TemplateKind.Introduction );
                }
            }
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
            var existingDeclaration = this.TargetDeclaration.Properties.OfExactSignature( this.MemberBuilder, false, false );

            // TODO: Introduce attributes that are added not present on the existing member?
            if ( existingDeclaration == null )
            {
                // There is no existing declaration, we will introduce and override the introduced.
                var overriddenMethod = new OverriddenProperty(
                    this,
                    this.MemberBuilder,
                    this.Template,
                    this._getTemplate,
                    this._setTemplate );

                return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
            }
            else
            {
                if ( existingDeclaration.IsStatic != this.MemberBuilder.IsStatic )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );
                }

                switch ( this.OverrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Produce fail diagnostic.
                        return
                            AdviceResult.Create(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, this.MemberBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceResult.Create();

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, we fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( this.TargetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenProperty = new OverriddenProperty(
                                this,
                                existingDeclaration,
                                this.Template,
                                this._getTemplate,
                                this._setTemplate );

                            return AdviceResult.Create( overriddenProperty );
                        }
                        else
                        {
                            this.MemberBuilder.IsNew = true;
                            this.MemberBuilder.OverriddenProperty = existingDeclaration;

                            var overriddenProperty = new OverriddenProperty(
                                this,
                                this.MemberBuilder,
                                this.Template,
                                this._getTemplate,
                                this._setTemplate );

                            return AdviceResult.Create( this.MemberBuilder, overriddenProperty );
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( this.TargetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverriddenProperty(
                                this,
                                existingDeclaration,
                                this.Template,
                                this._getTemplate,
                                this._setTemplate );

                            return AdviceResult.Create( overriddenMethod );
                        }
                        else if ( existingDeclaration.IsSealed || !existingDeclaration.IsVirtual )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                        this.TargetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, this.MemberBuilder, this.TargetDeclaration,
                                         existingDeclaration.DeclaringType) ) );
                        }
                        else if ( !compilation.InvariantComparer.Equals( this.Builder.Type, existingDeclaration.Type ) )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                                        this.TargetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, this.MemberBuilder, this.TargetDeclaration,
                                         existingDeclaration.DeclaringType, existingDeclaration.Type) ) );
                        }
                        else
                        {
                            this.MemberBuilder.IsOverride = true;
                            this.MemberBuilder.OverriddenProperty = existingDeclaration;

                            var overriddenProperty = new OverriddenProperty(
                                this,
                                this.MemberBuilder,
                                this.Template,
                                this._getTemplate,
                                this._setTemplate );

                            return AdviceResult.Create( this.MemberBuilder, overriddenProperty );
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}