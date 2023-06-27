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

internal sealed class IntroduceMethodAdvice : IntroduceMemberAdvice<IMethod, MethodBuilder>
{
    private readonly PartiallyBoundTemplateMethod _template;

    private new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

    public IntroduceMethodAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance templateInstance,
        INamedType targetDeclaration,
        ICompilation sourceCompilation,
        PartiallyBoundTemplateMethod template,
        IntroductionScope scope,
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
            scope,
            overrideStrategy,
            buildAction,
            layerName,
            tags )
    {
        this._template = template;

        this.Builder = new MethodBuilder( this, targetDeclaration, this.MemberName );
    }

    protected override void InitializeCore(
        ProjectServiceProvider serviceProvider,
        IDiagnosticAdder diagnosticAdder,
        TemplateAttributeProperties? templateAttributeProperties )
    {
        base.InitializeCore( serviceProvider, diagnosticAdder, templateAttributeProperties );

        this.Builder.IsAsync = this.Template!.Declaration.IsAsync;

        var typeRewriter = TemplateTypeRewriter.Get( this._template );

        // Handle iterator info.
        this.Builder.SetIsIteratorMethod( this.Template.IsIteratorMethod );

        // Handle return type.
        if ( this.Template.Declaration.ReturnParameter.Type.TypeKind == TypeKind.Dynamic )
        {
            // Templates with dynamic return value result in object return type of the introduced member.
            this.Builder.ReturnParameter.Type = this.Builder.Compilation.Factory.GetTypeByReflectionType( typeof(object) );
        }
        else
        {
            this.Builder.ReturnParameter.Type = typeRewriter.Visit( this.Template.Declaration.ReturnParameter.Type );

            if ( this.Template.Declaration.ReturnParameter.RefKind != RefKind.None )
            {
                throw new InvalidOperationException(
                    MetalamaStringFormatter.Format(
                        $"The '{this.Aspect.AspectClass.ShortName}' cannot introduce the method '{this.Builder}' because methods returning 'ref' are not supported." ) );
            }
        }

        CopyTemplateAttributes( this.Template.Declaration.ReturnParameter, this.Builder.ReturnParameter, serviceProvider );

        var runtimeParameters = this.Template.AssertNotNull().TemplateClassMember.RunTimeParameters;

        foreach ( var runtimeParameter in runtimeParameters )
        {
            var templateParameter = this.Template.AssertNotNull().Declaration.Parameters[runtimeParameter.SourceIndex];

            var parameterBuilder = this.Builder.AddParameter(
                templateParameter.Name,
                typeRewriter.Visit( templateParameter.Type ),
                templateParameter.RefKind,
                templateParameter.DefaultValue );

            CopyTemplateAttributes( templateParameter, parameterBuilder, serviceProvider );
        }

        var runtimeTypeParameters = this.Template.AssertNotNull().TemplateClassMember.RunTimeTypeParameters;

        foreach ( var runtimeTypeParameter in runtimeTypeParameters )
        {
            var templateTypeParameter = this.Template.AssertNotNull().Declaration.TypeParameters[runtimeTypeParameter.SourceIndex];
            var typeParameterBuilder = this.Builder.AddTypeParameter( templateTypeParameter.Name );
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

    public override AdviceImplementationResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );
        var existingMethod = targetDeclaration.FindClosestVisibleMethod( this.Builder );

        // TODO: Introduce attributes that are added not present on the existing member?
        if ( existingMethod == null )
        {
            // Check that there is no other member named the same, otherwise we cannot add a method.
            var existingOtherMember =
                this.Builder is { Name: "Finalize", Parameters.Count: 0, TypeParameters.Count: 0 }
                    ? targetDeclaration.Finalizer
                    : targetDeclaration.FindClosestUniquelyNamedMember( this.Builder.Name );

            if ( existingOtherMember != null )
            {
                return
                    AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration, existingOtherMember.DeclarationKind) ) );
            }

            // There is no existing declaration, we will introduce and override the introduced.
            var overriddenMethod = new OverrideMethodTransformation( this, this.Builder, this._template.ForIntroduction( this.Builder ), this.Tags );
            this.Builder.IsOverride = false;
            this.Builder.HasNewKeyword = this.Builder.IsNew = false;

            addTransformation( this.Builder.ToTransformation() );
            addTransformation( overriddenMethod );

            return AdviceImplementationResult.Success( this.Builder );
        }
        else
        {
            if ( existingMethod.IsStatic != this.Builder.IsStatic )
            {
                return
                    AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                             existingMethod.DeclaringType) ) );
            }
            else if ( !compilation.Comparers.Default.Is(
                         this.Builder.ReturnType,
                         existingMethod.ReturnType,
                         ConversionKind.DenyBoxing ) )
            {
                return
                    AdviceImplementationResult.Failed(
                        AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                             existingMethod.DeclaringType, existingMethod.ReturnType) ) );
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
                                 existingMethod.DeclaringType) ) );

                case OverrideStrategy.Ignore:
                    // Do nothing.
                    return AdviceImplementationResult.Ignored;

                case OverrideStrategy.New:
                    // If the existing declaration is in the current type, fail, otherwise, declare a new method and override.
                    if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingMethod.DeclaringType ) )
                    {
                        return AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceNewMemberWhenItAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, existingMethod.DeclaringType) ) );
                    }
                    else
                    {
                        this.Builder.HasNewKeyword = this.Builder.IsNew = true;
                        this.Builder.IsOverride = false;
                        this.Builder.OverriddenMethod = existingMethod;

                        var overriddenMethod = new OverrideMethodTransformation( this, this.Builder, this._template.ForIntroduction( this.Builder ), this.Tags );

                        addTransformation( overriddenMethod );
                        addTransformation( this.Builder.ToTransformation() );

                        return AdviceImplementationResult.Success( AdviceOutcome.New, this.Builder );
                    }

                case OverrideStrategy.Override:
                    if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingMethod.DeclaringType ) )
                    {
                        var overriddenMethod = new OverrideMethodTransformation( this, existingMethod, this._template.ForIntroduction( existingMethod ), this.Tags );

                        addTransformation( overriddenMethod );

                        return AdviceImplementationResult.Success( AdviceOutcome.Override, existingMethod );
                    }
                    else if ( existingMethod.IsSealed || !existingMethod.IsOverridable() )
                    {
                        return
                            AdviceImplementationResult.Failed(
                                AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                     existingMethod.DeclaringType) ) );
                    }
                    else
                    {
                        this.Builder.IsOverride = true;
                        this.Builder.HasNewKeyword = this.Builder.IsNew = false;
                        this.Builder.OverriddenMethod = existingMethod;
                        var overriddenMethod = new OverrideMethodTransformation( this, this.Builder, this._template.ForIntroduction( this.Builder ), this.Tags );

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