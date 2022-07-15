﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising
{
    // ReSharper disable once UnusedType.Global
    // TODO: Use this type and remove the warning waiver.

    internal class IntroduceFieldAdvice : IntroduceMemberAdvice<IField, FieldBuilder>
    {
        public IntroduceFieldAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            ICompilation sourceCompilation,
            string? explicitName,
            TemplateMember<IField> fieldTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            Action<IFieldBuilder>? buildAction,
            string? layerName,
            IObjectReader tags )
            : base(
                aspect,
                templateInstance,
                targetDeclaration,
                sourceCompilation,
                explicitName,
                fieldTemplate,
                scope,
                overrideStrategy,
                buildAction,
                layerName,
                tags )
        {
            this.Builder = new FieldBuilder( this, targetDeclaration, this.MemberName, tags );
            this.Builder.InitializerTemplate = fieldTemplate.GetInitializerTemplate();
        }

        protected override void InitializeCore( IServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            base.InitializeCore( serviceProvider, diagnosticAdder );

            if ( !this.Template.IsNull )
            {
                this.Builder.Type = this.Template.Declaration!.Type;
                this.Builder.Accessibility = this.Template.Declaration!.Accessibility;
                this.Builder.IsStatic = this.Template.Declaration!.IsStatic;
                this.Builder.Writeability = this.Template.Declaration!.Writeability;
            }
            else
            {
                this.Builder.Type = this.SourceCompilation.GetCompilationModel().Factory.GetSpecialType( SpecialType.Object );
                this.Builder.Accessibility = Accessibility.Private;
                this.Builder.IsStatic = false;
                this.Builder.Writeability = Writeability.All;
            }
        }

        public override AdviceImplementationResult Implement(
            IServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );
            var existingDeclaration = targetDeclaration.FindClosestUniquelyNamedMember( this.Builder.Name );

            if ( existingDeclaration != null )
            {
                if ( existingDeclaration is not IField )
                {
                    return
                        AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration, existingDeclaration.DeclarationKind) ) );
                }

                if ( existingDeclaration.IsStatic != this.Builder.IsStatic )
                {
                    return
                        AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                 existingDeclaration.DeclaringType) ) );
                }

                switch ( this.OverrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Produce fail diagnostic.
                        return
                            AdviceImplementationResult.Failed(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                     existingDeclaration.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceImplementationResult.Ignored;

                    case OverrideStrategy.New:
                        this.Builder.IsNew = true;
                        addTransformation( this.Builder );

                        return AdviceImplementationResult.Success( AdviceOutcome.New, this.Builder );

                    case OverrideStrategy.Override:
                        throw new NotSupportedException( "Override is not a supported OverrideStrategy for fields." );

                    default:
                        throw new AssertionFailedException();
                }
            }
            else
            {
                addTransformation( this.Builder );

                OverrideHelper.AddTransformationsForStructField(
                    targetDeclaration,
                    this,
                    addTransformation /* We add an initializer if it does not have one */ );

                return AdviceImplementationResult.Success( AdviceOutcome.Default, this.Builder );
            }
        }
    }
}