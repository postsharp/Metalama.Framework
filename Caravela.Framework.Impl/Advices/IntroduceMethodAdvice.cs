// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal sealed class IntroduceMethodAdvice : Advice, IIntroduceMethodAdvice
    {
        private readonly MethodBuilder _methodBuilder;

        public IntroductionScope Scope { get; }

        public ConflictBehavior ConflictBehavior { get; }

        public IMethod TemplateMethod { get; }

        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public AspectLinkerOptions? LinkerOptions { get; }

        public IntroduceMethodAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            IMethod templateMethod,
            IntroductionScope scope,
            ConflictBehavior conflictBehavior,
            AspectLinkerOptions? linkerOptions )
            : base( aspect, targetDeclaration )
        {
            this.Scope = scope;
            this.ConflictBehavior = conflictBehavior;
            this.TemplateMethod = templateMethod;
            this.LinkerOptions = linkerOptions;

            this._methodBuilder = new MethodBuilder( this, targetDeclaration, templateMethod.Name, this.LinkerOptions );
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            this._methodBuilder.Accessibility = this.TemplateMethod.Accessibility;

            // Handle the introduction scope.
            switch ( this.Scope )
            {
                case IntroductionScope.Default:
                    if ( this.TemplateMethod.IsStatic )
                    {
                        goto case IntroductionScope.Static;
                    }
                    else
                    {
                        goto case IntroductionScope.Target;
                    }

                case IntroductionScope.Instance:
                    if ( this.TargetDeclaration.IsStatic && this.TargetDeclaration is IType )
                    {
                        // TODO: This should not be an exception.
                        diagnosticAdder.ReportDiagnostic(
                            AdviceDiagnosticDescriptors.CannotIntroduceInstanceMemberIntoStaticType.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectType.Type, this._methodBuilder, this.TargetDeclaration) ) );
                    }

                    this._methodBuilder.IsStatic = false;

                    break;

                case IntroductionScope.Static:
                    this._methodBuilder.IsStatic = true;

                    break;

                case IntroductionScope.Target:
                    this._methodBuilder.IsStatic = this.TargetDeclaration.IsStatic;

                    break;

                default:
                    throw new AssertionFailedException();
            }

            this._methodBuilder.IsNew = this.TemplateMethod.IsNew;
            this._methodBuilder.IsAbstract = this.TemplateMethod.IsAbstract;
            this._methodBuilder.IsOverride = this.TemplateMethod.IsOverride;
            this._methodBuilder.IsVirtual = this.TemplateMethod.IsVirtual;
            this._methodBuilder.IsSealed = this.TemplateMethod.IsSealed;
            this._methodBuilder.IsAsync = this.TemplateMethod.IsAsync;

            // Handle return type.
            if ( this.TemplateMethod.ReturnParameter.ParameterType.TypeKind == TypeKind.Dynamic )
            {
                // Templates with dynamic return value result in object return type of the introduced member.
                this._methodBuilder.ReturnParameter.ParameterType = this._methodBuilder.Compilation.Factory.GetTypeByReflectionType( typeof(object) );
            }
            else
            {
                this._methodBuilder.ReturnParameter.ParameterType = this.TemplateMethod.ReturnParameter.ParameterType;
                this._methodBuilder.ReturnParameter.RefKind = this.TemplateMethod.ReturnParameter.RefKind;
            }

            CopyAttributes( this.TemplateMethod.ReturnParameter, this._methodBuilder.ReturnParameter );

            foreach ( var templateParameter in this.TemplateMethod.Parameters )
            {
                var parameterBuilder = this._methodBuilder.AddParameter(
                    templateParameter.Name,
                    templateParameter.ParameterType,
                    templateParameter.RefKind,
                    templateParameter.DefaultValue );

                CopyAttributes( templateParameter, parameterBuilder );
            }

            foreach ( var templateGenericParameter in this.TemplateMethod.GenericParameters )
            {
                var genericParameterBuilder = this._methodBuilder.AddGenericParameter( templateGenericParameter.Name );
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

            CopyAttributes( this.TemplateMethod, this._methodBuilder );

            static void CopyAttributes( ICodeElement codeElement, ICodeElementBuilder builder )
            {
                // TODO: Don't copy all attributes, but how to decide which ones to keep?
                foreach ( var codeElementAttribute in codeElement.Attributes )
                {
                    var builderAttribute = builder.AddAttribute(
                        codeElementAttribute.Type,
                        codeElementAttribute.ConstructorArguments.Select( x => x.Value ).ToArray() );

                    foreach ( var codeElementAttributeNamedArgument in codeElementAttribute.NamedArguments )
                    {
                        builderAttribute.AddNamedArgument( codeElementAttributeNamedArgument.Key, codeElementAttributeNamedArgument.Value.Value );
                    }
                }
            }
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
            var existingDeclaration = this.TargetDeclaration.Methods.OfExactSignature( this._methodBuilder, false, false );

            // TODO: Introduce attributes that are added not present on the existing member?
            if ( existingDeclaration == null )
            {
                // There is no existing declaration, we will introduce and override the introduced.
                var overriddenMethod = new OverriddenMethod( this, this._methodBuilder, this.TemplateMethod, this.LinkerOptions );

                return AdviceResult.Create( this._methodBuilder, overriddenMethod );
            }
            else
            {
                if ( existingDeclaration.IsStatic != this._methodBuilder.IsStatic )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectType.Type, this._methodBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );
                }

                switch ( this.ConflictBehavior )
                {
                    case ConflictBehavior.Fail:
                        // Produce fail diagnostic.
                        return
                            AdviceResult.Create(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateDiagnostic(
                                    this.TargetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectType.Type, this._methodBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );

                    case ConflictBehavior.Merge:
                    case ConflictBehavior.Ignore:
                        // Do nothing.
                        return AdviceResult.Create();

                    case ConflictBehavior.New:
                        // If the existing declaration is in the current type, we fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( this.TargetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverriddenMethod( this, existingDeclaration, this.TemplateMethod, this.LinkerOptions );

                            return AdviceResult.Create( overriddenMethod );
                        }
                        else
                        {
                            this._methodBuilder.IsNew = true;
                            var overriddenMethod = new OverriddenMethod( this, this._methodBuilder, this.TemplateMethod, this.LinkerOptions );

                            return AdviceResult.Create( this._methodBuilder, overriddenMethod );
                        }

                    case ConflictBehavior.Override:
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( this.TargetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overriddenMethod = new OverriddenMethod( this, existingDeclaration, this.TemplateMethod, this.LinkerOptions );

                            return AdviceResult.Create( overriddenMethod );
                        }
                        else if ( existingDeclaration.IsSealed )
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateDiagnostic(
                                        this.TargetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectType.Type, this._methodBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );
                        }
                        else
                        {
                            var overriddenMethod = new OverriddenMethod( this, this._methodBuilder, this.TemplateMethod, this.LinkerOptions );
                            this._methodBuilder.IsOverride = true;

                            return AdviceResult.Create( this._methodBuilder, overriddenMethod );
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        public IMethodBuilder Builder => this._methodBuilder;
    }
}