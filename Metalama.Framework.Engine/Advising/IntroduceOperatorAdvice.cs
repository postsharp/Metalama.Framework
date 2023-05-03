﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class IntroduceOperatorAdvice : IntroduceMemberAdvice<IMethod, MethodBuilder>
    {
        private readonly PartiallyBoundTemplateMethod _template;

        private new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        public IntroduceOperatorAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            ICompilation sourceCompilation,
            OperatorKind operatorKind,
            IType leftOperandType,
            IType? rightOperandType,
            IType resultType,
            PartiallyBoundTemplateMethod template,
            OverrideStrategy overrideStrategy,
            Action<IMethodBuilder>? buildAction,
            string? layerName,
            IObjectReader tags )
            : base(
                aspect,
                templateInstance,
                targetDeclaration,
                sourceCompilation,
                null,
                template.TemplateMember,
                IntroductionScope.Static,
                overrideStrategy,
                buildAction,
                layerName,
                tags )
        {
            this._template = template;

            this.Builder = new MethodBuilder( this, targetDeclaration, operatorKind.ToOperatorMethodName(), DeclarationKind.Operator, operatorKind );

            var runtimeParameters = template.TemplateMember.TemplateClassMember.RunTimeParameters;

            // Add predefined parameters of correct types.
            var firstParameterName = !runtimeParameters.IsEmpty ? runtimeParameters[0].Name : "a";
            this.Builder.AddParameter( firstParameterName, leftOperandType );

            if ( rightOperandType != null )
            {
                var secondParameterName = !runtimeParameters.IsEmpty ? runtimeParameters[1].Name : "a";
                this.Builder.AddParameter( secondParameterName, rightOperandType );
            }

            this.Builder.ReturnType = resultType;
        }

        protected override void InitializeCore( ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder, TemplateAttributeProperties? templateAttributeProperties )
        {
            base.InitializeCore( serviceProvider, diagnosticAdder, templateAttributeProperties );

            var runtimeParameters = this.Template.AssertNotNull().TemplateClassMember.RunTimeParameters;

            CopyTemplateAttributes( this.Template.AssertNotNull().Declaration.ReturnParameter, this.Builder.ReturnParameter, serviceProvider );

            if ( runtimeParameters.Length == this.Builder.Parameters.Count )
            {
                for ( var i = 0; i < runtimeParameters.Length; i++ )
                {
                    var runtimeParameter = runtimeParameters[i];
                    var templateParameter = this.Template.AssertNotNull().Declaration.Parameters[runtimeParameter.SourceIndex];
                    var parameterBuilder = this.Builder.Parameters[i];

                    CopyTemplateAttributes( templateParameter, parameterBuilder, serviceProvider );
                }
            }

            // Invalid signatures may have incorrect parameter number, but we validate template later than initializing the builder.
        }

        public override AdviceKind AdviceKind => AdviceKind.IntroduceOperator;

        public override AdviceImplementationResult Implement(
            ProjectServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );
            var existingOperator = targetDeclaration.FindClosestVisibleMethod( this.Builder );

            if ( existingOperator == null )
            {
                var overriddenOperator = new OverrideOperatorTransformation( this, this.Builder, this._template.ForIntroduction( this.Builder ), this.Tags );

                addTransformation( this.Builder.ToTransformation() );
                addTransformation( overriddenOperator );

                return AdviceImplementationResult.Success( this.Builder );
            }
            else
            {
                switch ( this.OverrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Produce fail diagnostic.
                        return
                            AdviceImplementationResult.Failed(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                     existingOperator.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceImplementationResult.Ignored;

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, override it, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingOperator.DeclaringType ) )
                        {
                            var overriddenOperator = new OverrideOperatorTransformation( this, existingOperator, this._template.ForIntroduction( existingOperator ), this.Tags );

                            addTransformation( overriddenOperator );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override, existingOperator );
                        }
                        else
                        {
                            this.Builder.IsNew = true;
                            this.Builder.IsOverride = false;

                            var overriddenOperator = new OverrideOperatorTransformation( this, this.Builder, this._template.ForIntroduction( this.Builder ), this.Tags );

                            addTransformation( overriddenOperator );
                            addTransformation( this.Builder.ToTransformation() );

                            return AdviceImplementationResult.Success( AdviceOutcome.New, this.Builder );
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingOperator.DeclaringType ) )
                        {
                            var overriddenOperator = new OverrideOperatorTransformation( this, existingOperator, this._template.ForIntroduction( existingOperator ), this.Tags );
                            addTransformation( overriddenOperator );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override, existingOperator );
                        }
                        else if ( existingOperator.IsSealed || !existingOperator.IsOverridable() )
                        {
                            return
                                AdviceImplementationResult.Failed(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                        targetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                         existingOperator.DeclaringType) ) );
                        }
                        else
                        {
                            this.Builder.IsOverride = true;
                            this.Builder.IsNew = false;
                            this.Builder.OverriddenMethod = existingOperator;
                            var overriddenOperator = new OverrideOperatorTransformation( this, this.Builder, this._template.ForIntroduction( this.Builder ), this.Tags );

                            addTransformation( this.Builder.ToTransformation() );
                            addTransformation( overriddenOperator );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override, this.Builder );
                        }

                    default:
                        throw new AssertionFailedException( $"Invalid value for OverrideStrategy: {this.OverrideStrategy}." );
                }
            }
        }
    }
}