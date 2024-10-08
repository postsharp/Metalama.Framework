﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceMethodAdvice : IntroduceMemberAdvice<IMethod, IMethod, MethodBuilder>
{
    private readonly PartiallyBoundTemplateMethod _template;

    public IntroduceMethodAdvice(
        AdviceConstructorParameters<INamedType> parameters,
        PartiallyBoundTemplateMethod template,
        IntroductionScope scope,
        OverrideStrategy overrideStrategy,
        Action<IMethodBuilder>? buildAction,
        IObjectReader tags,
        INamedType? explicitlyImplementedInterfaceType )
        : base( parameters, explicitName: null, template.TemplateMember, scope, overrideStrategy, buildAction, tags, explicitlyImplementedInterfaceType )
    {
        this._template = template;
    }

    protected override MethodBuilder CreateBuilder( in AdviceImplementationContext context )
    {
        return new MethodBuilder( this, this.TargetDeclaration, this.MemberName );
    }

    protected override void InitializeBuilderCore(
        MethodBuilder builder,
        TemplateAttributeProperties? templateAttributeProperties,
        in AdviceImplementationContext context )
    {
        base.InitializeBuilderCore( builder, templateAttributeProperties, in context );

        var templateDeclaration = this.Template.AssertNotNull().DeclarationRef.GetTarget( this.SourceCompilation );

        var serviceProvider = context.ServiceProvider;

        builder.IsAsync = templateDeclaration.IsAsync;

        var typeRewriter = TemplateTypeRewriter.Get( this._template );

        // Handle iterator info.
        builder.SetIsIteratorMethod( this.Template.IsIteratorMethod );

        // Handle return type.

        if ( templateDeclaration.ReturnParameter.Type.TypeKind == TypeKind.Dynamic )
        {
            // Templates with dynamic return value result in object return type of the introduced member.
            builder.ReturnParameter.Type = builder.Compilation.Cache.SystemObjectType;
        }
        else
        {
            builder.ReturnParameter.Type = typeRewriter.Visit( templateDeclaration.ReturnParameter.Type );

            if ( templateDeclaration.ReturnParameter.RefKind != RefKind.None )
            {
                throw new InvalidOperationException(
                    MetalamaStringFormatter.Format(
                        $"The '{this.AspectInstance.AspectClass.ShortName}' cannot introduce the method '{builder}' because methods returning 'ref' are not supported." ) );
            }
        }

        CopyTemplateAttributes( templateDeclaration.ReturnParameter, builder.ReturnParameter, serviceProvider );

        var runtimeParameters = this.Template.AssertNotNull().TemplateClassMember.RunTimeParameters;

        foreach ( var runtimeParameter in runtimeParameters )
        {
            var templateParameter = templateDeclaration.Parameters[runtimeParameter.SourceIndex];

            var parameterBuilder = builder.AddParameter(
                templateParameter.Name,
                typeRewriter.Visit( templateParameter.Type ),
                templateParameter.RefKind,
                templateParameter.DefaultValue );

            CopyTemplateAttributes( templateParameter, parameterBuilder, serviceProvider );
        }

        var runtimeTypeParameters = this.Template.AssertNotNull().TemplateClassMember.RunTimeTypeParameters;

        foreach ( var runtimeTypeParameter in runtimeTypeParameters )
        {
            var templateTypeParameter = templateDeclaration.TypeParameters[runtimeTypeParameter.SourceIndex];
            var typeParameterBuilder = builder.AddTypeParameter( templateTypeParameter.Name );
            typeParameterBuilder.Variance = templateTypeParameter.Variance;
            typeParameterBuilder.HasDefaultConstructorConstraint = templateTypeParameter.HasDefaultConstructorConstraint;
            typeParameterBuilder.TypeKindConstraint = templateTypeParameter.TypeKindConstraint;

            foreach ( var templateGenericParameterConstraint in templateTypeParameter.TypeConstraints )
            {
                typeParameterBuilder.AddTypeConstraint( typeRewriter.Visit( templateGenericParameterConstraint ) );
            }

            CopyTemplateAttributes( templateTypeParameter, typeParameterBuilder, serviceProvider );
        }
    }

    public override AdviceKind AdviceKind => AdviceKind.IntroduceMethod;

    protected override IntroductionAdviceResult<IMethod> ImplementCore( MethodBuilder builder, in AdviceImplementationContext context )
    {
        // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
        var targetDeclaration = this.TargetDeclaration;
        var existingMethod = targetDeclaration.FindClosestVisibleMethod( builder );

        // TODO: Introduce attributes that are added not present on the existing member?
        if ( existingMethod == null )
        {
            // Check that there is no other member named the same, otherwise we cannot add a method.
            var existingOtherMember =
                builder is { Name: "Finalize", Parameters.Count: 0, TypeParameters.Count: 0 }
                    ? targetDeclaration.Finalizer
                    : targetDeclaration.FindClosestUniquelyNamedMember( builder.Name );

            if ( existingOtherMember != null )
            {
                return
                    this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration, existingOtherMember.DeclarationKind),
                            this ) );
            }

            // There is no existing declaration, we will introduce and override the introduced.
            var overriddenMethod = new OverrideMethodTransformation( this, builder.ToFullRef(), this._template.ForIntroduction( builder ), this.Tags );
            builder.IsOverride = false;
            builder.HasNewKeyword = builder.IsNew = false;
            builder.Freeze();

            context.AddTransformation( builder.ToTransformation() );
            context.AddTransformation( overriddenMethod );

            return this.CreateSuccessResult( AdviceOutcome.Default, builder );
        }
        else
        {
            if ( existingMethod.IsStatic != builder.IsStatic )
            {
                return
                    this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration,
                             existingMethod.DeclaringType),
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
                                 existingMethod.DeclaringType),
                                this ) );

                case OverrideStrategy.Ignore:
                    // Do nothing.
                    return this.CreateIgnoredResult( existingMethod );

                case OverrideStrategy.New:
                    // If the existing declaration is in the current type, fail, otherwise, declare a new method and override.
                    if ( targetDeclaration.Equals( existingMethod.DeclaringType ) )
                    {
                        return this.CreateFailedResult(
                            AdviceDiagnosticDescriptors.CannotIntroduceNewMemberWhenItAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.AspectInstance.AspectClass.ShortName, builder, existingMethod.DeclaringType),
                                this ) );
                    }
                    else
                    {
                        builder.HasNewKeyword = builder.IsNew = true;
                        builder.IsOverride = false;
                        builder.OverriddenMethod = existingMethod;
                        builder.Freeze();

                        var overriddenMethod = new OverrideMethodTransformation(
                            this,
                            builder.ToFullRef(),
                            this._template.ForIntroduction( builder ),
                            this.Tags );

                        context.AddTransformation( overriddenMethod );
                        context.AddTransformation( builder.ToTransformation() );

                        return this.CreateSuccessResult( AdviceOutcome.New, builder );
                    }

                case OverrideStrategy.Override:
                    if ( !builder.ReturnType.Is( builder.ReturnType, ConversionKind.Reference ) )
                    {
                        return
                            this.CreateFailedResult(
                                AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration,
                                     existingMethod.DeclaringType, existingMethod.ReturnType),
                                    this ) );
                    }
                    else if ( existingMethod.IsSealed || !existingMethod.IsOverridable() )
                    {
                        return
                            this.CreateFailedResult(
                                AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration,
                                     existingMethod.DeclaringType),
                                    this ) );
                    }
                    else
                    {
                        builder.IsOverride = true;
                        builder.HasNewKeyword = builder.IsNew = false;
                        builder.OverriddenMethod = existingMethod;
                        builder.Freeze();

                        var overriddenMethod = new OverrideMethodTransformation(
                            this,
                            builder.ToFullRef(),
                            this._template.ForIntroduction( builder ),
                            this.Tags );

                        context.AddTransformation( builder.ToTransformation() );
                        context.AddTransformation( overriddenMethod );

                        return this.CreateSuccessResult( AdviceOutcome.Override, builder );
                    }

                default:
                    throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
            }
        }
    }
}