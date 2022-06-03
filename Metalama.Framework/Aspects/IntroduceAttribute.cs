// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    public sealed class IntroduceAttribute : DeclarativeAdviceAttribute
    {
        
        public IntroductionScope Scope { get; set; }

        /// <summary>
        /// Gets or sets the implementation strategy (like <see cref="OverrideStrategy.Override"/>, <see cref="OverrideStrategy.Fail"/> or <see cref="OverrideStrategy.Ignore"/>) when the member is already declared in the target type.
        /// The default value is <see cref="OverrideStrategy.Fail"/>. 
        /// </summary>
        public OverrideStrategy WhenExists { get; set; }

        /// <summary>
        /// Gets or sets the implementation strategy (like <see cref="OverrideStrategy.Override"/>, <see cref="OverrideStrategy.Fail"/> or <see cref="OverrideStrategy.Ignore"/>) when the member is already declared
        /// in a parent class of the target tye.
        /// The default value is <see cref="OverrideStrategy.Fail"/>. 
        /// </summary>
        [Obsolete( "Not implemented." )]
        public OverrideStrategy WhenInherited { get; set; }

        
        public override bool IsIntroduction => true;

        public override void BuildEligibility( IEligibilityBuilder<IDeclaration> builder )
        {
            builder.AddRule(
                new EligibilityRule<IDeclaration>(
                    EligibleScenarios.Inheritance,
                    x =>
                    {
                        var t = x.GetDeclaringType();

                        return t != null && t.TypeKind != TypeKind.Interface;
                    },
                    _ => $"the aspect contains a declarative introduction and therefore cannot be applied to an interface" ) );
        }

        public override bool TryBuildAspect( IMemberOrNamedType templateMember, string templateMemberId, IAspectBuilder<IDeclaration> builder )
        {
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
                        DeclarativeAdviceDiagnosticDescriptors.CannotUseIntroduceWithoutDeclaringType.WithArguments(
                            (builder.AspectInstance.AspectClass.ShortName, templateMember.DeclarationKind, builder.Target.DeclarationKind) ) );

                    return false;
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

            return true;
        }
    }
}