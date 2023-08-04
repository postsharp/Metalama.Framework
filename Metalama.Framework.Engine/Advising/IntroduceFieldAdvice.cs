// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising
{
    // ReSharper disable once UnusedType.Global
    // TODO: Use this type and remove the warning waiver.

    internal sealed class IntroduceFieldAdvice : IntroduceMemberAdvice<IField, FieldBuilder>
    {
        public IntroduceFieldAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            ICompilation sourceCompilation,
            string? explicitName,
            TemplateMember<IField>? fieldTemplate,
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

        protected override void InitializeCore(
            ProjectServiceProvider serviceProvider,
            IDiagnosticAdder diagnosticAdder,
            TemplateAttributeProperties? templateAttributeProperties )
        {
            base.InitializeCore( serviceProvider, diagnosticAdder, templateAttributeProperties );

            this.Builder.IsRequired = templateAttributeProperties?.IsRequired ?? this.Template?.Declaration.IsRequired ?? false;

            if ( this.Template != null )
            {
                this.Builder.Type = this.Template.Declaration.Type;
                this.Builder.Writeability = this.Template.Declaration.Writeability;
            }
            else
            {
                this.Builder.Type = this.SourceCompilation.GetCompilationModel().Factory.GetSpecialType( SpecialType.Object );
                this.Builder.Writeability = Writeability.All;
            }

            var targetType = this.TargetDeclaration.GetTarget( this.SourceCompilation );

            if ( targetType.TypeKind is TypeKind.Struct or TypeKind.RecordStruct && targetType.GetSymbol().IsReadOnly )
            {
                this.Builder.Writeability = Writeability.ConstructorOnly;
            }
        }

        public override AdviceKind AdviceKind => AdviceKind.IntroduceField;

        public override AdviceImplementationResult Implement(
            ProjectServiceProvider serviceProvider,
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
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration, existingDeclaration.DeclarationKind),
                                this ) );
                }

                if ( existingDeclaration.IsStatic != this.Builder.IsStatic )
                {
                    return
                        AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                 existingDeclaration.DeclaringType),
                                this ) );
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
                                     existingDeclaration.DeclaringType),
                                    this ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceImplementationResult.Ignored( existingDeclaration );

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            return AdviceImplementationResult.Failed(
                                AdviceDiagnosticDescriptors.CannotIntroduceNewMemberWhenItAlreadyExists.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, this.Builder, existingDeclaration.DeclaringType),
                                    this ) );
                        }
                        else
                        {
                            this.Builder.HasNewKeyword = this.Builder.IsNew = true;
                            addTransformation( this.Builder.ToTransformation() );

                            return AdviceImplementationResult.Success( AdviceOutcome.New, this.Builder );
                        }

                    case OverrideStrategy.Override:
                        throw new NotSupportedException( "Override is not a supported OverrideStrategy for fields." );

                    default:
                        throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
                }
            }
            else
            {
                addTransformation( this.Builder.ToTransformation() );

                OverrideHelper.AddTransformationsForStructField(
                    targetDeclaration.ForCompilation( compilation ),
                    this,
                    addTransformation /* We add an initializer if it does not have one */ );

                return AdviceImplementationResult.Success( AdviceOutcome.Default, this.Builder );
            }
        }
    }
}