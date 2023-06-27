// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
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
    internal sealed class IntroduceFinalizerAdvice : IntroduceMemberAdvice<IMethod, MethodBuilder>
    {
        private readonly PartiallyBoundTemplateMethod _template;

        private new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        public IntroduceFinalizerAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            ICompilation sourceCompilation,
            PartiallyBoundTemplateMethod template,
            OverrideStrategy overrideStrategy,
            string? layerName,
            IObjectReader tags )
            : base(
                aspect,
                templateInstance,
                targetDeclaration,
                sourceCompilation,
                null,
                template.TemplateMember,
                IntroductionScope.Instance,
                overrideStrategy,
                _ => { },
                layerName,
                tags )
        {
            this._template = template;

            this.Builder = new MethodBuilder( this, targetDeclaration, "Finalize", DeclarationKind.Finalizer );
        }

        protected override void InitializeCore(
            ProjectServiceProvider serviceProvider,
            IDiagnosticAdder diagnosticAdder,
            TemplateAttributeProperties? templateAttributeProperties )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( this.SourceCompilation );

            switch ( this.OverrideStrategy )
            {
                case OverrideStrategy.New:
                    diagnosticAdder.Report(
                        AdviceDiagnosticDescriptors.CannotUseNewOverrideStrategyWithFinalizers.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, targetDeclaration, this.OverrideStrategy) ) );

                    break;
            }

            // TODO: The base implementation may take more than needed from the template. Most would be ignored by the transformation, but
            //       the user may see it in the code model.
            base.InitializeCore( serviceProvider, diagnosticAdder, templateAttributeProperties );
        }

        public override AdviceKind AdviceKind => AdviceKind.IntroduceFinalizer;

        public override AdviceImplementationResult Implement(
            ProjectServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var existingFinalizer = targetDeclaration.Finalizer;

            // TODO: Introduce attributes that are added not present on the existing member?
            if ( existingFinalizer == null )
            {
                // Check that there is no other member named the same, which is possible, but very unlikely.
                var existingOtherMember = targetDeclaration.FindClosestUniquelyNamedMember( this.Builder.Name );

                if ( existingOtherMember != null )
                {
                    return
                        AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration, existingOtherMember.DeclarationKind) ) );
                }

                // There is no existing declaration, we will introduce and override the introduced.
                var overriddenMethod = new OverrideFinalizerTransformation( this, this.Builder, this._template.ForIntroduction( this.Builder ), this.Tags );
                this.Builder.IsOverride = false;
                this.Builder.HasNewKeyword = this.Builder.IsNew = false;

                addTransformation( this.Builder.ToTransformation() );
                addTransformation( overriddenMethod );

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
                                     existingFinalizer.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceImplementationResult.Ignored;

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingFinalizer.DeclaringType ) )
                        {
                            var overriddenMethod = new OverrideFinalizerTransformation( this, existingFinalizer, this._template.ForIntroduction( existingFinalizer ), this.Tags );
                            addTransformation( overriddenMethod );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override, existingFinalizer );
                        }
                        else
                        {
                            this.Builder.IsOverride = true;
                            this.Builder.HasNewKeyword = this.Builder.IsNew = false;
                            this.Builder.OverriddenMethod = existingFinalizer;
                            var overriddenMethod = new OverrideFinalizerTransformation( this, this.Builder, this._template.ForIntroduction( this.Builder ), this.Tags );

                            addTransformation( this.Builder.ToTransformation() );
                            addTransformation( overriddenMethod );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override, this.Builder );
                        }

                    default:
                        throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
                }
            }
        }
    }
}