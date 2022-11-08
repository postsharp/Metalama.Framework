// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Eligibility.Implementation;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Custom attribute that can be applied to any member of an aspect class and that means that this member must be introduced to
    /// the target class of the aspect. 
    /// </summary>
    /// <seealso href="@introducing-members"/>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Event )]
    public sealed class IntroduceAttribute : DeclarativeAdviceAttribute, ITemplateAttribute
    {
        private TemplateAttributeImpl _impl;

        public string? Name { get => this._impl.Name; set => this._impl.Name = value; }

        public Accessibility Accessibility
        {
            get => this._impl.Accessibility;
            set => this._impl.Accessibility = value;
        }

        public bool IsVirtual
        {
            get => this._impl.IsVirtual;

            set => this._impl.IsVirtual = value;
        }

        public bool IsSealed
        {
            get => this._impl.IsSealed;
            set => this._impl.IsSealed = value;
        }

        bool? ITemplateAttribute.IsVirtual => this._impl.GetIsVirtual();

        bool? ITemplateAttribute.IsSealed => this._impl.GetIsSealed();

        Accessibility? ITemplateAttribute.Accessibility => this._impl.GetAccessibility();

        public IntroductionScope Scope { get; set; }

        /// <summary>
        /// Gets or sets the implementation strategy (like <see cref="OverrideStrategy.Override"/>, <see cref="OverrideStrategy.Fail"/> or <see cref="OverrideStrategy.Ignore"/>) when the member is already declared in the target type.
        /// The default value is <see cref="OverrideStrategy.Fail"/>. 
        /// </summary>
        public OverrideStrategy WhenExists { get; set; }

        /// <summary>
        /// Gets or sets the implementation strategy (like <see cref="OverrideStrategy.Override"/>, <see cref="OverrideStrategy.Fail"/> or <see cref="OverrideStrategy.Ignore"/>) when the member is already declared
        /// in a parent class of the target type.
        /// The default value is <see cref="OverrideStrategy.Fail"/>. 
        /// </summary>
        [Obsolete( "Not implemented." )]
        public OverrideStrategy WhenInherited { get; set; }

        public override void BuildAspectEligibility( IEligibilityBuilder<IDeclaration> builder, IMemberOrNamedType adviceMember )
        {
            builder.MustBeOfType( typeof(IMemberOrNamedType) );

            builder.MustBeExplicitlyDeclared();

            builder.AddRule(
                new EligibilityRule<IDeclaration>(
                    EligibleScenarios.Inheritance,
                    x =>
                    {
                        var t = x.GetClosestNamedType();

                        return t != null && t.TypeKind != TypeKind.Interface;
                    },
                    _ => $"the aspect contains a declarative introduction and therefore cannot be applied to an interface" ) );

            var isEffectivelyInstance =
                (this.Scope, adviceMember.IsStatic) switch
                {
                    (IntroductionScope.Default, false) => true,
                    (IntroductionScope.Instance, _) => true,
                    _ => false
                };

            var isEffectivelyVirtual =
                (this._impl.GetIsVirtual(), (adviceMember as IMember)?.IsVirtual ?? false) switch
                {
                    (null, true) => true,
                    (true, _) => true,
                    _ => false
                };

            // Rules for virtuality and staticity.
            if ( isEffectivelyInstance )
            {
                builder.AddRule(
                    new EligibilityRule<IDeclaration>(
                        EligibleScenarios.Inheritance,
                        x =>
                        {
                            var t = x.GetClosestNamedType();

                            return t != null && !t.IsStatic;
                        },
                        _ => $"the aspect contains an instance declarative introduction and therefore cannot be applied to static types" ) );
            }

            if ( isEffectivelyVirtual )
            {
                builder.AddRule(
                    new EligibilityRule<IDeclaration>(
                        EligibleScenarios.Inheritance,
                        x =>
                        {
                            var t = x.GetClosestNamedType();

                            return t != null && t.TypeKind is not TypeKind.Struct or TypeKind.RecordStruct && !t.IsStatic && !t.IsSealed;
                        },
                        _ => $"the aspect contains an virtual declarative introduction and therefore cannot be applied to sealed types, static types and structs" ) );
            }
        }

        public override void BuildAdvice( IMemberOrNamedType templateMember, string templateMemberId, IAspectBuilder<IDeclaration> builder )
        {
            if ( this.Layer != builder.Layer )
            {
                return;
            }

            INamedType targetType;

            switch ( builder.Target )
            {
                case IMember member:
                    targetType = member.DeclaringType;

                    break;

                case INamedType type:
                    targetType = type;

                    break;

                default:
                    builder.Diagnostics.Report(
                        FrameworkDiagnosticDescriptors.CannotUseIntroduceWithoutDeclaringType.WithArguments(
                            (builder.AspectInstance.AspectClass.ShortName, templateMember.DeclarationKind, builder.Target.DeclarationKind) ) );

                    builder.SkipAspect();

                    return;
            }

            switch ( targetType )
            {
                case { TypeKind: not (TypeKind.Class or TypeKind.Struct or TypeKind.RecordClass or TypeKind.RecordStruct) }:
                    builder.Diagnostics.Report(
                        FrameworkDiagnosticDescriptors.CannotApplyAdviceOnTypeOrItsMembers.WithArguments(
                            (builder.AspectInstance.AspectClass.ShortName, templateMember.DeclarationKind, targetType.TypeKind) ) );

                    builder.SkipAspect();

                    return;
            }

            switch ( templateMember.DeclarationKind )
            {
                case DeclarationKind.Method:
                    builder.Advice.IntroduceMethod( targetType, templateMemberId, this.Scope, this.WhenExists );

                    break;

                case DeclarationKind.Property:
                    builder.Advice.IntroduceProperty( targetType, templateMemberId, this.Scope, this.WhenExists );

                    break;

                case DeclarationKind.Event:
                    builder.Advice.IntroduceEvent( targetType, templateMemberId, this.Scope, this.WhenExists );

                    break;

                case DeclarationKind.Field:
                    builder.Advice.IntroduceField( targetType, templateMemberId, this.Scope, this.WhenExists );

                    break;

                default:
                    throw new InvalidOperationException( $"Don't know how to introduce a {templateMember.DeclarationKind}." );
            }
        }
    }
}