// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceIndexerAdvice : IntroduceMemberAdvice<IIndexer, IIndexer, IndexerBuilder>
{
    private readonly PartiallyBoundTemplateMethod? _getTemplate;
    private readonly PartiallyBoundTemplateMethod? _setTemplate;

    public IntroduceIndexerAdvice(
        AdviceConstructorParameters<INamedType> parameters,
        IReadOnlyList<(IType Type, string Name)> indices,
        PartiallyBoundTemplateMethod? getTemplate,
        PartiallyBoundTemplateMethod? setTemplate,
        IntroductionScope scope,
        OverrideStrategy overrideStrategy,
        Action<IIndexerBuilder>? buildAction,
        IObjectReader tags,
        INamedType? explicitlyImplementedInterfaceType )
        : base( parameters, "this[]", template: null, scope, overrideStrategy, buildAction, tags, explicitlyImplementedInterfaceType )
    {
        this._getTemplate = getTemplate;
        this._setTemplate = setTemplate;

        var hasGet = getTemplate != null;
        var hasSet = setTemplate != null;

        this.Builder = new IndexerBuilder( this, parameters.TargetDeclaration, hasGet, hasSet );

        foreach ( var pair in indices )
        {
            this.Builder.AddParameter( pair.Name, pair.Type );
        }
    }

    public override AdviceKind AdviceKind => AdviceKind.IntroduceIndexer;

    protected override void InitializeCore(
        ProjectServiceProvider serviceProvider,
        IDiagnosticAdder diagnosticAdder,
        TemplateAttributeProperties? templateAttributeProperties )
    {
        base.InitializeCore( serviceProvider, diagnosticAdder, templateAttributeProperties );

        if ( this._getTemplate != null )
        {
            var typeRewriter = TemplateTypeRewriter.Get( this._getTemplate );

            this.Builder.Type = typeRewriter.Visit( this._getTemplate.Declaration.ReturnType );
        }
        else if ( this._setTemplate != null )
        {
            var lastRuntimeParameter = this._setTemplate.TemplateMember.TemplateClassMember.RunTimeParameters.LastOrDefault();

            var typeRewriter = TemplateTypeRewriter.Get( this._setTemplate );

            if ( lastRuntimeParameter != null )
            {
                // There may be an invalid template without runtime parameters.

                this.Builder.Type = typeRewriter.Visit( this._setTemplate.Declaration.Parameters[lastRuntimeParameter.SourceIndex].Type );
            }
        }

        this.Builder.Accessibility =
            this._getTemplate != null
                ? this._getTemplate.TemplateMember.Accessibility
                : this._setTemplate.AssertNotNull().TemplateMember.Accessibility;

        if ( this._getTemplate != null )
        {
            CopyTemplateAttributes( this._getTemplate.TemplateMember.Declaration, this.Builder.GetMethod.AssertNotNull(), serviceProvider );

            CopyTemplateAttributes(
                this._getTemplate.TemplateMember.Declaration.ReturnParameter,
                this.Builder.GetMethod!.ReturnParameter,
                serviceProvider );
        }

        if ( this._setTemplate != null )
        {
            CopyTemplateAttributes( this._setTemplate.TemplateMember.Declaration, this.Builder.SetMethod!, serviceProvider );

            var lastRuntimeParameter = this._setTemplate.TemplateMember.TemplateClassMember.RunTimeParameters.LastOrDefault();

            if ( lastRuntimeParameter != null )
            {
                // There may be an invalid template without runtime parameters.

                CopyTemplateAttributes(
                    this._setTemplate.TemplateMember.Declaration.Parameters[lastRuntimeParameter.SourceIndex],
                    this.Builder.SetMethod.AssertNotNull().Parameters.Last(),
                    serviceProvider );
            }

            CopyTemplateAttributes(
                this._setTemplate.TemplateMember.Declaration.ReturnParameter,
                this.Builder.SetMethod.AssertNotNull().ReturnParameter,
                serviceProvider );
        }

        var (accessorTemplateForAttributeCopy, skipLastParameter) =
            this._getTemplate == null
                ? (this._setTemplate!.TemplateMember, true)
                : (this._getTemplate.TemplateMember, false);

        var runtimeParameters = accessorTemplateForAttributeCopy.TemplateClassMember.RunTimeParameters;

        for ( var i = 0; i < runtimeParameters.Length - (skipLastParameter ? 1 : 0); i++ )
        {
            var runtimeParameter = runtimeParameters[i];
            var templateParameter = accessorTemplateForAttributeCopy.Declaration.Parameters[runtimeParameter.SourceIndex];
            var parameterBuilder = this.Builder.Parameters[i];

            CopyTemplateAttributes( templateParameter, parameterBuilder, serviceProvider );
        }

        // TODO: For get accessor template, we are ignoring accessibility of set accessor template because it can be easily incompatible.
    }

    protected override void ValidateBuilder( INamedType targetDeclaration, IDiagnosticAdder diagnosticAdder )
    {
        base.ValidateBuilder( targetDeclaration, diagnosticAdder );

        if ( this.Builder.Parameters.Count <= 0 )
        {
            diagnosticAdder.Report(
                AdviceDiagnosticDescriptors.CannotIntroduceIndexerWithoutParameters.CreateRoslynDiagnostic(
                    targetDeclaration.GetDiagnosticLocation(),
                    (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration),
                    this ) );
        }

        if ( this.Builder.IsStatic )
        {
            diagnosticAdder.Report(
                AdviceDiagnosticDescriptors.CannotIntroduceStaticIndexer.CreateRoslynDiagnostic(
                    targetDeclaration.GetDiagnosticLocation(),
                    (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration),
                    this ) );
        }
    }

    protected override IntroductionAdviceResult<IIndexer> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        var existingDeclaration = targetDeclaration.FindClosestVisibleIndexer( this.Builder );

        // TODO: Introduce attributes that are added not present on the existing member?
        if ( existingDeclaration == null )
        {
            // There is no existing declaration.

            // Introduce and override using the template.
            var overrideIndexerTransformation = new OverrideIndexerTransformation(
                this,
                this.Builder,
                this._getTemplate?.ForIntroduction( this.Builder.GetMethod ),
                this._setTemplate?.ForIntroduction( this.Builder.SetMethod ),
                this.Tags );

            addTransformation( this.Builder.ToTransformation() );
            addTransformation( overrideIndexerTransformation );

            return this.CreateSuccessResult();
        }
        else
        {
            if ( existingDeclaration is not { } existingIndexer )
            {
                return
                    this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration, existingDeclaration.DeclarationKind),
                            this ) );
            }

            if ( !compilation.Comparers.Default.Equals( this.Builder.Type, existingIndexer.Type ) )
            {
                return
                    this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration,
                             existingIndexer.DeclaringType, existingIndexer.Type),
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
                                 existingIndexer.DeclaringType),
                                this ) );

                case OverrideStrategy.Ignore:
                    // Do nothing.
                    return this.CreateIgnoredResult( existingIndexer );

                case OverrideStrategy.New:
                    // If the existing declaration is in the current type, fail, otherwise, declare a new method and override.
                    if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingIndexer.DeclaringType ) )
                    {
                        return this.CreateFailedResult(
                            AdviceDiagnosticDescriptors.CannotIntroduceNewMemberWhenItAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.AspectInstance.AspectClass.ShortName, this.Builder, existingIndexer.DeclaringType),
                                this ) );
                    }
                    else
                    {
                        this.Builder.HasNewKeyword = this.Builder.IsNew = true;
                        this.Builder.OverriddenIndexer = existingIndexer;

                        var overrideIndexerTransformation = new OverrideIndexerTransformation(
                            this,
                            this.Builder,
                            this._getTemplate?.ForIntroduction( this.Builder.GetMethod ),
                            this._setTemplate?.ForIntroduction( this.Builder.SetMethod ),
                            this.Tags );

                        addTransformation( this.Builder.ToTransformation() );
                        addTransformation( overrideIndexerTransformation );

                        return this.CreateSuccessResult( AdviceOutcome.New, this.Builder );
                    }

                case OverrideStrategy.Override:
                    if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingIndexer.DeclaringType ) )
                    {
                        var overrideIndexerTransformation = new OverrideIndexerTransformation(
                            this,
                            existingIndexer,
                            this._getTemplate?.ForIntroduction( existingIndexer.GetMethod ),
                            this._setTemplate?.ForIntroduction( existingIndexer.SetMethod ),
                            this.Tags );

                        addTransformation( overrideIndexerTransformation );

                        return this.CreateSuccessResult( AdviceOutcome.Override, existingIndexer );
                    }
                    else if ( existingIndexer.IsSealed || !existingIndexer.IsOverridable() )
                    {
                        return
                            this.CreateFailedResult(
                                AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration,
                                     existingIndexer.DeclaringType),
                                    this ) );
                    }
                    else
                    {
                        this.Builder.IsOverride = true;
                        this.Builder.HasNewKeyword = this.Builder.IsNew = false;
                        this.Builder.OverriddenIndexer = existingIndexer;

                        var overriddenIndexer = new OverrideIndexerTransformation(
                            this,
                            this.Builder,
                            this._getTemplate?.ForIntroduction( this.Builder.GetMethod ),
                            this._setTemplate?.ForIntroduction( this.Builder.SetMethod ),
                            this.Tags );

                        addTransformation( this.Builder.ToTransformation() );
                        addTransformation( overriddenIndexer );

                        return this.CreateSuccessResult( AdviceOutcome.Override, this.Builder );
                    }

                default:
                    throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
            }
        }
    }
}