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

namespace Metalama.Framework.Engine.Advices
{
    internal sealed class IntroduceMethodAdvice : IntroduceMemberAdvice<IMethod, MethodBuilder>
    {
        public BoundTemplateMethod BoundTemplate { get; }

        public new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        public IMethodBuilder Builder => this.MemberBuilder;

        public IntroduceMethodAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            BoundTemplateMethod boundTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            IObjectReader tags )
            : base( aspect, templateInstance, targetDeclaration, null, boundTemplate.Template, scope, overrideStrategy, layerName, tags )
        {
            this.BoundTemplate = boundTemplate;
            Invariant.Assert( !boundTemplate.IsNull );

            this.MemberBuilder = new MethodBuilder( this, targetDeclaration, this.MemberName, tags );
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( diagnosticAdder );

            this.MemberBuilder.IsAsync = this.Template.Declaration!.IsAsync;
            var typeRewriter = TemplateTypeRewriter.Get( this.BoundTemplate );

            // Handle return type.
            if ( this.Template.Declaration.ReturnParameter.Type.TypeKind == TypeKind.Dynamic )
            {
                // Templates with dynamic return value result in object return type of the introduced member.
                this.MemberBuilder.ReturnParameter.Type = this.MemberBuilder.Compilation.Factory.GetTypeByReflectionType( typeof(object) );
            }
            else
            {
                this.MemberBuilder.ReturnParameter.Type = typeRewriter.Visit( this.Template.Declaration.ReturnParameter.Type );
                this.MemberBuilder.ReturnParameter.RefKind = this.Template.Declaration.ReturnParameter.RefKind;
            }

            CopyAttributes( this.Template.Declaration.ReturnParameter, this.MemberBuilder.ReturnParameter );

            foreach ( var templateParameter in this.Template.Declaration.Parameters )
            {
                if ( this.Template.TemplateClassMember.Parameters[templateParameter.Index].IsCompileTime )
                {
                    continue;
                }

                var parameterBuilder = this.MemberBuilder.AddParameter(
                    templateParameter.Name,
                    typeRewriter.Visit( templateParameter.Type ),
                    templateParameter.RefKind,
                    templateParameter.DefaultValue );

                CopyAttributes( templateParameter, parameterBuilder );
            }

            foreach ( var templateGenericParameter in this.Template.Declaration.TypeParameters )
            {
                if ( this.Template.TemplateClassMember.TypeParameters[templateGenericParameter.Index].IsCompileTime )
                {
                    continue;
                }

                var genericParameterBuilder = this.MemberBuilder.AddTypeParameter( templateGenericParameter.Name );
                genericParameterBuilder.Variance = templateGenericParameter.Variance;
                genericParameterBuilder.HasDefaultConstructorConstraint = templateGenericParameter.HasDefaultConstructorConstraint;
                genericParameterBuilder.TypeKindConstraint = templateGenericParameter.TypeKindConstraint;

                foreach ( var templateGenericParameterConstraint in templateGenericParameter.TypeConstraints )
                {
                    genericParameterBuilder.AddTypeConstraint( typeRewriter.Visit( templateGenericParameterConstraint ) );
                }

                CopyAttributes( templateGenericParameter.AssertNotNull(), genericParameterBuilder );
            }
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var existingMethod = targetDeclaration.FindClosestVisibleMethod( this.MemberBuilder );

            // TODO: Introduce attributes that are added not present on the existing member?
            if ( existingMethod == null )
            {
                // Check that there is no other member named the same, otherwise we cannot add a method.
                var existingOtherMember =
                    this.Builder is { Name: "Finalize", Parameters: { Count: 0 }, TypeParameters: { Count: 0 } }
                    ? targetDeclaration.Finalizer
                    : targetDeclaration.FindClosestUniquelyNamedMember( this.MemberBuilder.Name );

                if ( existingOtherMember != null )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration, existingOtherMember.DeclarationKind) ) );
                }

                // There is no existing declaration, we will introduce and override the introduced.
                var overriddenMethod = new OverrideMethodTransformation( this, this.MemberBuilder, this.BoundTemplate, this.Tags );
                this.MemberBuilder.IsOverride = false;
                this.MemberBuilder.IsNew = false;

                return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
            }
            else
            {
                if ( existingMethod.IsStatic != this.MemberBuilder.IsStatic )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                 existingMethod.DeclaringType) ) );
                }
                else if ( !compilation.InvariantComparer.Is(
                             this.Builder.ReturnType,
                             existingMethod.ReturnType,
                             ConversionKind.ImplicitReference ) )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                 existingMethod.DeclaringType, existingMethod.ReturnType) ) );
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
                                     existingMethod.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceResult.Create();

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, override it, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingMethod.DeclaringType ) )
                        {
                            var overriddenMethod = new OverrideMethodTransformation( this, existingMethod, this.BoundTemplate, this.Tags );

                            return AdviceResult.Create( overriddenMethod );
                        }
                        else
                        {
                            this.MemberBuilder.IsNew = true;
                            this.MemberBuilder.IsOverride = false;

                            var overriddenMethod = new OverrideMethodTransformation( this, this.MemberBuilder, this.BoundTemplate, this.Tags );

                            return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingMethod.DeclaringType ) )
                        {
                            var overriddenMethod = new OverrideMethodTransformation( this, existingMethod, this.BoundTemplate, this.Tags );

                            return AdviceResult.Create( overriddenMethod );
                        }
                        else if ( existingMethod.IsSealed || !existingMethod.IsVirtual )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                        targetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                         existingMethod.DeclaringType) ) );
                        }
                        else
                        {
                            this.MemberBuilder.IsOverride = true;
                            this.MemberBuilder.IsNew = false;
                            this.MemberBuilder.OverriddenMethod = existingMethod;
                            var overriddenMethod = new OverrideMethodTransformation( this, this.MemberBuilder, this.BoundTemplate, this.Tags );

                            return AdviceResult.Create( this.MemberBuilder, overriddenMethod );
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}