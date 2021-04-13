// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;

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

            this._methodBuilder.Accessibility = templateMethod.Accessibility;

            this._methodBuilder.IsStatic = templateMethod.IsStatic;
            this._methodBuilder.IsNew = templateMethod.IsNew;
            this._methodBuilder.IsAbstract = templateMethod.IsAbstract;
            this._methodBuilder.IsOverride = templateMethod.IsOverride;
            this._methodBuilder.IsVirtual = templateMethod.IsVirtual;
            this._methodBuilder.IsSealed = templateMethod.IsSealed;
            this._methodBuilder.IsAsync = templateMethod.IsAsync;

            this._methodBuilder.ReturnParameter.ParameterType = templateMethod.ReturnParameter.ParameterType;
            this._methodBuilder.ReturnParameter.RefKind = templateMethod.ReturnParameter.RefKind;

            CopyAttributes( templateMethod.ReturnParameter, this._methodBuilder.ReturnParameter );

            foreach ( var templateParameter in templateMethod.Parameters )
            {
                var parameterBuilder = this._methodBuilder.AddParameter( templateParameter.Name, templateParameter.ParameterType, templateParameter.RefKind, templateParameter.DefaultValue );
                CopyAttributes( templateParameter, parameterBuilder );
            }

            foreach ( var templateGenericParameter in templateMethod.GenericParameters )
            {
                var genericParameterBuilder = this._methodBuilder.AddGenericParameter( templateGenericParameter.Name );
                genericParameterBuilder.IsContravariant = templateGenericParameter.IsContravariant;
                genericParameterBuilder.IsCovariant = templateGenericParameter.IsCovariant;
                genericParameterBuilder.HasDefaultConstructorConstraint = templateGenericParameter.HasDefaultConstructorConstraint;
                genericParameterBuilder.HasNonNullableValueTypeConstraint = templateGenericParameter.HasNonNullableValueTypeConstraint;
                genericParameterBuilder.HasReferenceTypeConstraint = templateGenericParameter.HasReferenceTypeConstraint;

                foreach (var templateGenericParamterConstraint in genericParameterBuilder.TypeConstraints )
                {
                    genericParameterBuilder.TypeConstraints.Add( templateGenericParamterConstraint );
                }

                CopyAttributes( templateGenericParameter.AssertNotNull(), genericParameterBuilder );
            }

            CopyAttributes( templateMethod, this._methodBuilder );

            static void CopyAttributes( ICodeElement codeElement, ICodeElementBuilder builder )
            {
                // TODO: We don't want to copy all attributes.
                foreach ( var codeElementAttribute in codeElement.Attributes )
                {
                    var builderAttribute = builder.AddAttribute( codeElementAttribute.Type, codeElementAttribute.ConstructorArguments.Select( x => x.Value ).ToArray() );

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
            var existingDeclaration = this.TargetDeclaration.Methods.OfExactSignature( this._methodBuilder, false );

            // TODO: Introduce attributes that are added not present on the existing member?
            if ( existingDeclaration == null )
            {
                // There is no existing declaration, we will introduce and override the introduced.
                var overriddenMethod = new OverriddenMethod( this, this._methodBuilder, this.TemplateMethod, this.LinkerOptions );
                return AdviceResult.Create( this._methodBuilder, overriddenMethod );
            }
            else
            {
                switch ( this.ConflictBehavior )
                {
                    case ConflictBehavior.Fail:
                        // Produce fail diagnostic.
                        return 
                            AdviceResult.Create(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateDiagnostic(
                                    this.TargetDeclaration.GetLocation(),
                                    (this.Aspect.AspectType, this._methodBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );

                    case ConflictBehavior.Merge:
                    case ConflictBehavior.Ignore:
                        // Do nothing.
                        return AdviceResult.Create();

                    case ConflictBehavior.New:
                        // If the existing declaration is in the current type, we fail, otherwise, declare a newslot method and override.
                        if ( ((IEqualityComparer<IType>)compilation.InvariantComparer).Equals(this.TargetDeclaration, existingDeclaration.DeclaringType) )
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
                        else if ( existingDeclaration.IsSealed)
                        {
                            return
                                AdviceResult.Create(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateDiagnostic(
                                        this.TargetDeclaration.GetLocation(),
                                        (this.Aspect.AspectType, this._methodBuilder, this.TargetDeclaration, existingDeclaration.DeclaringType) ) );
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