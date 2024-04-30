// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising;

internal sealed class IntroduceConstructorAdvice : IntroduceMemberAdvice<IMethod, IConstructor, ConstructorBuilder>
{
    private readonly PartiallyBoundTemplateMethod _template;

    private new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

    public IntroduceConstructorAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance templateInstance,
        INamedType targetDeclaration,
        ICompilation sourceCompilation,
        PartiallyBoundTemplateMethod template,
        IntroductionScope scope,
        OverrideStrategy overrideStrategy,
        Action<IConstructorBuilder>? buildAction,
        string? layerName,
        IObjectReader tags )
        : base(
            aspect,
            templateInstance,
            targetDeclaration,
            sourceCompilation,
            null,
            template.TemplateMember,
            scope,
            overrideStrategy,
            buildAction,
            layerName,
            tags )
    {
        this._template = template;

        this.Builder = new ConstructorBuilder(this, targetDeclaration );
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
                        (this.Aspect.AspectClass.ShortName, targetDeclaration, this.OverrideStrategy),
                        this ) );

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

        var existingConstructor = targetDeclaration.Constructors.OfExactSignature( this.Builder );

        // TODO: Introduce attributes that are added not present on the existing member?
        if ( existingConstructor == null )
        {
            // Check that there is no other member named the same, which is possible, but very unlikely.
            var existingOtherMember = targetDeclaration.FindClosestUniquelyNamedMember( this.Builder.Name );

            if ( existingOtherMember != null )
            {
                return
                    AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration, existingOtherMember.DeclarationKind),
                            this ) );
            }

            // There is no existing declaration, we will introduce and override the introduced.
            var overriddenConstructor = new OverrideConstructorTransformation( this, this.Builder, this._template.ForIntroduction( this.Builder ), this.Tags );
            this.Builder.IsOverride = false;
            this.Builder.HasNewKeyword = this.Builder.IsNew = false;

            addTransformation( this.Builder.ToTransformation() );
            addTransformation( overriddenConstructor );

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
                                 existingConstructor.DeclaringType),
                                this ) );

                case OverrideStrategy.Ignore:
                    // Do nothing.
                    return AdviceImplementationResult.Ignored( existingConstructor );

                case OverrideStrategy.Override:
                    var overriddenMethod = new OverrideConstructorTransformation(
                        this,
                        existingConstructor,
                        this._template.ForIntroduction( existingConstructor ),
                        this.Tags );

                    addTransformation( overriddenMethod );

                    return AdviceImplementationResult.Success( AdviceOutcome.Override, existingConstructor );

                default:
                    throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
            }
        }
    }
}