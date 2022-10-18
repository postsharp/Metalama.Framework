// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class IntroduceFinalizerAdvice : IntroduceMemberAdvice<IMethod, MethodBuilder>
    {
        public BoundTemplateMethod BoundTemplate { get; }

        public new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        public IntroduceFinalizerAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            ICompilation sourceCompilation,
            BoundTemplateMethod boundTemplate,
            OverrideStrategy overrideStrategy,
            string? layerName,
            IObjectReader tags )
            : base(
                aspect,
                templateInstance,
                targetDeclaration,
                sourceCompilation,
                null,
                boundTemplate.Template,
                IntroductionScope.Instance,
                overrideStrategy,
                _ => { },
                layerName,
                tags )
        {
            this.BoundTemplate = boundTemplate;

            this.Builder = new MethodBuilder( targetDeclaration, "Finalize", this, DeclarationKind.Finalizer );
        }

        protected override void InitializeCore( IServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
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
            base.InitializeCore( serviceProvider, diagnosticAdder );
        }

        public override AdviceImplementationResult Implement(
            IServiceProvider serviceProvider,
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
                var overriddenMethod = new OverrideMethodTransformation( this, this.Builder, this.BoundTemplate, this.Tags );
                this.Builder.IsOverride = false;
                this.Builder.IsNew = false;

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
                        if ( ((IEqualityComparer<IType>) compilation.InvariantComparer).Equals( targetDeclaration, existingFinalizer.DeclaringType ) )
                        {
                            var overriddenMethod = new OverrideMethodTransformation( this, existingFinalizer, this.BoundTemplate, this.Tags );
                            addTransformation( overriddenMethod );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override );
                        }
                        else
                        {
                            this.Builder.IsOverride = true;
                            this.Builder.IsNew = false;
                            this.Builder.OverriddenMethod = existingFinalizer;
                            var overriddenMethod = new OverrideMethodTransformation( this, this.Builder, this.BoundTemplate, this.Tags );

                            addTransformation( this.Builder.ToTransformation() );
                            addTransformation( overriddenMethod );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override );
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}