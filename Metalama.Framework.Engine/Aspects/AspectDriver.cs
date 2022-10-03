﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Eligibility.Implementation;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Configuration;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// Executes aspects.
/// </summary>
internal class AspectDriver : IAspectDriver
{
    private readonly ReflectionMapper _reflectionMapper;
    private readonly IAspectClassImpl _aspectClass;
    private readonly CodeFixAvailability _codeFixAvailability;

    public IEligibilityRule<IDeclaration>? EligibilityRule { get; }

    public AspectDriver( IServiceProvider serviceProvider, IAspectClassImpl aspectClass, CompilationModel compilation )
    {
        this._reflectionMapper = serviceProvider.GetRequiredService<ReflectionMapperFactory>().GetInstance( compilation.RoslynCompilation );
        this._aspectClass = aspectClass;

        // We don't store the IServiceProvider because the AspectDriver is created during the pipeline initialization but used
        // during pipeline execution, and execution has a different service provider.

        // Introductions must have a deterministic order because of testing.
        var declarativeAdviceAttributes = aspectClass
            .TemplateClasses.SelectMany( c => c.GetDeclarativeAdvice( serviceProvider, compilation ) )
            .ToList();

        if ( declarativeAdviceAttributes.Count > 0 )
        {
            foreach ( var declarativeAdvice in declarativeAdviceAttributes )
            {
                var eligibilityBuilder = new EligibilityBuilder<IDeclaration>();

                ((DeclarativeAdviceAttribute) declarativeAdvice.AdviceAttribute!).BuildAspectEligibility(
                    eligibilityBuilder,
                    declarativeAdvice.Declaration );

                this.EligibilityRule = eligibilityBuilder.Build();
            }
        }

        // Determine the licensing abilities of the current aspect class.
        var licenseVerifier = serviceProvider.GetService<LicenseVerifier>();

        if ( licenseVerifier == null || licenseVerifier.VerifyCanApplyCodeFix( aspectClass ) )
        {
            this._codeFixAvailability = CodeFixAvailability.PreviewAndApply;
        }
        else
        {
            var designTimeConfiguration = serviceProvider.GetRequiredBackstageService<IConfigurationManager>().Get<DesignTimeConfiguration>();
            this._codeFixAvailability = designTimeConfiguration.HideUnlicensedCodeActions ? CodeFixAvailability.None : CodeFixAvailability.PreviewOnly;
        }
    }

    public AspectInstanceResult ExecuteAspect(
        IAspectInstanceInternal aspectInstance,
        string? layer,
        CompilationModel initialCompilationRevision,
        CompilationModel currentCompilationRevision,
        AspectPipelineConfiguration pipelineConfiguration,
        int pipelineStepIndex,
        int indexWithinType,
        CancellationToken cancellationToken )
    {
        var target = aspectInstance.TargetDeclaration.GetTarget( initialCompilationRevision );

        return target switch
        {
            ICompilation compilation => EvaluateAspectImpl( compilation ),
            INamedType type => EvaluateAspectImpl( type ),
            IMethod method => EvaluateAspectImpl( method ),
            IField field => EvaluateAspectImpl( field ),
            IProperty property => EvaluateAspectImpl( property ),
            IConstructor constructor => EvaluateAspectImpl( constructor ),
            IParameter parameter => EvaluateAspectImpl( parameter ),
            ITypeParameter genericParameter => EvaluateAspectImpl( genericParameter ),
            IEvent @event => EvaluateAspectImpl( @event ),
            INamespace ns => EvaluateAspectImpl( ns ),
            _ => throw new NotSupportedException( $"Cannot add an aspect to a declaration of type {target.DeclarationKind}." )
        };

        AspectInstanceResult EvaluateAspectImpl<T>( T targetDeclaration )
            where T : class, IDeclaration
        {
            if ( aspectInstance.IsSkipped )
            {
                // The aspect instance was skipped from a previous layer.
                return new AspectInstanceResult(
                    aspectInstance,
                    AdviceOutcome.Ignored,
                    ImmutableUserDiagnosticList.Empty,
                    ImmutableArray<ITransformation>.Empty,
                    ImmutableArray<IAspectSource>.Empty,
                    ImmutableArray<IValidatorSource>.Empty );
            }

            AspectInstanceResult CreateResultForError( Diagnostic diagnostic )
            {
                return new AspectInstanceResult(
                    aspectInstance,
                    AdviceOutcome.Error,
                    new ImmutableUserDiagnosticList( ImmutableArray.Create( diagnostic ), ImmutableArray<ScopedSuppression>.Empty ),
                    ImmutableArray<ITransformation>.Empty,
                    ImmutableArray<IAspectSource>.Empty,
                    ImmutableArray<IValidatorSource>.Empty );
            }

            cancellationToken.ThrowIfCancellationRequested();

            var serviceProvider = pipelineConfiguration.ServiceProvider;

            // Map the target declaration to the correct revision of the compilation model.
            targetDeclaration = initialCompilationRevision.Factory.GetDeclaration( targetDeclaration );

            if ( aspectInstance.Aspect is not IAspect<T> aspectOfT )
            {
                // TODO: should the diagnostic be applied to the attribute, if one exists?

                // Get the code model type for the reflection type so we have better formatting of the diagnostic.
                var interfaceType = this._reflectionMapper.GetTypeSymbol( typeof(IAspect<T>) ).AssertNotNull();

                var diagnostic =
                    GeneralDiagnosticDescriptors.AspectAppliedToIncorrectDeclaration.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (AspectType: this._aspectClass.ShortName, targetDeclaration.DeclarationKind, targetDeclaration, interfaceType) );

                return CreateResultForError( diagnostic );
            }

            var diagnosticSink = new UserDiagnosticSink(
                this._aspectClass.Project,
                pipelineConfiguration.CodeFixFilter,
                this._aspectClass.DisplayName,
                this._codeFixAvailability );

            var executionContext = new UserCodeExecutionContext(
                serviceProvider,
                diagnosticSink,
                UserCodeMemberInfo.FromDelegate( new Action<IAspectBuilder<T>>( aspectOfT.BuildAspect ) ),
                new AspectLayerId( this._aspectClass ),
                initialCompilationRevision,
                targetDeclaration );

            // Create the AdviceFactory.
            var adviceFactoryState = new AdviceFactoryState(
                serviceProvider,
                initialCompilationRevision,
                currentCompilationRevision,
                aspectInstance,
                diagnosticSink,
                pipelineConfiguration,
                executionContext,
                pipelineStepIndex,
                indexWithinType );

            var adviceFactory = new AdviceFactory(
                adviceFactoryState,
                aspectInstance.TemplateInstances.Count == 1 ? aspectInstance.TemplateInstances.Values.Single() : null,
                layer );

            // Prepare declarative advice.
            var declarativeAdvice = this._aspectClass.TemplateClasses
                .SelectMany( c => c.GetDeclarativeAdvice( serviceProvider, initialCompilationRevision ) )
                .ToList();

            // Create the AspectBuilder.
            var aspectBuilderState = new AspectBuilderState(
                serviceProvider,
                diagnosticSink,
                pipelineConfiguration,
                aspectInstance,
                adviceFactoryState,
                layer,
                cancellationToken );

            var aspectBuilder = new AspectBuilder<T>( targetDeclaration, aspectBuilderState, adviceFactory );

            adviceFactoryState.AspectBuilder = aspectBuilder;

            using ( SyntaxBuilder.WithImplementation( new SyntaxBuilderImpl( initialCompilationRevision, serviceProvider ) ) )
            {
                if ( !serviceProvider.GetRequiredService<UserCodeInvoker>()
                        .TryInvoke(
                            () =>
                            {
                                // Execute declarative advice.
                                foreach ( var advice in declarativeAdvice )
                                {
                                    ((DeclarativeAdviceAttribute) advice.AdviceAttribute.AssertNotNull()).BuildAdvice(
                                        advice.Declaration,
                                        advice.TemplateClassMember.Key,
                                        aspectBuilder );
                                }

                                if ( !aspectBuilder.IsAspectSkipped )
                                {
                                    aspectOfT.BuildAspect( aspectBuilder );
                                }
                            },
                            executionContext ) )
                {
                    aspectInstance.Skip();

                    return
                        new AspectInstanceResult(
                            aspectInstance,
                            AdviceOutcome.Error,
                            diagnosticSink.ToImmutable(),
                            ImmutableArray<ITransformation>.Empty,
                            ImmutableArray<IAspectSource>.Empty,
                            ImmutableArray<IValidatorSource>.Empty );
                }

                var aspectResult = aspectBuilderState.ToResult();

                if ( aspectResult.Outcome == AdviceOutcome.Error )
                {
                    aspectInstance.Skip();
                }
                else
                {
                    // Validators on the current version of the compilation must be executed now.

                    if ( !aspectResult.ValidatorSources.IsDefaultOrEmpty )
                    {
                        diagnosticSink.Reset();

                        var validationRunner = new ValidationRunner( pipelineConfiguration, aspectResult.ValidatorSources, CancellationToken.None );
                        validationRunner.RunDeclarationValidators( initialCompilationRevision, CompilationModelVersion.Current, diagnosticSink );

                        if ( !diagnosticSink.IsEmpty )
                        {
                            aspectResult = aspectResult.WithAdditionalDiagnostics( diagnosticSink.ToImmutable() );
                        }
                    }
                }

                return aspectResult;
            }
        }
    }
}