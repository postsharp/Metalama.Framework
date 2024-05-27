// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

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

        this.Builder = new ConstructorBuilder( this, targetDeclaration );
    }

    protected override void InitializeCore(
        ProjectServiceProvider serviceProvider,
        IDiagnosticAdder diagnosticAdder,
        TemplateAttributeProperties? templateAttributeProperties )
    {
        base.InitializeCore( serviceProvider, diagnosticAdder, templateAttributeProperties );

        var typeRewriter = TemplateTypeRewriter.Get( this._template );

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
    }

    public override AdviceKind AdviceKind => AdviceKind.IntroduceConstructor;

    protected override IntroductionAdviceResult<IConstructor> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        var existingImplicitConstructor = targetDeclaration.Constructors.FirstOrDefault( c => c.IsImplicitInstanceConstructor() );
        var existingConstructor = targetDeclaration.Constructors.OfExactSignature( this.Builder );

        // TODO: Introduce attributes that are added not present on the existing member?
        if ( existingConstructor == null || existingImplicitConstructor != null )
        {
            if ( existingImplicitConstructor != null )
            {
                this.Builder.ReplacedImplicit = existingImplicitConstructor.ToTypedRef();
            }

            // There is no existing declaration, we will introduce and override the introduced.
            var overriddenConstructor = new OverrideConstructorTransformation( this, this.Builder, this._template.ForIntroduction( this.Builder ), this.Tags );

            addTransformation( this.Builder.ToTransformation() );
            addTransformation( overriddenConstructor );

            return this.CreateSuccessResult();
        }
        else
        {
            switch ( this.OverrideStrategy )
            {
                case OverrideStrategy.Fail:
                    // Produce fail diagnostic.
                    return
                        this.CreateFailedResult(
                            AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration,
                                 existingConstructor.DeclaringType),
                                this ) );

                case OverrideStrategy.Ignore:
                    // Do nothing.
                    return this.CreateIgnoredResult( existingConstructor );

                case OverrideStrategy.Override:
                    var overriddenMethod = new OverrideConstructorTransformation(
                        this,
                        existingConstructor,
                        this._template.ForIntroduction( existingConstructor ),
                        this.Tags );

                    addTransformation( overriddenMethod );

                    return this.CreateSuccessResult( AdviceOutcome.Override, existingConstructor );

                default:
                    throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
            }
        }
    }
}