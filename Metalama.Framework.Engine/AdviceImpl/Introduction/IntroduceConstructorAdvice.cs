// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;
using Metalama.Framework.Engine.CodeModel.Helpers;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceConstructorAdvice : IntroduceMemberAdvice<IMethod, IConstructor, ConstructorBuilder>
{
    private readonly PartiallyBoundTemplateMethod _template;

    public IntroduceConstructorAdvice(
        AdviceConstructorParameters<INamedType> parameters,
        PartiallyBoundTemplateMethod template,
        OverrideStrategy overrideStrategy,
        Action<IConstructorBuilder>? buildAction,
        IObjectReader tags )
        : base(
            parameters,
            null,
            template.TemplateMember,
            IntroductionScope.Instance,
            overrideStrategy,
            buildAction,
            tags,
            explicitlyImplementedInterfaceType: null )
    {
        this._template = template;
    }

    protected override ConstructorBuilder CreateBuilder( in AdviceImplementationContext context )
    {
        return new ConstructorBuilder( this, this.TargetDeclaration );
    }

    protected override void InitializeBuilderCore(
        ConstructorBuilder builder,
        TemplateAttributeProperties? templateAttributeProperties,
        in AdviceImplementationContext context )
    {
        base.InitializeBuilderCore( builder, templateAttributeProperties, in context );

        var typeRewriter = TemplateTypeRewriter.Get( this._template );

        var runtimeParameters = this.Template.AssertNotNull().TemplateClassMember.RunTimeParameters;

        foreach ( var runtimeParameter in runtimeParameters )
        {
            var templateParameter = this.Template.AssertNotNull().Declaration.Parameters[runtimeParameter.SourceIndex];

            var parameterBuilder = builder.AddParameter(
                templateParameter.Name,
                typeRewriter.Visit( templateParameter.Type ),
                templateParameter.RefKind,
                templateParameter.DefaultValue );

            CopyTemplateAttributes( templateParameter, parameterBuilder, context.ServiceProvider );
        }
    }

    public override AdviceKind AdviceKind => AdviceKind.IntroduceConstructor;

    protected override IntroductionAdviceResult<IConstructor> ImplementCore( ConstructorBuilder builder, in AdviceImplementationContext context )
    {
        // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
        var targetDeclaration = this.TargetDeclaration;

        var existingImplicitConstructor =
            builder.IsStatic
                ? targetDeclaration.StaticConstructor?.IsImplicitlyDeclared == true
                    ? targetDeclaration.StaticConstructor
                    : null
                : targetDeclaration.Constructors.FirstOrDefault( c => c.IsImplicitInstanceConstructor() );

        var existingConstructor =
            builder.IsStatic
                ? targetDeclaration.StaticConstructor
                : targetDeclaration.Constructors.OfExactSignature( builder );

        // TODO: Introduce attributes that are added not present on the existing member?
        if ( existingConstructor == null || existingImplicitConstructor != null )
        {
            if ( existingImplicitConstructor != null && builder.Parameters.Count == 0 )
            {
                // Redirect if the builder has no parameters and the existing constructor is implicit.
                builder.ReplacedImplicitConstructor = existingImplicitConstructor;
            }

            builder.Freeze();
            
            // There is no existing declaration, we will introduce and override the introduced.
            
            
            var overriddenConstructor = new OverrideConstructorTransformation( this, builder.ToRef(), this._template.ForIntroduction( builder ), this.Tags );
            context.AddTransformation( builder.ToTransformation() );
            context.AddTransformation( overriddenConstructor );

            return this.CreateSuccessResult( AdviceOutcome.Default, builder );
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
                                (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration,
                                 existingConstructor.DeclaringType),
                                this ) );

                case OverrideStrategy.Ignore:
                    // Do nothing.
                    return this.CreateIgnoredResult( existingConstructor );

                case OverrideStrategy.Override:
                    var overriddenMethod = new OverrideConstructorTransformation(
                        this,
                        existingConstructor.ToRef(),
                        this._template.ForIntroduction( existingConstructor ),
                        this.Tags );

                    context.AddTransformation( overriddenMethod );

                    return this.CreateSuccessResult( AdviceOutcome.Override, existingConstructor );

                default:
                    throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
            }
        }
    }
}