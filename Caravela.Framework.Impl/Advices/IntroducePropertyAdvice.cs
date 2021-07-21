// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroducePropertyAdvice : IntroduceMemberAdvice<PropertyBuilder>
    {
        private readonly IMethod? _getTemplateMethod;
        private readonly IMethod? _setTemplateMethod;

        public new IProperty? TemplateMember => (IProperty?) base.TemplateMember;

        public new INamedType TargetDeclaration => base.TargetDeclaration!;

        public IPropertyBuilder Builder => this.MemberBuilder;

        public IntroducePropertyAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            string? explicitName,
            IProperty? templateProperty,
            IMethod? getTemplateMethod,
            IMethod? setTemplateMethod,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            Dictionary<string, object?>? tags )
            : base( aspect, targetDeclaration, templateProperty, scope, overrideStrategy, layerName, tags )
        {
            this._getTemplateMethod = getTemplateMethod;
            this._setTemplateMethod = setTemplateMethod;

            var name = templateProperty?.Name ?? explicitName ?? throw new AssertionFailedException();
            var hasGet = templateProperty != null ? templateProperty.Getter != null : getTemplateMethod != null;
            var hasSet = templateProperty != null ? templateProperty.Setter != null : setTemplateMethod != null;

            this.MemberBuilder = new PropertyBuilder(
                this,
                this.TargetDeclaration,
                name,
                hasGet,
                hasSet,
                this.TemplateMember != null && this.TemplateMember.IsAutoPropertyOrField,
                this.TemplateMember != null && this.TemplateMember.Writeability == Writeability.InitOnly );
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( declarativeAdvices, diagnosticAdder );

            // TODO: Indexers.

            this.MemberBuilder.Type = (this.TemplateMember?.Type ?? this._getTemplateMethod?.ReturnType).AssertNotNull();
            this.MemberBuilder.Accessibility = (this.TemplateMember?.Accessibility ?? this._getTemplateMethod?.Accessibility).AssertNotNull();

            if ( this.TemplateMember != null )
            {
                var declaration = (PropertyDeclarationSyntax) this.TemplateMember.GetPrimaryDeclaration().AssertNotNull();
                this.MemberBuilder.InitializerSyntax = declaration.Initializer?.Value;
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
                    this.TemplateMember,
                    this._getTemplateMethod,
                    this._setTemplateMethod );

                return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
            }
            else
            {
                if ( existingDeclaration.IsStatic != this.MemberBuilder.IsStatic )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );
                }

                switch ( this.OverrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Produce fail diagnostic.
                        return
                            AdviceResult.Create(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceResult.Create();

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, we fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( this.TargetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverriddenProperty(
                                this,
                                existingDeclaration,
                                this.TemplateMember,
                                this._getTemplateMethod,
                                this._setTemplateMethod );

                            return AdviceResult.Create( overriddenMethod );
                        }
                        else
                        {
                            this.MemberBuilder.IsNew = true;
                            this.MemberBuilder.OverriddenProperty = existingDeclaration;

                            var overriddenMethod = new OverriddenProperty(
                                this,
                                this.MemberBuilder,
                                this.TemplateMember,
                                this._getTemplateMethod,
                                this._setTemplateMethod );

                            return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( this.TargetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverriddenProperty(
                                this,
                                existingDeclaration,
                                this.TemplateMember,
                                this._getTemplateMethod,
                                this._setTemplateMethod );

                            return AdviceResult.Create( overriddenMethod );
                        }
                        else if ( existingDeclaration.IsSealed || !existingDeclaration.IsVirtual )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateDiagnostic(
                                        this.TargetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration,
                                         existingDeclaration.DeclaringType) ) );
                        }
                        else if ( !compilation.InvariantComparer.Equals( this.Builder.Type, existingDeclaration.Type ) )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateDiagnostic(
                                        this.TargetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration,
                                         existingDeclaration.DeclaringType, existingDeclaration.Type) ) );
                        }
                        else
                        {
                            this.MemberBuilder.IsOverride = true;
                            this.MemberBuilder.OverriddenProperty = existingDeclaration;

                            var overriddenMethod = new OverriddenProperty(
                                this,
                                this.MemberBuilder,
                                this.TemplateMember,
                                this._getTemplateMethod,
                                this._setTemplateMethod );

                            return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}