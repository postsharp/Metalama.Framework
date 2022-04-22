// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Advices
{
    internal sealed class IntroduceMethodAdvice : IntroduceMemberAdvice<IMethod, MethodBuilder>
    {
        public new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        public IMethodBuilder Builder => this.MemberBuilder;

        public IntroduceMethodAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            TemplateMember<IMethod> templateMethod,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            Dictionary<string, object?>? tags )
            : base( aspect, templateInstance, targetDeclaration, templateMethod, scope, overrideStrategy, layerName, tags )
        {
            Invariant.Assert( templateMethod.IsNotNull );

            this.MemberBuilder = new MethodBuilder( this, targetDeclaration, templateMethod.Declaration.AssertNotNull().Name );
            this.MemberBuilder.ApplyTemplateAttribute( templateMethod.TemplateInfo.Attribute );
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( diagnosticAdder );

            this.MemberBuilder.IsAsync = this.Template.Declaration!.IsAsync;

            // Handle return type.
            if ( this.Template.Declaration.ReturnParameter.Type.TypeKind == TypeKind.Dynamic )
            {
                // Templates with dynamic return value result in object return type of the introduced member.
                this.MemberBuilder.ReturnParameter.Type = this.MemberBuilder.Compilation.Factory.GetTypeByReflectionType( typeof(object) );
            }
            else
            {
                this.MemberBuilder.ReturnParameter.Type = this.Template.Declaration.ReturnParameter.Type;
                this.MemberBuilder.ReturnParameter.RefKind = this.Template.Declaration.ReturnParameter.RefKind;
            }

            CopyAttributes( this.Template.Declaration.ReturnParameter, this.MemberBuilder.ReturnParameter );

            foreach ( var templateParameter in this.Template.Declaration.Parameters )
            {
                var parameterBuilder = this.MemberBuilder.AddParameter(
                    templateParameter.Name,
                    templateParameter.Type,
                    templateParameter.RefKind,
                    templateParameter.DefaultValue );

                CopyAttributes( templateParameter, parameterBuilder );
            }

            foreach ( var templateGenericParameter in this.Template.Declaration.TypeParameters )
            {
                var genericParameterBuilder = this.MemberBuilder.AddTypeParameter( templateGenericParameter.Name );
                genericParameterBuilder.Variance = templateGenericParameter.Variance;
                genericParameterBuilder.HasDefaultConstructorConstraint = templateGenericParameter.HasDefaultConstructorConstraint;
                genericParameterBuilder.TypeKindConstraint = templateGenericParameter.TypeKindConstraint;

                foreach ( var templateGenericParameterConstraint in templateGenericParameter.TypeConstraints )
                {
                    genericParameterBuilder.AddTypeConstraint( templateGenericParameterConstraint );
                }

                CopyAttributes( templateGenericParameter.AssertNotNull(), genericParameterBuilder );
            }
        }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var existingDeclaration = targetDeclaration.FindClosestVisibleMethod(
                this.MemberBuilder,
                observableTransformations.OfType<IMethod>().ToList() );

            // TODO: Introduce attributes that are added not present on the existing member?
            if ( existingDeclaration == null )
            {
                // There is no existing declaration, we will introduce and override the introduced.
                var overriddenMethod = new OverriddenMethod( this, this.MemberBuilder, this.Template );
                this.MemberBuilder.IsOverride = false;
                this.MemberBuilder.IsNew = false;

                return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
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
                        // If the existing declaration is in the current type, override it, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverriddenMethod( this, existingDeclaration, this.Template );

                            return AdviceResult.Create( overriddenMethod );
                        }
                        else
                        {
                            this.MemberBuilder.IsNew = true;
                            this.MemberBuilder.IsOverride = false;

                            var overriddenMethod = new OverriddenMethod( this, this.MemberBuilder, this.Template );

                            return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverriddenMethod( this, existingDeclaration, this.Template );

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
                        else if ( !compilation.InvariantComparer.Is(
                                     this.Builder.ReturnType,
                                     existingDeclaration.ReturnType,
                                     ConversionKind.ImplicitReference ) )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                                        targetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                         existingDeclaration.DeclaringType, existingDeclaration.ReturnType) ) );
                        }
                        else
                        {
                            this.MemberBuilder.IsOverride = true;
                            this.MemberBuilder.IsNew = false;
                            this.MemberBuilder.OverriddenMethod = existingDeclaration;
                            var overriddenMethod = new OverriddenMethod( this, this.MemberBuilder, this.Template );

                            return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}