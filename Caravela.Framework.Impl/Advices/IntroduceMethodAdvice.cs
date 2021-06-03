// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal sealed class IntroduceMethodAdvice : IntroduceMemberAdvice<MethodBuilder>
    {
        public new IMethod TemplateMember => (IMethod) base.TemplateMember.AssertNotNull();

        public new INamedType TargetDeclaration => base.TargetDeclaration!;

        public IMethodBuilder Builder => this.MemberBuilder;

        public IntroduceMethodAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            IMethod templateMethod,
            IntroductionScope scope,
            ConflictBehavior conflictBehavior,
            string? layerName,
            AdviceOptions? options )
            : base( aspect, targetDeclaration, templateMethod, scope, conflictBehavior, layerName, options )
        {
            this.MemberBuilder = new MethodBuilder( this, targetDeclaration, templateMethod.Name, this.LinkerOptions );
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( declarativeAdvices, diagnosticAdder );

            this.MemberBuilder.IsAsync = this.TemplateMember.IsAsync;

            // Handle return type.
            if ( this.TemplateMember.ReturnParameter.ParameterType.TypeKind == TypeKind.Dynamic )
            {
                // Templates with dynamic return value result in object return type of the introduced member.
                this.MemberBuilder.ReturnParameter.ParameterType = this.MemberBuilder.Compilation.Factory.GetTypeByReflectionType( typeof(object) );
            }
            else
            {
                this.MemberBuilder.ReturnParameter.ParameterType = this.TemplateMember.ReturnParameter.ParameterType;
                this.MemberBuilder.ReturnParameter.RefKind = this.TemplateMember.ReturnParameter.RefKind;
            }

            CopyAttributes( this.TemplateMember.ReturnParameter, this.MemberBuilder.ReturnParameter );

            foreach ( var templateParameter in this.TemplateMember.Parameters )
            {
                var parameterBuilder = this.MemberBuilder.AddParameter(
                    templateParameter.Name,
                    templateParameter.ParameterType,
                    templateParameter.RefKind,
                    templateParameter.DefaultValue );

                CopyAttributes( templateParameter, parameterBuilder );
            }

            foreach ( var templateGenericParameter in this.TemplateMember.GenericParameters )
            {
                var genericParameterBuilder = this.MemberBuilder.AddGenericParameter( templateGenericParameter.Name );
                genericParameterBuilder.IsContravariant = templateGenericParameter.IsContravariant;
                genericParameterBuilder.IsCovariant = templateGenericParameter.IsCovariant;
                genericParameterBuilder.HasDefaultConstructorConstraint = templateGenericParameter.HasDefaultConstructorConstraint;
                genericParameterBuilder.HasNonNullableValueTypeConstraint = templateGenericParameter.HasNonNullableValueTypeConstraint;
                genericParameterBuilder.HasReferenceTypeConstraint = templateGenericParameter.HasReferenceTypeConstraint;

                foreach ( var templateGenericParameterConstraint in genericParameterBuilder.TypeConstraints )
                {
                    genericParameterBuilder.TypeConstraints.Add( templateGenericParameterConstraint );
                }

                CopyAttributes( templateGenericParameter.AssertNotNull(), genericParameterBuilder );
            }
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
            var existingDeclaration = this.TargetDeclaration.Methods.OfExactSignature( this.MemberBuilder, false, false );

            // TODO: Introduce attributes that are added not present on the existing member?
            if ( existingDeclaration == null )
            {
                // There is no existing declaration, we will introduce and override the introduced.
                var overriddenMethod = new OverriddenMethod( this, this.MemberBuilder, this.TemplateMember, this.LinkerOptions );

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

                switch ( this.ConflictBehavior )
                {
                    case ConflictBehavior.Fail:
                        // Produce fail diagnostic.
                        return
                            AdviceResult.Create(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );

                    case ConflictBehavior.Merge:
                    case ConflictBehavior.Ignore:
                        // Do nothing.
                        return AdviceResult.Create();

                    case ConflictBehavior.New:
                        // If the existing declaration is in the current type, we fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( this.TargetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverriddenMethod( this, existingDeclaration, this.TemplateMember, this.LinkerOptions );

                            return AdviceResult.Create( overriddenMethod );
                        }
                        else
                        {
                            this.MemberBuilder.IsNew = true;
                            this.MemberBuilder.OverriddenMethod = existingDeclaration;
                            var overriddenMethod = new OverriddenMethod( this, this.MemberBuilder, this.TemplateMember, this.LinkerOptions );

                            return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
                        }

                    case ConflictBehavior.Override:
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( this.TargetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverriddenMethod( this, existingDeclaration, this.TemplateMember, this.LinkerOptions );

                            return AdviceResult.Create( overriddenMethod );
                        }
                        else if ( existingDeclaration.IsSealed )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateDiagnostic(
                                        this.TargetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration,
                                         existingDeclaration.DeclaringType) ) );
                        }
                        else
                        {
                            this.MemberBuilder.IsOverride = true;
                            this.MemberBuilder.OverriddenMethod = existingDeclaration;
                            var overriddenMethod = new OverriddenMethod( this, this.MemberBuilder, this.TemplateMember, this.LinkerOptions );

                            return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}