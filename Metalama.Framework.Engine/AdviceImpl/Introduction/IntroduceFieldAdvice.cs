// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.Diagnostics;
using System;

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
        : base( parameters, explicitName, fieldTemplate, scope, overrideStrategy, buildAction, tags, explicitlyImplementedInterfaceType: null ) { }

    protected override FieldBuilder CreateBuilder( in AdviceImplementationContext context )
    {
        return new FieldBuilder( this.AspectLayerInstance, this.TargetDeclaration, this.MemberName, this.Tags )
        {
            InitializerTemplate = this.Template.GetInitializerTemplate()
        };
    }

    protected override void InitializeBuilderCore(
        FieldBuilder builder,
        TemplateAttributeProperties? templateAttributeProperties,
        in AdviceImplementationContext context )
    {
        base.InitializeBuilderCore( builder, templateAttributeProperties, in context );

        var templateDeclaration = this.Template?.DeclarationRef.GetTarget( this.SourceCompilation );
        builder.IsRequired = templateAttributeProperties?.IsRequired ?? templateDeclaration?.IsRequired ?? false;

        if ( this.Template != null )
        {
            builder.Type = templateDeclaration.AssertNotNull().Type;
            builder.Writeability = templateDeclaration.Writeability;
        }
        else
        {
            builder.Type = this.SourceCompilation.Cache.SystemObjectType;
            builder.Writeability = Writeability.All;
        }
    }

    protected override void ValidateBuilder( FieldBuilder builder, INamedType targetDeclaration, IDiagnosticAdder diagnosticAdder )
    {
        if ( targetDeclaration.TypeKind is TypeKind.Struct or TypeKind.RecordStruct && targetDeclaration.IsReadOnly )
        {
            builder.Writeability = Writeability.ConstructorOnly;
        }
    }

    public override AdviceKind AdviceKind => AdviceKind.IntroduceField;

    protected override IntroductionAdviceResult<IField> ImplementCore( FieldBuilder builder, in AdviceImplementationContext context )
    {
        var targetDeclaration = this.TargetDeclaration;
        var existingDeclaration = targetDeclaration.FindClosestUniquelyNamedMember( builder.Name );

        if ( existingDeclaration != null )
        {
            if ( existingDeclaration is not IField )
            {
                return
                    this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration, existingDeclaration.DeclarationKind),
                            this ) );
            }

            if ( existingDeclaration.IsStatic != builder.IsStatic )
            {
                return
                    this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration,
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
                                (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration,
                                 existingDeclaration.DeclaringType),
                                this ) );

                case OverrideStrategy.Ignore:
                    // Do nothing.
                    return this.CreateIgnoredResult( existingDeclaration );

                case OverrideStrategy.New:
                    // If the existing declaration is in the current type, fail, otherwise, declare a new method and override.
                    if ( targetDeclaration.Equals( existingDeclaration.DeclaringType ) )
                    {
                        return this.CreateFailedResult(
                            AdviceDiagnosticDescriptors.CannotIntroduceNewMemberWhenItAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.AspectInstance.AspectClass.ShortName, builder, existingDeclaration.DeclaringType),
                                this ) );
                    }
                    else
                    {
                        builder.HasNewKeyword = builder.IsNew = true;
                        context.AddTransformation( builder.ToTransformation() );

                        return this.CreateSuccessResult( AdviceOutcome.New, builder );
                    }

                case OverrideStrategy.Override:
                    throw new NotSupportedException( "Override is not a supported OverrideStrategy for fields." );

                default:
                    throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
            }
        }
        else
        {
            context.AddTransformation( builder.ToTransformation() );

            OverrideHelper.AddTransformationsForStructField(
                targetDeclaration,
                this.AspectLayerInstance,
                context.AddTransformation /* We add an initializer if it does not have one */ );

            return this.CreateSuccessResult( AdviceOutcome.Default, builder );
        }
    }
}