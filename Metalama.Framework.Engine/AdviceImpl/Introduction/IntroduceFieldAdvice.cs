// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceFieldAdvice : IntroduceMemberAdvice<IField, IField, FieldBuilder>
{
    public IntroduceFieldAdvice(
        AdviceConstructorParameters<INamedType> parameters,
        string? explicitName,
        TemplateMember<IField>? fieldTemplate,
        IntroductionScope scope,
        OverrideStrategy overrideStrategy,
        Action<IFieldBuilder>? buildAction,
        IObjectReader tags )
        : base( parameters, explicitName, fieldTemplate, scope, overrideStrategy, buildAction, tags, explicitlyImplementedInterfaceType: null )
    {
        this.Builder = new FieldBuilder( this, parameters.TargetDeclaration, this.MemberName, tags );
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
            this.Builder.Type = this.SourceCompilation.GetCompilationModel().Cache.SystemObjectType;
            this.Builder.Writeability = Writeability.All;
        }
    }

    protected override void ValidateBuilder( INamedType targetDeclaration, IDiagnosticAdder diagnosticAdder )
    {
        if ( targetDeclaration.TypeKind is TypeKind.Struct or TypeKind.RecordStruct && targetDeclaration.IsReadOnly )
        {
            this.Builder.Writeability = Writeability.ConstructorOnly;
        }
    }

    public override AdviceKind AdviceKind => AdviceKind.IntroduceField;

    protected override IntroductionAdviceResult<IField> Implement(
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
                    this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration, existingDeclaration.DeclarationKind),
                            this ) );
            }

            if ( existingDeclaration.IsStatic != this.Builder.IsStatic )
            {
                return
                    this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration,
                             existingDeclaration.DeclaringType),
                            this ) );
            }

            switch ( this.OverrideStrategy )
            {
                case OverrideStrategy.Fail:
                    // Produce fail diagnostic.
                    return
                        this.CreateFailedResult(
                            AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration,
                                 existingDeclaration.DeclaringType),
                                this ) );

                case OverrideStrategy.Ignore:
                    // Do nothing.
                    return this.CreateIgnoredResult( existingDeclaration );

                case OverrideStrategy.New:
                    // If the existing declaration is in the current type, fail, otherwise, declare a new method and override.
                    if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                    {
                        return this.CreateFailedResult(
                            AdviceDiagnosticDescriptors.CannotIntroduceNewMemberWhenItAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.AspectInstance.AspectClass.ShortName, this.Builder, existingDeclaration.DeclaringType),
                                this ) );
                    }
                    else
                    {
                        this.Builder.HasNewKeyword = this.Builder.IsNew = true;
                        addTransformation( this.Builder.ToTransformation() );

                        return this.CreateSuccessResult( AdviceOutcome.New, this.Builder );
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

            return this.CreateSuccessResult( AdviceOutcome.Default, this.Builder );
        }
    }
}